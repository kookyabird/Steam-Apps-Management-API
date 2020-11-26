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
                        appendCharAndIterateReaderPos(curChar);
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
                        appendCharAndIterateReaderPos(curChar);
                        continue;
                }
            }

            if(_currentNode != null) // Didn't exit last node before EOF.
            {
                throw new VdfStreamException("\"}\" expected.", _lineCounter, _characterCount);
            }
        }

        private void appendCharAndIterateReaderPos(char c)
        {
            _sb.Append(c);
            iterateReaderPos();
        }

        private void closeQuotedToken()
        {
            _parserMode = Mode.None;
            tryNewToken(true);
            iterateReaderPos();
        }

        private void endLine()
        {
            _lineCounter++;
            _characterCount = 0;
            _parserMode = Mode.None;
            // Newline is a delimeter for unquoted tokens.
            tryNewToken(false);
            iterateReaderPos();
        }

        private void enterBracket()
        {
            _parserMode = Mode.InsideBrackets;
            tryNewToken(false);
            iterateReaderPos();
        }

        private void enterComment()
        {
            _parserMode = Mode.InsideComment;

            // Check if we just finished a token since comments can touch unescaped tokens.
            tryNewToken(false);

            // Move ahead 2 to avoid reprocessing the same character.
            iterateReaderPos(2);
        }

        private void enterNode()
        {
            // Check for node name.
            if(_previousString == null)
            {
                if(_sb.Length == 0) throw new VdfStreamException("No node name detected.", _lineCounter, _characterCount);
                storePreviousString();
            }

            var newNode = new VdfNode(_previousString, _vdfData);

            _previousString = null;

            if(_currentNode != null) // Check if we are already inside another node.
            {
                // Creating a child of our current node, so set the current node as parent.
                newNode.Parent = _currentNode;
                _currentNode.Nodes.Add(newNode); //Add new node to the nodes of the parent node
            }
            else // Creating a root node, so add to the nodes list.
            {
                _vdfData.Nodes.Add(newNode); 
            }

            // Switch to work on the new node.
            _currentNode = newNode;
            iterateReaderPos();
        }

        private void exitNode()
        {
            tryNewToken(false);
            // Switch to current node's parent, or no node if if there isn't.
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
            tryNewToken(false);
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
            tryNewToken(false);
            iterateReaderPos();
        }

        private void storePreviousString()
        {
            _previousString = _sb.ToString();
            _sb.Clear();
        }

        //TODO: Clean this up
        private void tryNewToken(bool mustHaveNewToken)
        {
            if(_previousString != null)
            {
                var newToken = "";
                if(mustHaveNewToken && _sb.Length > 0)
                {
                    newToken = _sb.ToString();
                }
                //NOTE: Scenario of requiring a token and not finding one isn't covered.
                else if(!mustHaveNewToken)
                {
                    if(_sb.Length == 0) return;

                    newToken = _sb.ToString();
                }

                // An unhandled previousString means the current and previous tokens are a key-value pair, and must be inside a node.
                if(_currentNode == null)
                {
                    throw new VdfStreamException("Key-value pair must be inside a node.", _lineCounter, _characterCount);
                }

                VdfKey key = new VdfKey(_previousString, newToken, _currentNode);
                _currentNode.Keys.Add(key);
                _previousString = null;
                _sb.Clear();
            }
            else
            {
                if(_sb.Length == 0)
                {
                    if(mustHaveNewToken)
                    {
                        throw new VdfStreamException("Node name or Key name is set to null.", _lineCounter, _characterCount);
                    }

                    return;
                }
                // We do not know if this string is a name of a node/key so we store it and proceed.
                storePreviousString();
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