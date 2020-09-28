using System.Diagnostics;
using System.Threading;

namespace Indieteur.SAMAPI
{
    //Based on https://stackoverflow.com/questions/42651987/get-process-id-or-process-name-after-launching-a-steam-game-via-steam-rungamei
    //This file contains the method which fires the delegates registered to the events of the class.
    public partial class SteamAppsManager
    {
        public delegate void DStatusChange(SteamApp app);
        public delegate void DExecChange(SteamApp app, Process process);
        internal const string RegAppsKey = "Apps";
        internal const string RegUpdatingKey = "Updating";
        internal const string RegRunningKey = "Running";
        
        /// <summary>
        /// This event is called when an installed Steam Application is being updated.
        /// </summary>
        public event DStatusChange SteamAppOnUpdating;
        
        /// <summary>
        /// This event is called when a Steam Application has been updated or if the update has been aborted.
        /// </summary>
        public event DStatusChange SteamAppOnUpdateAbortOrFinish;
        
        /// <summary>
        /// This event is called when a Steam Application status was set to launched by the Steam Client.
        /// </summary>
        public event DStatusChange SteamAppOnLaunched;

        /// <summary>
        /// This event is called when a Steam Application status was set to not running by the Steam Client.
        /// </summary>
        public event DStatusChange SteamAppOnExit;

        /// <summary>
        /// This event is called when the process associated to the Steam Application has been detected.
        /// </summary>
        public event DExecChange SteamAppOnProcessDetected;
        
        /// <summary>
        /// This event is called when the process associated to the Steam Application has exited.
        /// </summary>
        public event DExecChange SteamAppOnProcessQuit;

        /// <summary>
        /// Manually updates the running and updating status of the Steam Applications and if a Steam App is running, detects the process associated with it. This function also calls the methods subscribed to the events associated with the status changes.
        /// </summary>
        public void CheckForEvents()
        {
            foreach (SteamApp sapp in _steamapps) 
            {
                lock (sapp) //lock the steam app class for thread safety reasons.
                {
                    //Cache the isRunning, isUpdating statuses and the associated process which we will be using for comparison after the status and associated process of the Application has been updated.
                    bool isRunning = sapp.IsRunning; 
                    bool isUpdating = sapp.IsUpdating;
                    Process proc = sapp.RunningProcess;

                    sapp.UpdateAppStatus(); 

                    //Compare the values before the update was performed and after the update was performed. Check if they match. If they don't, it means something has changed so call the event associated with the change.
                    if (!isRunning && sapp.IsRunning) 
                        Interlocked.CompareExchange(ref SteamAppOnLaunched, null, null)?.Invoke(sapp); 
                    else if (isRunning && !sapp.IsRunning)
                        Interlocked.CompareExchange(ref SteamAppOnExit, null, null)?.Invoke(sapp);

                    if (!isUpdating && sapp.IsUpdating)
                        Interlocked.CompareExchange(ref SteamAppOnUpdating, null, null)?.Invoke(sapp);
                    else if (isUpdating && !sapp.IsUpdating)
                        Interlocked.CompareExchange(ref SteamAppOnUpdateAbortOrFinish, null, null)?.Invoke(sapp);

                    if (proc == null && sapp.RunningProcess != null)
                        Interlocked.CompareExchange(ref SteamAppOnProcessDetected, null, null)?.Invoke(sapp, sapp.RunningProcess);
                    else if (proc != null && sapp.RunningProcess == null)
                        Interlocked.CompareExchange(ref SteamAppOnProcessQuit, null, null)?.Invoke(sapp, proc);
                }
            }
        }
    }
}
