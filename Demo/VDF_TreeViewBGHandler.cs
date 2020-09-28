using Indieteur.VDFAPI;
using System;
using System.Windows.Forms;

namespace Demo
{
    public partial class FrmDemoVdf : Form
    {
        void loadVdfDataToTreeView(VdfData vdfData)
        {
            tViewData.Nodes.Clear();
            if (vdfData.Nodes != null) 
            {
                foreach (VdfNode vNode in vdfData.Nodes) 
                {
                    loadVdfNodeToTreeView(vNode);
                }
            }
            if (tViewData.Nodes.Count > 0) //Make sure to expand our first node if it exists.
                tViewData.Nodes[0].Expand();
        }

        void loadVdfNodeToTreeView(VdfNode node, TreeNode parent = null)
        {
            TreeNode newTreeNode = new TreeNode(node.Name); 
            newTreeNode.Tag = new TreeNodeVdfTag(TreeNodeVdfTag.Type.Node, node); //Set the tag of our new TreeNode to our VDFNode using our TreeNodeVDFTag which will help us identify which BaseToken we have.
            if (node.Keys != null)
            {
                foreach (VdfKey key in node.Keys)
                {
                    loadVdfKeyToTreeView(key, newTreeNode); //Loop through each child keys of our VDFNode and add them to our newTreeNode child nodes recursively.
                }
            }
            if (node.Nodes != null) 
            {
                foreach (VdfNode childNode in node.Nodes)
                {
                    loadVdfNodeToTreeView(childNode, newTreeNode);
                }
            }


            if (parent == null) //Check if the parent argument is set to null. If it is, it means that our node is a root node. Add them directly to the Treeview. Otherwise, add them to the parent TreeViewNode.
                tViewData.Nodes.Add(newTreeNode);
            else
                parent.Nodes.Add(newTreeNode);
        }

        void loadVdfKeyToTreeView(VdfKey key, TreeNode parent)
        {
            if (parent == null) //Key must have a parent node. Throw an error if parent is set to null.
                throw new ArgumentNullException(nameof(parent));
            TreeNode newTreeNode = new TreeNode(key.Name); 
            newTreeNode.Tag = new TreeNodeVdfTag(TreeNodeVdfTag.Type.Key, key);
            parent.Nodes.Add(newTreeNode); 
        }

        TreeNode addRootNode(string name, VdfData vdfdata)
        {
            VdfNode newNode; //Declare our newNode type VDFNode variable which we will be used by our CreateNewNode to pass back our newly created VDFNode.
            TreeNode newTreeNode = createNewNode(name, vdfdata, null, out newNode); //Call the method which will do the job of creating a TreeNode and VDFNode for us.
            if (vdfdata.Nodes == null) 
                throw new NullReferenceException("Nodes list of VDFData class is set to null!");
            vdfdata.Nodes.Add(newNode); 
            tViewData.Nodes.Add(newTreeNode);
            return newTreeNode;
        }

        TreeNode addNodeToNode(string name, VdfData parentVdfDataStructure, TreeNodeVdfTag treeNodeTag, TreeNode nodeSelected)
        {
            TreeNode newTreeNode; 
            if (treeNodeTag.TagType == TreeNodeVdfTag.Type.Node) //If we have the node itself selected on our Treeview, we just need to call the AddNodeToNode method directly.
            {
                newTreeNode = addNodeToNode(name, nodeSelected, parentVdfDataStructure, treeNodeTag.Token as VdfNode);
            }
            else
            {
                //We will need to retrieve the parent of this key for this one if the key is the one that is selected. 
                if (nodeSelected.Parent == null) //Make sure that the parent of the TreeNode is set to another TreeNode
                    throw new NullReferenceException("TreeNode " + nodeSelected.Text + " parent property is not set!");
                VdfKey selectedKey = treeNodeTag.Token as VdfKey; //Cast our token to key first.
                if (selectedKey.Parent == null) 
                    throw new NullReferenceException("Parent property of key " + selectedKey.Name + " is not set!");
                VdfNode parentNode = selectedKey.Parent; 
                newTreeNode = addNodeToNode(name, nodeSelected.Parent, _vdfData, parentNode);
            }
            return newTreeNode;
        }

        TreeNode addNodeToNode(string name, TreeNode parent, VdfData parentVdfDataStructure, VdfNode nodeParent)
        {
            TreeNode newTreeNode = createNewNode(name, parentVdfDataStructure, nodeParent, out VdfNode newNode);
            if (nodeParent.Nodes == null) 
                throw new NullReferenceException("Nodes list of VDFData class is set to null!");
            nodeParent.Nodes.Add(newNode);
            parent.Nodes.Add(newTreeNode); 
            return newTreeNode;
        }

        TreeNode createNewNode(string name, VdfData parentVdfStructure, VdfNode parentNode, out VdfNode newNode)
        {
            newNode = new VdfNode(name, parentVdfStructure, parentNode); //Create our new VDFNode first which will be referenced by our newNode argument (which will be passed back to the calling method.)
            TreeNode newTreeNode = new TreeNode(name); 
            newTreeNode.Tag = new TreeNodeVdfTag(TreeNodeVdfTag.Type.Node, newNode); //Set the tag to our newNode. Make sure to use VDFTag.
            return newTreeNode; 
        }


        TreeNode createNewKey(string name, string value, TreeNodeVdfTag treeNodeTag, TreeNode nodeSelected)
        {
            TreeNode newTreeNode;
            if (treeNodeTag.TagType == TreeNodeVdfTag.Type.Node) //If the tag type of our selected tree node is a VDFNode. Then it's quite easy to create a new key for us. Just need to call the CreateNewKey method.
            {
                newTreeNode = createNewKey(name, value, treeNodeTag.Token as VdfNode, nodeSelected);
            }
            else
            {
                //We will need to retrieve the parent of this key for this one if the key is the one that is selected. 
                if (nodeSelected.Parent == null) //Make sure that the parent of the TreeNode is set to another TreeNode
                    throw new NullReferenceException("TreeNode " + nodeSelected.Text + " parent property is not set!");
                VdfKey selectedKey = treeNodeTag.Token as VdfKey; //Cast our token to key first.
                if (selectedKey.Parent == null) 
                    throw new NullReferenceException("Parent property of key " + selectedKey.Name + " is not set!");
                VdfNode parentNode = selectedKey.Parent;
                newTreeNode = createNewKey(name, value, parentNode, nodeSelected.Parent );
            }
            return newTreeNode;
        }
        TreeNode createNewKey(string name, string value, VdfNode parent, TreeNode parentTreeNode)
        {
            if (parent.Keys == null) 
                throw new NullReferenceException("Keys list of " + parent.Name + " node is set to null!");
            VdfKey newKey = new VdfKey(name, value, parent); //Create our key with the values passed on to us as arguments.
            parent.Keys.Add(newKey); 
            TreeNode newTreeNode = new TreeNode(name); 
            newTreeNode.Tag = new TreeNodeVdfTag(TreeNodeVdfTag.Type.Key, newKey); //Store our VDFKey on the tag property of our newTreeNode. Make sure to use the TreeNodeVDFTag.
            parentTreeNode.Nodes.Add(newTreeNode); 
            return newTreeNode;
        }

        void deleteVdfNode(VdfNode nodeToDelete, TreeNode treeNodeToDelete)
        {
            if (nodeToDelete.Parent == null) //If Parent property of the nodeToDelete is null then most likely we are a root node.
            {
                if (nodeToDelete.ParentVdfStructure == null) 
                    throw new NullReferenceException("ParentVDFStructure property of Node " + nodeToDelete.Name + " is not set!");
                nodeToDelete.RemoveNodeFromParent(fullRemovalFromTheVdfStruct: true);
                tViewData.Nodes.Remove(treeNodeToDelete); //No need to do checks for our TreeNode as we can most certainly assume that our TreeNode is a root node as well.
            }
            else //If the nodeToDelete's parent property is set to another VDFNode then just remove it from the child nodes list of the parent. Do the same for the treenode.
            {
                nodeToDelete.RemoveNodeFromParent();
                treeNodeToDelete.Parent.Nodes.Remove(treeNodeToDelete);
            }
           
        }

        void deleteVdfKey(VdfKey keyToDelete, TreeNode treeNodeToDelete)
        {
            if (keyToDelete.Parent == null) 
                throw new NullReferenceException("Parent property of Key " + keyToDelete.Name + " is not set!");
            keyToDelete.RemoveKeyFromNode();
            treeNodeToDelete.Parent.Nodes.Remove(treeNodeToDelete);
        }


        void updateTokenInfo(TreeNodeVdfTag treeNodeTag, TreeNode treeNodeToUpdate)
        {
            if (treeNodeTag.TagType == TreeNodeVdfTag.Type.Key) //Call the correct function by checking the type of token that we have.
                updateKeyInfo(txtName.Text, txtValue.Text, (VdfKey)treeNodeTag.Token, treeNodeToUpdate);
            else
                updateNodeInfo(txtName.Text, (VdfNode)treeNodeTag.Token, treeNodeToUpdate);
        }

        void updateNodeInfo(string name, VdfNode nodeToUpdate, TreeNode treeNodeToUpdate)
        {
            if (name != nodeToUpdate.Name) //To prevent initializing a new string. Check if Name is just the same as the one on the node.
            {
                nodeToUpdate.Name = name;
                treeNodeToUpdate.Text = name;
            }
        }

        void updateKeyInfo(string name, string value, VdfKey keyToUpdate, TreeNode treeNodeToUpdate)
        {
            if (name != keyToUpdate.Name) //To prevent initializing a new string. Check if Name is just the same as the one on the node.
            {
                keyToUpdate.Name = name;
                treeNodeToUpdate.Text = name;
            }
            if (value != keyToUpdate.Value) //Check if the value string is just the same on the VDFKey instance.
                keyToUpdate.Value = value;
        }

    }
}
