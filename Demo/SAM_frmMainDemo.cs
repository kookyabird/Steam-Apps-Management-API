using System;
using System.Windows.Forms;
using Indieteur.SAMAPI;

namespace Demo
{
    public partial class FrmMainDemo : Form
    {
        SteamAppsManager _sam;
        public FrmMainDemo()
        {
            InitializeComponent();
            
        }

        private void frmMainDemo_Load(object sender, EventArgs e)
        {
            loadSamapi(); //Initialze the SteamAppsManager class.
            loadSteamAppsToListBox(); //Once we are done initializing the SteamAppsManager class, load the steam apps to the list box.
        }

        private void tmrEvents_Tick(object sender, EventArgs e) //We need a timer that will always update the textboxes and the list box items statuses for us. This timer will be called every 1 second.
        {
            refreshListBox();
            if (lstApps.SelectedItem != null)
            {
                updateGuiElements(lstApps.SelectedItem as ListBoxItem);
            }
        }

        private void frmMainDemo_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            if (_sam !=null && _sam.EventListenerMustRun && _sam.EventListenerRunning) //If the user did not stop the event listener before closing the form.
                _sam.StopListeningForEvents();
        }

       
    }
}
