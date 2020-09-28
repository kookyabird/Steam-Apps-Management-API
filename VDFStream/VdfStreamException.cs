using System;

namespace Indieteur.VDFAPI
{
    /// <summary>
    /// Exception thrown by the library.
    /// </summary>
    public class VdfStreamException : Exception
    {
        /// <summary>
        /// The Line where the error was found. The minimum value is 1.
        /// </summary>
        public int Line { get; }
        /// <summary>
        /// The position of the error causing character in the line. The minimum value is 1.
        /// </summary>
        public int Character { get; }

        public VdfStreamException(string message) : base(message) { }
        
        public VdfStreamException(string message, int line) : base("Line: " + line + ". " + message)
        {
            Line = line;
        }

        public VdfStreamException(string message, int line, int character) : base("Line: " + line + ". Character: " + character + ". " + message)
        {
            Line = line;
            Character = character;
        }
    }
}