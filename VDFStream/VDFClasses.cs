using System;
using System.Collections.Generic;
using System.Text;

namespace Indieteur.VDFAPI
{
    /// <summary>
    /// Contains keys which stores information as well as child nodes.
    /// </summary>
    public class VdfNode : BaseToken
    {
        public List<VdfKey> Keys { get; internal set; }
        public List<VdfNode> Nodes { get; private set; }

        /// <summary>
        /// The parent VDF Data class instance of this node.
        /// </summary>
        public VdfData ParentVdfStructure { get; internal set; }

        public VdfNode()
        {
            initializeKeysAndNodesList();
        }
        
        public VdfNode(string name, VdfData parentVdfStructure, VdfNode parent = null)
        {
            Name = name;
            Parent = parent;
            ParentVdfStructure = parentVdfStructure;
            initializeKeysAndNodesList();
        }

        private void initializeKeysAndNodesList()
        {
            Keys = new List<VdfKey>();
            Nodes = new List<VdfNode>();
        }

        /// <summary>
        /// Creates a VDF parsable string which contains this node and its children (Keys and Nodes).
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(Delimiters.SystemDefault); //Call the other ToString overload method, pass on a default value for the delimiter argument.
        }

        /// <summary>
        /// Creates a VDF string which contains this node and its children (Keys and Nodes).
        /// </summary>
        /// <param name="delimiter">Indicates the delimiter to be appended after the name of the node, the curly brackets and the key-value pair.</param>
        /// <param name="tabLevel">Indicates how many tab characters should be appended at the beginning of the name of the node, the curly brackets and the key-value pair.</param>
        /// <returns></returns>
        public string ToString(string delimiter, int tabLevel = 0)
        {
            string tab = (tabLevel > 0) ? Helper.Tabify(tabLevel) : ""; //Append horizontal tab(s) at the beginning of our strings.
            
            StringBuilder sb = new StringBuilder(tab + "\"" + Helper.UnformatString(Name) + "\""); //Begin building our string by starting with the name of our VDFData

            sb.Append(delimiter + tab + "{" + delimiter); // Append the delimiter and then the '{' character which tells us that we are within the contents of the node and then another delimiter
            if (tabLevel >= 0)
            {
                ++tabLevel; //Make sure to increase the TabLevel if it isn't set to a negative number.
            }

            if (Keys != null)
            {
                foreach (var key in Keys) //Append all the keys under this node to the string
                {
                    sb.Append(key.ToString(tabLevel) + delimiter); //Append the string value of the single key element. We must also make sure that the string returned has the correct amount of tabs at the beginning.
                }
            }

            if (Nodes != null)
            {
                foreach (var node in Nodes) //Append all the nodes under this node and their children to the string
                {
                    sb.Append(node.ToString(delimiter, tabLevel) + delimiter); //We must make sure that the child nodes have the same styling as their parent node so pass on the delimiter and the tab level.
                }
            }

            sb.Append(tab + "}"); //Close off our node with '}'. Also, make sure that the correct number of tabs is appended before the closing curly brackets.
            return sb.ToString(); 
        }
    }

    /// <summary>
    /// A Key-Value pair found inside a node.
    /// </summary>
    public class VdfKey : BaseToken
    {
        public string Value { get; set; }

        public VdfKey(string name, string value, VdfNode parent)
        {
            Name = name ?? throw new VdfStreamException("Name of the Key cannot be Null!");
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Value = value;
        }
        
        /// <summary>
        /// Creates a VDF key string from the name and value field.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(0);  //Call the other ToString overload method, the default TabLevel is 0.
        }
    
        /// <summary>
        /// Creates a VDF key string from the name and value field and appends tab character(s) at the beginning of the string.
        /// </summary>
        /// <param name="tabLevel">Indicates how many times should a tab character be added. Set this to 0 or a negative number if you don't want to append a tab character.</param>
        /// <returns></returns>
        public string ToString(int tabLevel)
        {
            string tab = (tabLevel > 0) ? Helper.Tabify(tabLevel) : ""; //If tab level is greater than 0 then we call the tabify helper method if not, just set the tab string variable to "".
            return tab + "\"" + Helper.UnformatString(Name) + "\" \"" + Helper.UnformatString(Value) + "\""; //Make sure to use the correct format for the Name and Value variables.
        }
    }

    public abstract class BaseToken
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => _name = value ?? throw new VdfStreamException("Name cannot be Null!");
        }

        public VdfNode Parent { get; internal set; }
    }
}
