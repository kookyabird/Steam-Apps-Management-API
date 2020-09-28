﻿using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Indieteur.SAMAPI
{
    /// <summary>
    /// Provides read only basic information about the Steam Applications installed on the local computer. (e.g. List of Installed Steam Apps, Steam App update, launch, quit and basic process detection.) 
    /// </summary>
    public partial class SteamAppsManager
    {
        //Default Directory or Executable names
        internal const string SteamAppsDirname = "steamapps";
        internal const string SteamAppsCommonDirname = "common";
        internal const string SteamName = "steam";
        internal const string SteamApiLibname = "steam_api";
        internal const string SteamApiLibnameX64 = "steam_api64";


        //Default path of the steam registry keys
        internal const string RegSteamX64 = @"SOFTWARE\WOW6432Node\Valve\Steam"; //64 bit machine
        internal const string RegSteam = @"SOFTWARE\Valve\Steam"; //32 bit machine and for HKCU
        internal const string RegInstallpathKey = "InstallPath"; //The name of the key which contains the path to the installation folder of steam.


        /// <summary>
        /// The installation directory of steam.
        /// </summary>
        public string InstallDir => _installdir;

        /// <summary>
        /// List of path to the Library Folders of the Steam Installation.
        /// </summary>
        public IReadOnlyList<string> LibraryFolders => _libraryFolders;

        /// <summary>
        /// List of Steam Applications installed.
        /// </summary>
        public IReadOnlyList<SteamApp> SteamApps => _steamapps;

        List<string> _libraryFolders;
        List<SteamApp> _steamapps;


        string _installdir;

        /// <summary>
        /// Creates an instance of the SteamAppsManager class using the provided steamDirectory path.
        /// </summary>
        /// <param name="steamDirectory">The steam installation directory.</param>
        public SteamAppsManager(string steamDirectory)
        {
            _installdir = steamDirectory;
            init(); //Call the init method.
        }
        /// <summary>
        /// Creates an instance of the SteamAppsManager class by checking where Steam is installed using the System Registry.
        /// </summary>
        public SteamAppsManager()
        {
            _installdir = GetSteamDirectory();
            if (string.IsNullOrWhiteSpace(_installdir))
                throw new NullReferenceException("Steam Directory was not found!");
            init();
           
        }

        /// <summary>
        /// Refreshes the list of library folders and installed steam applications and their information.
        /// </summary>
        public void Refresh()
        {
            bool isEventListenerRunning = false; //This variable will tell us if the event listener should be started again after the refresh.
            if (_listenerThread.IsAlive) 
            {
                if (_listenerShouldRun > 0) //Now, this one is different. It checks if the event listener was set to run as per the user. 
                {
                    isEventListenerRunning = true; //If it was, we need to make sure that we start the event listener again after we have completed the refresh. 
                    StopListeningForEvents(); //Order the event listener to stop running. We have to do this to prevent the listener thread from accessing the steam apps list while we are refreshing it.

                }
                while (_listenerThread.IsAlive) //The thread would most likely not stop immediately. We need to wait for it to stop before we do anything else.
                {
                    //Do nothing except wait for the event listener to stop before proceeding to refresh.
                }
            }
           
            init(); //The init method should do the job of refreshing the Library Directory List and the Steam Apps list.
            if (isEventListenerRunning) //Check if the listener was running before we performed the refresh. If it was, start the listener again.
                StartListeningForEvents(EventListenerInterval);
        }
        /// <summary>
        /// Performs the initialization of the class.
        /// </summary>
        void init()
        {
            _libraryFolders = LibraryFoldersHelper.RetrieveLibraryFolders(_installdir);
            _steamapps = LibraryFoldersHelper.RetrieveSteamAppsUnderLibraryFolders(_libraryFolders); 
        }

        /// <summary>
        /// Gets the installation folder of steam using the system registry. Returns an empty string if not found.
        /// </summary>
        /// <returns></returns>
        public static string GetSteamDirectory()
        {
            //Code based on DonBoitnott answer on https://stackoverflow.com/questions/18232972/how-to-read-value-of-a-registry-key-c-sharp.
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey((Environment.Is64BitOperatingSystem) ? RegSteamX64 : RegSteam)) //Find the key that contains the installation path of Steam
            {
                Object value = key?.GetValue(RegInstallpathKey); //Find the "InstallPath" value in the Steam Registry Key. It'll return it as an object which we will need to cast
                if (value != null)
                {
                        
                    return (string)value; //Value is a string so we should be able to cast it without any problems.
                }
            }
            return ""; 
        }

    }
}
