using Indieteur.VDFAPI;
using System.Windows.Forms;

namespace Demo
{
    public partial class FrmDemoVdf : Form
    {
        private void tViewData_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null) //If none is selected, update our GUI.
            {
                _selectedToken = null;
                GUIUpdate_NoneSelected();
                return;
            }
            _selectedToken = (TreeNodeVdfTag)e.Node.Tag; //We can be 99 percent sure that the tag of the node is a TreeNodeVDFTag so we can just cast it without doing any error checking.

            if (_selectedToken.TagType == TreeNodeVdfTag.Type.Node)
            {
                GUIUpdate_NodeSelected(); //Update our GUI Buttons
                GUIUpdate_LoadNodeInfo(_selectedToken.Token as VdfNode); //Load information about the node on our textboxes. We can safely assume that the token will be a node as the tagtype is set to node.
            }
            else
            {
                GUIUpdate_KeySelected();
                GUIUpdate_LoadKeyInfo(_selectedToken.Token as VdfKey); 
            }
        }
    }
}
