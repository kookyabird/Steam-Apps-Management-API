using System.Text;

namespace Indieteur.VDFAPI
{
    internal static class Helper
    {
        public static string UnformatString(string str)
        {
            char[] charofString = str.ToCharArray();
            StringBuilder sb = new StringBuilder(str.Length); //Create our stringbuilder and add offset it's size by 5.

            foreach(var c in charofString)
            {
                switch (c)
                {
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        break; //No need to append carriage return to the string. 
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '"':
                        sb.Append("\\\"");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns the horizontal tab character and appends it repeatedly for a set amount of times.
        /// </summary>
        /// <param name="repititions">Indicates how many times the tab character should be repeated.</param>
        /// <returns></returns>
        public static string Tabify (int repititions)
        {
            if (repititions <= 0) //If repititions is set to 0 or a negative number, we return an empty string.
                return "";
            StringBuilder sb = new StringBuilder(repititions); //Initialize our StringBuilder. We already know what the size of the resulting string would be equal to the number of times the tab will be appended to it.
            for (int i = 0; i < repititions; ++i) //Iterate the appending of the tab character until i is equals to repititions.
                sb.Append("\t");
            return sb.ToString(); //return the resulting character.
        }

        /// <summary>
        /// Gets the next char from an array after the given index.
        /// </summary>
        /// <param name="index">The index of the element to start from.</param>
        /// <param name="array">The array which contains the element.</param>
        /// <returns>The char value of the next element if it's found, or a null character.</returns>
        public static char GetNextChar(int index, char[] array) => array.Length <= index + 1 ? '\0' : array[index + 1];
    }
}
