using System;
using System.Windows.Forms;

namespace SoundSampler
{
    static class Program

    {
        static void Main()
        {
            // Based on https://github.com/glowboy/SpectrumLED
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SamplerAppContext app = new SamplerAppContext();
            Application.ApplicationExit += app.OnApplicationExit;
            Application.Run(app);
        }
    }
}
