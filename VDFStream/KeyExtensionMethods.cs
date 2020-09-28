﻿using System;
using System.Collections.Generic;

namespace Indieteur.VDFAPI
{
    public static class KeyListExtensionMethods 
    {
        /// <summary>
        ///  Finds a Key in a Key collection by using the Name field. 
        /// </summary>
        /// <param name="keys">The key collection that contains the key that the method will search for.</param>
        /// <param name="name">The name of the key that the method will search for.</param>
        /// <param name="caseSensitive">Indicates if the name argument and the name of the node needs to be an exact match in terms of capitalization.</param>
        /// <param name="throwErrorIfNotFound">Throw an exception if the key could not be found. If false, method will return null instead.</param>
        /// <returns></returns>
        public static VdfKey FindKey(this IEnumerable<VdfKey> keys, string name, bool caseSensitive = false, bool throwErrorIfNotFound = false)
        {
            BaseToken baseToken = keys.FindBaseToken(name, caseSensitive, throwErrorIfNotFound);
            return (VdfKey) baseToken;
        }
    
        /// <summary>
        ///  Finds a Key in a Key collection by using the Name field and returns the index of the key if found.
        /// </summary>
        /// <param name="keys">The key collection that contains the key that the method will search for.</param>
        /// <param name="name">The name of the key that the method will search for.</param>
        /// <param name="caseSensitive">Indicates if the name argument and the name of the node needs to be an exact match in terms of capitalization.</param>
        /// <param name="throwErrorIfNotFound">Throw an exception if the key could not be found.  If set to false, method will return -1 if key could be found.</param>
        /// <returns></returns>
        public static int FindKeyIndex(this IEnumerable<VdfKey> keys, string name, bool caseSensitive = false, bool throwErrorIfNotFound = false)
        {
            return keys.FindBaseTokenIndex(name, caseSensitive, throwErrorIfNotFound); //Call the base method which finds the key for us.
        }

        /// <summary>
        /// Cleanly removes a key from the list of children of the parent node.
        /// </summary>
        /// <param name="keys">The collection of keys to search through.</param>
        /// <param name="name">The name of the key that the method needs to find.</param>
        /// <param name="caseSensitive">Indicates if the name argument and the name of the key needs to be an exact match in terms of capitalization.</param>
        /// <param name="throwErrorIfNotFound">Throw an exception if the key could not be found.</param>
        /// <returns></returns>
        public static void CleanRemoveKey(this List<VdfKey> keys, string name, bool caseSensitive = false, bool throwErrorIfNotFound = false)
        {
            int i = keys.FindBaseTokenIndex(name, caseSensitive, throwErrorIfNotFound); //Find our node from the nodes list by calling the FindBaseTokenIndex method and pass on the arguments.
            if (i == -1) //The FindBaseTokenIndex method will do the error handling for us. However, if the argument throwErrorIfNotFound is set to false, it wouldn't do that so what'll do is exit from this method if the func returns -1.
                return;
            VdfKey tKey = keys[i];//cache our node.
            tKey.Parent = null; 
            keys.RemoveAt(i); 
        }
    }
    
    public static class KeyExtensionMethods
    {
        /// <summary>
        /// Creates a copy of the key.
        /// </summary>
        /// <param name="key">The Key to be duplicated.</param>
        /// <param name="parent">The Node that will parent the key.</param>
        /// <returns></returns>
        public static VdfKey Duplicate(this VdfKey key, VdfNode parent)
        {
            if (parent == null) 
                throw new ArgumentNullException(nameof(parent));
            return new VdfKey(key.Name, key.Value, parent); //Return a new instance of a key class. Copy the name and value of the original key but set the parent of the clone to the parent argument passed to this method.
        }

        /// <summary>
        /// Moves the key to another node while making sure that the Parent property is set correctly and that the key is removed from the previous parent's key list and added to the new parent's key list.
        /// </summary>
        /// <param name="key">The key to be moved.</param>
        /// <param name="newParent">The new parent of the key.</param>
        public static void Migrate(this VdfKey key, VdfNode newParent)
        {
            if (newParent == null)
                throw new ArgumentNullException(nameof(newParent));
            if (key.Parent == null) 
                throw new NullReferenceException($"The Parent property of key {key.Name} is set to null!");
            key.Parent.Keys.Remove(key); 
            key.Parent = newParent;
            if (newParent.Keys == null) //If the newParent node's keys list is not yet created, we will have to instantiate it ourselves.
                newParent.Keys = new List<VdfKey>();
            newParent.Keys.Add(key);
        }

        /// <summary>
        /// Removes key from its parent.
        /// </summary>
        /// <param name="key">The key to be removed.</param>
        /// <param name="throwErrorOnNoParent">Throw an error if the parent property of the key is set to null.</param>
        public static void RemoveKeyFromNode(this VdfKey key, bool throwErrorOnNoParent = false)
        {
            if (key.Parent != null)
            {
                key.Parent.Keys.Remove(key);
                key.Parent = null; 
            }
            else if (key.Parent == null && throwErrorOnNoParent) 
                throw new NullReferenceException("Key " + key.Name + " parent property is not set!");
        }
    }
}
