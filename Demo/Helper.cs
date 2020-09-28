using System.Windows.Forms;


namespace Demo
{

    public partial class FrmDemoVdf : Form
    {
        //default values
        const string DEFAULT_NODE_NAME = "New Node";
        const string DEFAULT_KEY_NAME = "New Key";
        const string DEFAULT_KEY_VALUE = "New Value"; 

       void resetGuiAndVariables()
        {
            GUIUpdate_NoneSelected(); //Make sure to reset our GUI
            _selectedToken = null; 
        }
        
        bool addRemoveSaveButtonErrorHandling()
        {
            if (tViewData.SelectedNode == null)
            {
                MessageBox.Show("Update Token request failed! SelectedNode property of tViewData is not set!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GUIUpdate_NoneSelected();
                return true;
            }
            if (_selectedToken == null)
            {
                MessageBox.Show("Update Token request failed! SelectedToken variable of frmDemo is not set!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                GUIUpdate_NoneSelected();
                return true;
            }
            return false;
        }
    }
}
