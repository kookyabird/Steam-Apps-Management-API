using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Demo
{
    //This file handles all the events associated with the GUI buttons.
    partial class FrmMainDemo
    {
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            SAM_Refresh();
        }
        private void btnStartWatchEvents_Click(object sender, EventArgs e)
        {
            startStopWatchEvents();
        }
        private void btnOpenVDFEditor_Click(object sender, EventArgs e)
        {
            showVdfDemo();
        }
        private void btnOpenFileExplore_Click(object sender, EventArgs e)
        {
            openDir(txtInstallDir.Text);
        }
        private void btnLaunchApp_Click(object sender, EventArgs e)
        {
            if (lstApps.SelectedItem != null)
                runApp(lstApps.SelectedItem as ListBoxItem);
        }
        private void btnExitApp_Click(object sender, EventArgs e)
        {
            if (lstApps.SelectedItem != null)
                stopApp(lstApps.SelectedItem as ListBoxItem);
        }


        void stopApp(ListBoxItem lbi)
        {
            lbi.AssociatedApp.RunningProcess.CloseMainWindow(); //Change this to kill or KillProcessAndChildren instead of CloseMainWindow for an immediate termination of process.
        }
        void runApp(ListBoxItem lbi)
        {
            try
            {
                lbi.AssociatedApp.Launch();
            }
            catch
            {
                //If we encountered an error, let our user know.
                MessageBox.Show("An error has occured when launching application " + lbi.Name, "Steam Apps Management", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }
        }

        void openDir(string path) 
        {
            if (!Directory.Exists(path)) 
                MessageBox.Show("Directory " + path + " does not exist!", "Steam Apps Management", MessageBoxButtons.OK, MessageBoxIcon.Error);
            try
            {
                Process.Start(path); //If it does, try opening it using the default file explorer.
            }
            catch
            {
              
                MessageBox.Show("An error has occured when opening directory " + path , "Steam Apps Management", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void showVdfDemo() //This method is called when the Show VDF Editor button is pressed.
        {
            FrmDemoVdf frmVdf = new FrmDemoVdf();
            frmVdf.Show(this);
        }

        void SAM_Refresh() //This method is called when the Refresh Button is pressed.
        {
            if (tmrEvents.Enabled) //stop our timer first before we do anything.
                tmrEvents.Stop();
            _sam.Refresh(); //Perform the refresh.
            GUI_Reset(); 
            loadSteamAppsToListBox(); //Reload data to list box.
        }

        void startStopWatchEvents() //This method is called when the Start/Stop watching events button is pressed.
        {
            if (_sam.EventListenerRunning) 
            {
                _sam.StopListeningForEvents();
                GUI_EventListenUpdate(false);
            }
            else
            {
                _sam.StartListeningForEvents();
                GUI_EventListenUpdate(true);

            }
        }
    }
}
