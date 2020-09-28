using System;
using System.Collections.Generic;

namespace Indieteur.VDFAPI
{
    public static class NodesListExtensionMethod
    {
        /// <summary>
        /// Finds a node in a node collection by using the Name field.
        /// </summary>
        /// <param name="nodes">The collection of nodes to search through.</param>
        /// <param name="name">The name of the node that the method needs to find.</param>
        /// <param name="caseSensitive">Indicates if the name argument and the name of the node needs to be an exact match in terms of capitalization.</param>
        /// <param name="throwErrorIfNotFound">Throw an exception if the node could not be found. If false, method will return null instead.</param>
        /// <returns></returns>
        public static VdfNode FindNode(this IEnumerable<VdfNode> nodes, string name, bool caseSensitive = false, bool throwErrorIfNotFound = false)
        {
            BaseToken baseToken = nodes.FindBaseToken(name, caseSensitive, throwErrorIfNotFound); //Use the baseToken GetBaseToken extension method to search for the node.
            return (VdfNode) baseToken;
        }

        /// <summary>
        /// Finds a node in a node collection by using the Name field and returns the index of the node if found.
        /// </summary>
        /// <param name="nodes">The collection of nodes to search through.</param>
        /// <param name="name">The name of the node that the method needs to find.</param>
        /// <param name="caseSensitive">Indicates if the name argument and the name of the node needs to be an exact match in terms of capitalization.</param>
        /// <param name="throwErrorIfNotFound">Throw an exception if the node could not be found. If set to false, method will return -1 if node could not be found.</param>
        /// <returns></returns>
        public static int FindNodeIndex(this IEnumerable<VdfNode> nodes, string name, bool caseSensitive = false, bool throwErrorIfNotFound = false)
        {
            return nodes.FindBaseTokenIndex(name, caseSensitive, throwErrorIfNotFound); //Call the base method which finds the node for us.
        }

        /// <summary>
        /// Cleanly removes a node from the list of children of the parent node.
        /// </summary>
        /// <param name="nodes">The collection of nodes to search through.</param>
        /// <param name="name">The name of the node that the method needs to find.</param>
        /// <param name="caseSensitive">Indicates if the name argument and the name of the node needs to be an exact match in terms of capitalization.</param>
        /// <param name="fullRemovalFromTheVdfStruct">If true, the ParentVDFStructure property of the node will be set to null as well. Always set this to true if the node is a root node.</param>
        /// <param name="throwErrorIfNotFound">Throw an exception if the node could not be found.</param>
        /// <returns></returns>
        public static void CleanRemoveNode(this List<VdfNode> nodes, string name, bool caseSensitive = false, bool fullRemovalFromTheVdfStruct = false, bool throwErrorIfNotFound = false)
        {
            int i = nodes.FindBaseTokenIndex(name, caseSensitive, throwErrorIfNotFound); //Find our node from the nodes list by calling the FindBaseTokenIndex method and pass on the arguments.
            if (i == -1) //The FindBaseTokenIndex method will do the error handling for us. However, if the argument throwErrorIfNotFound is set to false, it wouldn't do that so what'll do is exit from this method if the func returns -1.
            {
                return;
            }

            var tNode = nodes[i];//cache our node.
            tNode.Parent = null; 
            nodes.RemoveAt(i);
            if (fullRemovalFromTheVdfStruct)
            {
                tNode.ParentVdfStructure = null;
            }
        }
    }

    public static class NodeExtensionMethod
    {
        /// <summary>
        /// Creates a copy of the node and its elements.
        /// </summary>
        /// <param name="node">The Node to be duplicated.</param>
        /// <param name="parent">Indicates which node will parent the duplicate node. Set it to null if you want the duplicate node to be a root node.</param>
        /// <param name="parentVdfStructure"></param>
        /// <returns></returns>
        public static VdfNode Duplicate(this VdfNode node, VdfNode parent, VdfData parentVdfStructure)
        {
            VdfNode newNode = new VdfNode(node.Name, parentVdfStructure, parent); //Create our clone node and set it's name to the name of the original node but set the parent to the one specified by the calling method.
           
            //Do a null check for the Nodes and Keys List of the original node. If they aren't null, loop through the elements of the lists and call the duplicate method for each elements. Make sure to set their parent to the clone node.
            if (node.Nodes != null)
            {
                foreach(VdfNode curNode in node.Nodes)
                {
                    newNode.Nodes.Add(curNode.Duplicate(newNode, parentVdfStructure));
                }
            }

            if (node.Keys != null)
            {
                foreach (VdfKey curKey in node.Keys)
                {
                    newNode.Keys.Add(curKey.Duplicate(newNode));
                }
            }

            return newNode; 
        }
        
        /// <summary>
        /// Moves the selected node to a new parent node/to the root. 
        /// </summary>
        /// <param name="node">The node to be moved.</param>
        /// <param name="newParent">The new parent of the node. NOTE: If you want the node to be a root node, set this to null.</param>
        public static void Migrate(this VdfNode node, VdfNode newParent)
        {
            if (node.Parent != null)
            {
                node.Parent.Nodes.Remove(node); 
                node.Parent = null; 
            }
            if (newParent != null)
            {
                node.Parent = newParent;
                newParent.Nodes.Add(node);
            }
        }

        /// <summary>
        /// Removes node from its parent and/or from the VDFDataStructure.
        /// </summary>
        /// <param name="node">The node to be removed.</param>
        /// <param name="throwErrorOnNoParent">Throw an error if the parent/parentVDFStructure property of the node is set to null.</param>
        /// <param name="fullRemovalFromTheVdfStruct">If true, the ParentVDFStructure property of the node will be set to null as well. Always set this to true if the node is a root node.</param>
        public static void RemoveNodeFromParent(this VdfNode node, bool throwErrorOnNoParent = false, bool fullRemovalFromTheVdfStruct = false)
        {
            if (node.Parent != null)
            {
                node.Parent.Nodes.Remove(node);
                node.Parent = null;
                if (fullRemovalFromTheVdfStruct) 
                    node.ParentVdfStructure = null;
            }
            else
            {
                if(node.ParentVdfStructure == null || !fullRemovalFromTheVdfStruct)
                {
                    if(throwErrorOnNoParent)
                        throw new NullReferenceException($"Node {node.Name} parent property is not set!");
                    return;
                }

                node.ParentVdfStructure.Nodes.Remove(node);
                node.ParentVdfStructure = null;
            }
        }
    }
}
