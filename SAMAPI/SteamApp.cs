using System;
using System.Diagnostics;
using System.Threading;
using Indieteur.VDFAPI;

namespace Indieteur.SAMAPI
{
    /// <summary>
    /// Provides basic read only information about a single Steam Application.
    /// </summary>
    public class SteamApp
    {
        const string MANIFEST_NODE = "AppState"; //The VDF node containing the information about the Steam App
        const string MANIFEST_KEY_NAME = "name"; //The name of the VDF Key which tells us the name of the app
        const string MANIFEST_KEY_APPID = "appid"; //The name of the VDF Key pertaining to the ID of the App.
        const string MANIFEST_KEY_INSDIR = "installdir"; //The name of the VDF Key which pertains to the installation directory name of the app.

        /// <summary>
        /// The name of the Steam application.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The Application ID of the Steam application.
        /// </summary>
        public int AppId { get; private set; }

        /// <summary>
        /// The path to the installation directory of the Steam Application.
        /// </summary>
        public string InstallDir { get; private set; }

        /// <summary>
        /// The name of the installation directory of the Steam Application.
        /// </summary>
        public string InstallDirName { get; private set; }

        /// <summary>
        /// Returns true if the status of the Steam Application was set to updating by the Steam Client and false if not. (This field only gets updated if the Event Listener is running or the CheckForEvents/UpdateAppStatus method was called beforehand.)
        /// </summary>
        public bool IsUpdating { get; private set; }

        /// <summary>
        /// Returns true if the status of the Steam Application was set to running by the Steam Client and false if not. (This field only gets updated if the Event Listener is running or the CheckForEvents/UpdateAppStatus method was called beforehand.)
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Returns a Process class instance which contains detailed information about the running process pertaining to the application. (This field only gets updated if the Event Listener is running or the CheckForEvents/UpdateAppStatus method was called beforehand.)
        /// </summary>
        public Process RunningProcess { get; private set; }

        /// <summary>
        /// Indicates the executable name of the application (without the .exe) which will be used to locate the running process when the app is launched. If left empty, the library will try to guess which process pertains to the app.
        /// </summary>
        public string ProcessNameToFind
        {
            get => _exeName;
            set => Interlocked.Exchange(ref _exeName, value);
        }

        string _exeName = "";

        /// <summary>
        /// Returns the VDF Data Structure pertaining to the Steam Application which was parsed to retrieve information like the Name, AppId, etc. of the app.
        /// </summary>
        public VdfData UnparsedData { get; private set; }

        /// <summary>
        /// Creates a SteamApp Class instance using an already parsed VDF Data Structure.
        /// </summary>
        /// <param name="parsedmanifest">The parsed VDF Data Structure.</param>
        /// <param name="libPath">The path of the library containing the application.</param>
        public SteamApp(VdfData parsedmanifest, string libPath)
        {
            init(parsedmanifest, libPath);
        }

        /// <summary>
        /// Creates a SteamApp Class instance by parsing the VDF Data Structure in a Manifest file.
        /// </summary>
        /// <param name="manifestPath">The path to the Application Manifest file.</param>
        /// <param name="libPath">The path of the library containing the application.</param>
        public SteamApp(string manifestPath, string libPath)
        {
            var vData = new VdfData();
            vData.LoadDataFromFile(manifestPath);
            init(vData, libPath);
        }

        /// <summary>
        /// Initializes the SteamApp class.
        /// </summary>
        /// <param name="vdfdata">The parsed VDF Data Structure.</param>
        /// <param name="libPath">The path of the library containing the application.</param>
        void init (VdfData vdfdata, string libPath)
        {
            if (vdfdata == null) 
                throw new ArgumentNullException(nameof(vdfdata), "Argument VDF Data is set to null!");

            if (vdfdata.Nodes == null || vdfdata.Nodes.Count == 0) 
                throw new NullReferenceException("Nodes of VDFData is either null or empty!");

            UnparsedData = vdfdata;

            var vNode = vdfdata.Nodes.FindNode(MANIFEST_NODE); //Locate our root node. We can do nodes[0] as well and it'll be faster but just to be safe.

            if (vNode == null)
                throw new NullReferenceException("Node " + MANIFEST_NODE + " is not found!");

            if (vNode.Keys == null || vNode.Keys.Count == 0)
                throw new NullReferenceException("Node " + MANIFEST_NODE + " list of keys is either null or empty!");

            var vKey = vNode.Keys.FindKey(MANIFEST_KEY_NAME); //Locate our first key which will be the name of the app.

            if (vKey != null) 
                Name = vKey.Value;
            else
                throw new NullReferenceException("Key pertaining to the name of the app is not found under " + MANIFEST_NODE + " node.");

            vKey = vNode.Keys.FindKey(MANIFEST_KEY_APPID); 

            if (vKey != null) 
            {
                if(int.TryParse(vKey.Value, out var tryresult))
                    AppId = tryresult;
                else
                    throw new NullReferenceException("Key pertaining to the Application ID of the app under " + MANIFEST_NODE + " node is invalid!");
            }
            else
                throw new NullReferenceException("Key pertaining to the Application ID of the app is not found under " + MANIFEST_NODE + " node.");

            vKey = vNode.Keys.FindKey(MANIFEST_KEY_INSDIR); 

            if (vKey != null)
            {
                InstallDir = libPath + "\\" + SteamAppsManager.SteamAppsDirname + "\\" + SteamAppsManager.SteamAppsCommonDirname + "\\" + vKey.Value; 
                InstallDirName = vKey.Value; 
            }
            else
                throw new NullReferenceException("Key pertaining to the directory name containing the app under " + MANIFEST_NODE + " node is not found!");

        }

        /// <summary>
        /// Launches the Steam Application by calling "steam://rungameid/[AppID]".
        /// </summary>
        public void Launch()
        {
            Process.Start("steam://rungameid/" + AppId);
        }

        /// <summary>
        /// Checks if the Steam Application is running or updating by checking certain registry keys and updates the fields IsUpdating and IsRunning accordingly. Also, sets the RunningProcess field if the process pertaining to the app is found.
        /// </summary>
        public void UpdateAppStatus()
        {
            var regPath = SteamAppsManager.RegSteam + "\\" + SteamAppsManager.RegAppsKey + "\\" + AppId; //We can know if the application is running or updating by checking certain values under this registry key.
            int isRunningOrUpdating = Helper.HKCU_RegGetKeyInt(regPath, SteamAppsManager.RegRunningKey); //The first value we check is if the application is running. Retrieve the value on registry using our helper method.
            if (isRunningOrUpdating < 0)
                throw new NullReferenceException("Running key under " + regPath + " registry path is not found!");
            if (isRunningOrUpdating > 0) 
            {
                if (RunningProcess == null) 
                {
                    string tExeName = _exeName; //For thread safety purposes. Cache the string value.
                    RunningProcess = Helper.FindAppProcess(InstallDir, tExeName); //Only perform the search helper method when the process hasn't been located yet as the execution is costly.
                }
                else
                {
                    RunningProcess.Refresh(); //Refresh the information about the process.
                    if (RunningProcess.HasExited) //If Running process has already exited then set the _runningProc to null. This should also fix problems for launcher apps being set as the RunningProcess and never being updated when the main app has been launched.
                        RunningProcess = null;
                }
                IsRunning = true; 
            }
            else 
            {
                IsRunning = false; 
                RunningProcess = null; //Make sure to set the RunningProcess field to nothing.
            }

            isRunningOrUpdating = Helper.HKCU_RegGetKeyInt(regPath, SteamAppsManager.RegUpdatingKey); //The next thing that we need to check is if the app is being updated by steam. 
            
            if (isRunningOrUpdating < 0) //This one is less complex than for the "app is running" check as we only need to set the IsUpdating field to true or false.
                throw new NullReferenceException("Updating key under " + regPath + " registry path is not found!");
            
            IsUpdating = isRunningOrUpdating > 0;
        }
    }
}
