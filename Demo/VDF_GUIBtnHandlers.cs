using Indieteur.VDFAPI;
using System;
using System.Windows.Forms;

namespace Demo
{
    public partial class FrmDemoVdf : Form
    {
        private void btnAddRootNode_Click(object sender, EventArgs e)
        {
            tViewData.SelectedNode = addRootNode(DEFAULT_NODE_NAME, _vdfData);
        }

        private void btnAddNode_Click(object sender, EventArgs e)
        {
            if (addRemoveSaveButtonErrorHandling())
                return;
            tViewData.SelectedNode = addNodeToNode(DEFAULT_NODE_NAME,  _vdfData, _selectedToken, tViewData.SelectedNode);
        }

        private void btnAddKey_Click(object sender, EventArgs e)
        {
            if (addRemoveSaveButtonErrorHandling())
                return;
            tViewData.SelectedNode = createNewKey(DEFAULT_KEY_NAME, DEFAULT_KEY_VALUE, _selectedToken,  tViewData.SelectedNode);
        }


        private void btnDeleteNode_Click(object sender, EventArgs e)
        {
            if (addRemoveSaveButtonErrorHandling())
                return;
            deleteVdfNode(_selectedToken.Token as VdfNode, tViewData.SelectedNode); //We can be sure that the treenode we have selected is a VDFNode as the btnDeleteNode will not be enabled otherwise.
            if (tViewData.Nodes.Count == 0)
                resetGuiAndVariables(); //Make sure to reset the gui and the variables related to the selected node afterwards if there's nothing else to select.

        }
        private void btnDeleteKey_Click(object sender, EventArgs e)
        {
            if (addRemoveSaveButtonErrorHandling())
                return;
            deleteVdfKey(_selectedToken.Token as VdfKey, tViewData.SelectedNode); //We can be sure that the treenode we have selected is a VDFKey as the btnDeleteKey will not be enabled otherwise.
        } 

        private void btnRevertInfo_Click(object sender, EventArgs e)
        {
            GUIUpdate_RevertInfo(_selectedToken);
            
        }
        private void btnSaveInfo_Click(object sender, EventArgs e)
        {
            if (addRemoveSaveButtonErrorHandling())
                return;
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Name of Token cannot be empty or whitespace!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            updateTokenInfo(_selectedToken, tViewData.SelectedNode);
        }
    }
}
