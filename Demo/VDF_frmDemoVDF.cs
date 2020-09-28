using System;
using System.Windows.Forms;
using Indieteur.VDFAPI;
using Indieteur.SAMAPI;

namespace Demo
{
    public partial class FrmDemoVdf : Form
    {
        VdfData _vdfData = new VdfData(); //Instance of our VDFData.
        TreeNodeVdfTag _selectedToken;
        public FrmDemoVdf()
        {
            InitializeComponent();
           
        }

     

        private void frmDemo_Load(object sender, EventArgs e)
        {
            string steamdir = SteamAppsManager.GetSteamDirectory(); //Set the initial directory of the openFileDialog control by calling the GetSteamDirectory static method from the SteamAppsMan class. It'll return empty if Steam Directory doesn't exist. 
            if (!string.IsNullOrWhiteSpace(steamdir)) 
                steamdir += "\\steamapps";
            openFileDialog.InitialDirectory = steamdir;
        }

        private void btnLoadfromFile_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = openFileDialog.ShowDialog(this); //Show our open file dialog and store its result on a variable.
            string fileName = openFileDialog.FileName; //Cache our filename as we are going to reset the dialogbox
            clearDialogBoxes();
            if (dialogResult == DialogResult.Cancel) 
                return;
            resetGuiAndVariables(); 
            _vdfData.LoadDataFromFile(fileName); 
            loadVdfDataToTreeView(_vdfData); 
        }

        private void btnSaveToFile_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = saveFileDialog.ShowDialog(this); //Show our save file dialog and store its result on a variable.
            string fileName = saveFileDialog.FileName; //Cache our filename as we are going to reset the dialogbox
            clearDialogBoxes();
            if (dialogResult == DialogResult.Cancel) 
                return;
            _vdfData.SaveToFile(fileName, true);

        }

        
    }
}
