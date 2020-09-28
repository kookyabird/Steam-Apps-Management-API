using System;
using System.Text;

namespace Indieteur.VDFAPI
{
    internal class VdfParser
    {
        #region Fields

        private const string SMALLEST_VDFDATA = "a{\n}";
        private readonly StringBuilder _sb = new StringBuilder();

        private readonly VdfData _vdfData;
        private int _characterCount = 1;
        private VdfNode _currentNode = null;
        private int _lineCounter = 1;
        private Mode _parserMode = Mode.None;
        private string _previousString;
        private int _readerPos = 0;
        private char[] _stream;

        #endregion

        #region Constructors

        public VdfParser(VdfData vdfData)
        {
            _vdfData = vdfData;
        }

        #endregion

        #region Methods

        public void ParseData(char[] stream)
        {
            _stream = stream;
            if(stream.Length < SMALLEST_VDFDATA.Length)
            {
                throw new VdfStreamException("Provided data is not a valid VDF Data structure!");
            }

            while(_readerPos < stream.Length)
            {
                var curChar = stream[_readerPos];

                // Newlines and escaped characters are handled the same regardless of mode.
                switch(curChar)
                {
                    case '\n':
                        endLine();
                        continue;
                    //NOTE: Documentation from Valve is ambiguous about whether or not you can escape a character anywere, or only in a token.
                    case '\\':
                        parseEscapedCharacter();
                        continue;
                }

                // Handle special modes.
                switch(_parserMode)
                {
                    case Mode.InsideComment:
                        iterateReaderPos();
                        continue;
                    case Mode.InsideBrackets:
                        if(curChar == ']') _parserMode = Mode.None;
                        iterateReaderPos();
                        continue;
                    case Mode.InsideDoubleQuotes when curChar == '"':
                        closeQuotedToken();
                        continue;
                    case Mode.InsideDoubleQuotes:
                        appendChar(curChar);
                        continue;
                    case Mode.None:
                    default:
                        break;
                }

                // Check for special characters to handle and change modes.
                switch(curChar)
                {
                    case '"':
                        openQuotedToken();
                        continue;
                    case '[':
                        enterBracket();
                        continue;
                    case '/' when Helper.GetNextChar(_readerPos, stream) == '/':
                        enterComment();
                        continue;
                    case '{':
                        enterNode();
                        continue;
                    case '}':
                        exitNode();
                        continue;
                    //NOTE: Spec lists only these whitespace chars, but maybe we should cover all Unicode whitespace?
                    case ' ':
                    case '\r':
                    case '\t':
                        processWhitespace();
                        continue;
                    default:
                        appendChar(curChar);
                        continue;
                }
            }

            if(_currentNode != null) // Didn't exit last node before EOF.
            {
                throw new VdfStreamException("\"}\" expected.", _lineCounter, _characterCount);
            }
        }

        private void appendChar(char c)
        {
            _sb.Append(c);
            iterateReaderPos();
        }

        private void closeQuotedToken()
        {
            _parserMode = Mode.None;
            tryNewTokenOrKey(true);
            iterateReaderPos();
        }

        private void endLine()
        {
            _lineCounter++;
            _characterCount = 0;
            _parserMode = Mode.None;
            // Newline is a delimeter for unquoted tokens.
            tryNewTokenOrKey(false);
            iterateReaderPos();
        }

        private void enterBracket()
        {
            _parserMode = Mode.InsideBrackets;
            tryNewTokenOrKey(false);
            iterateReaderPos();
        }

        private void enterComment()
        {
            _parserMode = Mode.InsideComment;

            // Check if we just finished a token since comments can touch unescaped tokens.
            tryNewTokenOrKey(false);

            // Move ahead 2 to avoid reprocessing the same character.
            iterateReaderPos(2);
        }

        private void enterNode()
        {
            if(_previousString == null) //Let's first check if the StringBuilder hasn't been flushed yet.
            {
                if(_sb.Length == 0)
                {
                    throw new VdfStreamException("Node name is set to null!", _lineCounter, _characterCount);
                }

                setPreviousStringToStringBuilder();
            }

            var newNode = new VdfNode(_previousString, _vdfData);

            _previousString = null; //We already processed the previousString and it's the name of our node.
            if(_currentNode != null) //Check if we are already inside another node.
            {
                //If we are, then the node we are creating is the children of our current node so let's set the parent reference of the new node to our current node.
                newNode.Parent = _currentNode;
                _currentNode.Nodes.Add(newNode); //Add new node to the nodes of the parent node
            }
            else
            {
                _vdfData.Nodes.Add(newNode); // If we aren't, it means that the node we are creating is a root node so we have to add the new node to the nodes list of this class.
            }

            _currentNode = newNode; // We are now inside a child node
            iterateReaderPos();
        }

        private void exitNode()
        {
            tryNewTokenOrKey(false);
            //If current node has parent, switch to working on the parent node, else we are working on the root node, and we switch to working on no node.
            _currentNode = _currentNode?.Parent;
            iterateReaderPos();
        }

        private void iterateReaderPos(int count = 1)
        {
            _readerPos += count;
            _characterCount += count;
        }

        private void openQuotedToken()
        {
            _parserMode = Mode.InsideDoubleQuotes;
            // New quoted token can be a delimeter for unquoted tokens.
            tryNewTokenOrKey(false);
            _sb.Clear();
            iterateReaderPos();
        }

        private void parseEscapedCharacter()
        {
            char nextChar = Helper.GetNextChar(_readerPos, _stream);

            //TODO: This is either unecessary, or missing from many other areas.
            // Check for EOF.
            if(nextChar == '\0') throw new VdfStreamException("Incomplete escape character detected!", _lineCounter, _characterCount);
            
            string parsedNextChar;
            switch(nextChar)
            {
                case 'n':
                    parsedNextChar = Environment.NewLine;
                    break;
                case 't':
                    parsedNextChar = "\t";
                    break;
                case '\\':
                    parsedNextChar = "\\";
                    break;
                case '"':
                    parsedNextChar = "\"";
                    break;
                default:
                    throw new VdfStreamException("Invalid escape character detected!", _lineCounter, _characterCount + 1);
            }
            
            _sb.Append(parsedNextChar);
            iterateReaderPos(2);
        }

        private void processWhitespace()
        {
            // Whitespace is a delimeter for unquoted tokens.
            tryNewTokenOrKey(false);
            iterateReaderPos();
        }

        private void setPreviousStringToStringBuilder()
        {
            _previousString = _sb.ToString();
            _sb.Clear();
        }

        //TODO: Clean this up
        private void tryNewTokenOrKey(bool mustHaveNewToken)
        {
            if(_previousString != null) // If we already have a string before this one that isn't part of another key or isn't the name of another node.
            {
                var newToken = ""; //This will contain the value of our new token.
                if(mustHaveNewToken && _sb.Length > 0) //If we are required to have a new token and the StringBuilder isn't empty.
                {
                    newToken = _sb.ToString(); //Set the newToken to the value of the StringBuilder.
                }
                //NOTE: Scenario of requiring a token and not finding one isn't covered.
                else if(!mustHaveNewToken)
                {
                    if(_sb.Length == 0) return;

                    newToken = _sb.ToString();
                }

                //Since we have an unhandled previousString, it means that the current token we are working on and the previous one is a key-value pair. If that's the case, then it needs to be inside a node so we check that.
                if(_currentNode == null)
                {
                    throw new VdfStreamException("Key-value pair must be inside a node!", _lineCounter, _characterCount);
                }

                VdfKey key = new VdfKey(_previousString, newToken, _currentNode); //key name first (previousString) before its value
                _currentNode.Keys.Add(key);
                _previousString = null; //We already know that the previous String is the name of the key and it has already been parsed, so set the referencing variable to null.
                _sb.Clear(); //We do the same with stringbuilder.
            }
            else
            {
                if(_sb.Length == 0)
                {
                    if(mustHaveNewToken)
                    {
                        throw new VdfStreamException("Node name or Key name is set to null!", _lineCounter, _characterCount);
                    }

                    return;
                }
                //We do not know if this string is a name of a node/key so we store it and proceed.
                setPreviousStringToStringBuilder();
            }
        }

        #endregion

        #region Nested type: Mode

        private enum Mode
        {
            InsideComment,
            InsideDoubleQuotes,
            InsideBrackets,
            None
        }

        #endregion
    }
}