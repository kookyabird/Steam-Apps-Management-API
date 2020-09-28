using System;
using System.Collections.Generic;
using System.Management;
using System.Diagnostics;

namespace Indieteur.SAMAPI
{
    public static class Extension
    {
        /// <summary>
        /// Locate a Steam App using its name in the SteamApps collection.
        /// </summary>
        /// <param name="listofapps"></param>
        /// <param name="name">Name of the app to locate.</param>
        /// <param name="caseSensitive">Indicates if the capitalization of the name should matter for the search.</param>
        /// <param name="throwErrorOnNotFound">Indicates whether the method should throw an error if no matching Application is found.</param>
        /// <returns></returns>
        public static SteamApp FindAppByName(this IEnumerable<SteamApp> listofapps, string name, bool caseSensitive = false, bool throwErrorOnNotFound = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "Argument name cannot be null, empty or whitespace!");           
            if (!caseSensitive) //If not case sensitive, then convert the name to its lower case variant.
                name = name.ToLower();

            foreach (SteamApp sapp in listofapps)
            {
                //Will store the name of the steam app we are checking
                var compareName = caseSensitive ? sapp.Name : sapp.Name.ToLower();
                if (compareName == name)
                    return sapp;
            }

            if (throwErrorOnNotFound)
                throw new SteamAppNotFoundException(name + " application is not found!");
            return null;
        }

        /// <summary>
        /// Locate a Steam App using its unique ID in the SteamApps collection.
        /// </summary>
        /// <param name="listofapps"></param>
        /// <param name="appId">The unique identifier of the application.</param>
        /// <param name="throwErrorOnNotFound">Indicates whether the method should throw an error if no matching Application is found.</param>
        /// <returns></returns>
        public static SteamApp FindAppById(this IEnumerable<SteamApp> listofapps, int appId, bool throwErrorOnNotFound = false)
        {
            foreach (SteamApp sapp in listofapps) //loop through our list of apps
            {
                if (sapp.AppId == appId)
                    return sapp;
            }
            if (throwErrorOnNotFound)
                throw new SteamAppNotFoundException("application with ID of " + appId + " is not found!");
            return null;
        }

        /// <summary>
        /// Kills the process and its children processes.
        /// </summary>
        /// <param name="proc"></param>
        public static void KillProcessAndChildren(this Process proc)
        {
            killProcessAndChildrens(proc.Id);
        }
        //Based from https://stackoverflow.com/questions/23845395/in-c-how-to-kill-a-process-tree-reliably.
        static void killProcessAndChildrens(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                killProcessAndChildrens(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                var proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException) { } // Process already exited.
        }
    }

    /// <summary>
    /// An exception thrown by FindAppByID and FindAppByName methods.
    /// </summary>
    public class SteamAppNotFoundException : Exception
    {
        public SteamAppNotFoundException(string message) : base(message) { }
    }
}
