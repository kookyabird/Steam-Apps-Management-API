using Indieteur.VDFAPI;
using System.Windows.Forms;

namespace Demo
{
    public partial class FrmDemoVdf : Form
    {
        void clearDialogBoxes()
        {
            openFileDialog.FileName = "";
            saveFileDialog.FileName = "";
        }

        void GUIUpdate_NoneSelected()
        {
            GUIUpdate_ClearInfo();
            groupInfo.Enabled = false;
            GUIUpdate_SetTextNameEnabled(false);
            GUIUpdate_SetTextValueEnabled(false);
            btnSaveInfo.Enabled = false;
            btnRevertInfo.Enabled = false;
            btnAddKey.Enabled = false;
            btnAddNode.Enabled = false;
            btnDeleteKey.Enabled = false;
            btnDeleteNode.Enabled = false;
        }

        void GUIUpdate_NodeSelected()
        {
            GUIUpdate_TokenSelected();
            btnDeleteKey.Enabled = false;
            btnDeleteNode.Enabled = true;
            GUIUpdate_SetTextValueEnabled(false);
        }
        void GUIUpdate_KeySelected()
        {
            GUIUpdate_TokenSelected();
            btnDeleteKey.Enabled = true;
            btnDeleteNode.Enabled = false;
            GUIUpdate_SetTextValueEnabled(true);
        }

        void GUIUpdate_TokenSelected()
        {
            GUIUpdate_ClearInfo();
            groupInfo.Enabled = true;
            GUIUpdate_SetTextNameEnabled(true);
            btnSaveInfo.Enabled = true;
            btnRevertInfo.Enabled = true;
            btnAddKey.Enabled = true;
            btnAddNode.Enabled = true;
        }

        void GUIUpdate_SetTextValueEnabled(bool enabled)
        {
            lblValue.Enabled = enabled;
            txtValue.Enabled = enabled;
        }
        void GUIUpdate_SetTextNameEnabled(bool enabled)
        {
            lblName.Enabled = enabled;
            txtName.Enabled = enabled;
        }

        void GUIUpdate_ClearInfo()
        {
            txtName.Text = "";
            txtValue.Text = "";
        }
        void GUIUpdate_RevertInfo (TreeNodeVdfTag token)
        {
            if (token.TagType == TreeNodeVdfTag.Type.Key)
            {
                GUIUpdate_LoadKeyInfo(token.Token as VdfKey);
            }
            else
            {
                GUIUpdate_LoadNodeInfo(token.Token as VdfNode);
            }
        }
        void GUIUpdate_LoadNodeInfo(VdfNode node)
        {
            txtName.Text = node.Name;
        }
        void GUIUpdate_LoadKeyInfo(VdfKey key)
        {
            txtName.Text = key.Name;
            txtValue.Text = key.Value;
        }
    }
}
