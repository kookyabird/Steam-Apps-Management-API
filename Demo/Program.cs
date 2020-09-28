using System;
using System.Windows.Forms;


namespace Demo
{
    static class Program
    {
        public static bool FailedStartMain = false; //This variable will let us know if we need to launch the frmDemoVDF on its own.
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMainDemo());
            if (FailedStartMain) //If the loading of the Steam Apps Manager library has failed, then launch frmDemoVDF instead.
                Application.Run(new FrmDemoVdf());
        }
    }
}

