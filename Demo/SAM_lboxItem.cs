using Indieteur.SAMAPI;
using System.Text;

namespace Demo
{
    class ListBoxItem //The item that will be added to the list box.
    {
        public string Name;
        public SteamApp AssociatedApp;
        SteamAppsManager _sam;
        public ListBoxItem(string name, SteamApp associatedApp, SteamAppsManager steamAppsMan)
        {
            Name = name;
            AssociatedApp = associatedApp;
            _sam = steamAppsMan;
        }
        public override string ToString() //Will be called by the list box when the item is added/re-added. The resulting string is the text shown on the list box for the item.
        {
            StringBuilder sb = new StringBuilder(Name);
            if (_sam.EventListenerRunning) //Reflect the status only if the Event Listener is running.
            {
                if (AssociatedApp.IsRunning)
                    sb.Append(" [Running]");
                if (AssociatedApp.IsUpdating)
                    sb.Append(" (Updating)");
            }
            return sb.ToString();
        }
    }
}
