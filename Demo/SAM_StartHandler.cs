using System;
using System.IO;
using System.Windows.Forms;
using Indieteur.SAMAPI;

namespace Demo
{
    partial class FrmMainDemo
    {
        void loadSamapi() //Called once only when the form is first loaded.
        {
            try
            {
                _sam = new SteamAppsManager(); //Load our steam app manager. The library should be able to automatically find our steam directory.
            }
            catch (Exception ex)
            {
                NullReferenceException nullRefException = ex as NullReferenceException;  //The library will throw a null reference exception when it cannot find installation directory.
                if (nullRefException != null) 
                {
                    MessageBox.Show("Steam installation folder was not found or is invalid! Please provide the path to the Steam installation folder.", "Steam App Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DialogResult dResult = folderBrowse.ShowDialog(); //Show the dialog which will allow the user to browse the steam installation directory manually.
                    if (dResult == DialogResult.Cancel)//If the user aborts the operation...
                    {
                        OnLoadFailed(); 
                        return;
                    }
                    tryLoadManual(folderBrowse.SelectedPath); //If the user selects a directory...
                }
                else
                {
                    throw; //For other errors, throw it back and show it to the user.
                }
            }
        }

        void tryLoadManual(string path)
        {
            bool loadOk = false; //This variable will tell the loop if the steam library was loaded successfully.
            while (!loadOk) //Loop until we have successfully loaded the library or when the user aborts the operation.
            {
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) 
                {
                    path = OnInvalidPathSelected(); //Call the method which will show the user the browse dialog.
                    continue; //Iterate again so that the check path is valid statement will be called again.
                }
                try
                {
                    _sam = new SteamAppsManager(path); //Try to load the steam library again but this time with the path that was provided.
                    loadOk = true; //If load was successful, there should be no errors thus this piece of code should be reached which will exit the loop.
                }
                catch
                {
                    path = OnInvalidPathSelected(); //Just try again if the given path is not the steam library installation location.
                }
            }
        }

        string OnInvalidPathSelected()
        {
            MessageBox.Show("Invalid path selected! Please try again.", "Steam App Manager", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            DialogResult dResult = folderBrowse.ShowDialog();
            if (dResult == DialogResult.Cancel) //If user presses the cancel or close button then abort the load and show the VDF Demo form instead.
            {
                OnLoadFailed();
                return null;
            }
            return folderBrowse.SelectedPath;
        }
        void OnLoadFailed()
        {
            MessageBox.Show("Unable to locate Steam Installation folder. Disabling Steam Apps Manager library and launching VDF Demo instead.", "Steam App Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Program.FailedStartMain = true; //Since we cannot launch the VDF Demo form on its own (The form will close if we close this form as it is a child form), we will need to launch the VDF demo form from the main method which launched this form.
            Close();  
        }
    }
}
