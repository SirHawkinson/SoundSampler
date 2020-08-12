using System;
using System.Windows.Forms;

namespace SoundSampler
{
    static class Program

    {
       
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SamplerAppContext app = new SamplerAppContext();
            Application.ApplicationExit += app.OnApplicationExit;
            Application.Run(app);
           
        }
    }
}
