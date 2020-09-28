using System;
using System.Collections.Generic;
using System.Linq;

namespace Indieteur.VDFAPI
{
    static class BaseTokenExtensionMethod
    {
        /// <summary>
        /// Finds a Node or a Key from a BaseToken collection by using the Name field.
        /// </summary>
        /// <param name="tokens">The collection of BaseToken to search through.</param>
        /// <param name="name">The name of the node that the method needs to find.</param>
        /// <param name="caseSensitive">Indicates if the name argument and the name of the BaseToken needs to be an exact match in terms of capitalization.</param>
        /// <param name="throwErrorIfNotFound">Throw an exception if the BaseToken could not be found. If false, method will return null instead.</param>
        /// <returns></returns>
        internal static BaseToken FindBaseToken(this IEnumerable<BaseToken> tokens, string name, bool caseSensitive = false, bool throwErrorIfNotFound = false)
        {
            var tokenIndex = tokens.FindBaseTokenIndex(name, caseSensitive, throwErrorIfNotFound);
            return tokenIndex == -1 ? null : tokens.ElementAt(tokenIndex);
        }

        /// <summary>
        /// Finds a Node or a Key from a BaseToken collection by using the Name field and return the index of the element if found.
        /// </summary>
        /// <param name="tokens">The collection of BaseToken to search through.</param>
        /// <param name="name">The name of the node that the method needs to find.</param>
        /// <param name="caseSensitive">Indicates if the name argument and the name of the BaseToken needs to be an exact match in terms of capitalization.</param>
        /// <param name="throwErrorIfNotFound">Throw an exception if the BaseToken could not be found. If false, method will return -1 if element could not be found.</param>
        /// <returns></returns>
        internal static int FindBaseTokenIndex(this IEnumerable<BaseToken> tokens, string name, bool caseSensitive = false, bool throwErrorIfNotFound = false)
        {
            var baseTokens = tokens.ToList();
            int tokensLength = baseTokens.Count(); //Store the length of our tokens list to a variable 
            if (!caseSensitive) //If CaseSensitve is set to false, then we convert the Name argument to lower case.
                name = name.ToLower();
            for (int i = 0; i < tokensLength; ++i) 
            {
                string tokenName; //This will store the name of the BaseToken that we are checking.
                BaseToken currentToken = baseTokens.ElementAt(i); //Cache the BaseToken that we are working with.
                tokenName = caseSensitive ? currentToken.Name : currentToken.Name.ToLower();

                if (tokenName == name) 
                    return i;
            }
            if (throwErrorIfNotFound) //We're done looping through our collection and we haven't found the currentToken. 
                throw new TokenNotFoundException(name + " has not been found in the collection!");
            return -1; //If throwError is set to false, then return -1 instead.
        }
    }

    public class TokenNotFoundException : Exception
    {
        public TokenNotFoundException(string message) : base(message) {}
    }
}
