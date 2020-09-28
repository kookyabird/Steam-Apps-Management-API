using System.Threading;

namespace Indieteur.SAMAPI
{
    public partial class SteamAppsManager
    {
        /// <summary>
        /// Returns true if the Event Listener is running. False if not.
        /// </summary>
        public bool EventListenerRunning => _listenerThread != null && _listenerThread.IsAlive;

        /// <summary>
        /// Returns true if the library was ordered to start listening for events and false if not or if the library already received the command to stop.
        /// </summary>
        public bool EventListenerMustRun => _listenerShouldRun > 0;

        /// <summary>
        /// The interval set between Event Listener checks in milliseconds.
        /// </summary>
        private int EventListenerInterval => _listenerInterval;

        private int _listenerShouldRun; //Thread Cleanup based on https://stackoverflow.com/questions/12312155/finishing-up-a-thread-loop-after-disposal.
        private int _listenerInterval;
        private Thread _listenerThread;

        /// <summary>
        /// Creates a new thread which will listen for Steam Events.
        /// </summary>
        /// <param name="interval">The interval between checks in milliseconds.</param>
        public void StartListeningForEvents (int interval = 1000)
        {
            if (_listenerThread != null && _listenerThread.IsAlive)
                throw new ThreadStateException("A listener thread is already running!");
            Interlocked.Exchange(ref _listenerShouldRun, 1); //Similar to listenerShouldRun = 1; but for thread safety reason, we need to do call this method instead.
            Interlocked.Exchange(ref _listenerInterval, interval);
            _listenerThread = new Thread(listenerThreadMethod); 
            _listenerThread.Start();
        }

        /// <summary>
        /// Sets the interval between Event Listener checks.
        /// </summary>
        /// <param name="interval">The interval between checks in milliseconds.</param>
        public void SetEventListenerInterval (int interval)
        {
            if (_listenerShouldRun <= 0)
                throw new ThreadStateException("The listener thread is not running!");
            Interlocked.Exchange(ref _listenerInterval, interval); //Similar to listener_interval = interval; but for thread safety reason, we need to do call this method instead.
        }

        /// <summary>
        /// Stops the thread which is listening for events. (NOTE: A delay might occur before the thread completely stops executing.)
        /// </summary>
        public void StopListeningForEvents()
        {
            if (_listenerShouldRun <= 0)
                throw new ThreadStateException("The listener thread is not running!");
            Interlocked.Exchange(ref _listenerShouldRun, 0);
        }

        void listenerThreadMethod()
        {
            while (_listenerShouldRun > 0) //Continue looping to listen for events until the main thread has set the listenerShouldRun variable to 0. (which means stop the thread.)
            {
                CheckForEvents();
                Thread.Sleep(_listenerInterval); //listener_interval is specified by the user.
            }
            
        }
    }
}
