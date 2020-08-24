using SoundSampler.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;

namespace SoundSampler
{

    /*
     * Application context frame. Since the app runs only in the quick bar (so no windows or forms)
     * you can only interact with the program by enabling or disabling it on left click. You can
     * access its menu (COM Port,refresh rate selection and exit). 
     */
    public class SamplerAppContext : ApplicationContext
    {
        // Refresh rate options.
        public const double Slow_MS = 1000 / 30.0;
        public const double Med_MS = 1000 / 60.0;
        public const double Fast_MS = 1000 / 120.0;
        public const double Veryfast_MS = 1000 / 400.0;
        public const double Exp_MS = 1000 / 1000.0;

        /*
        //Legacy method.
        public string bassHandling = "bass";
        public string octavesHandling = "octaves";
        */

        // The systray icon and main app control.
        private NotifyIcon systrayIcon;
        private SamplerApp SamplerApp;

        // Set the program as disabled and COMPort as null by default.
        private Boolean enabled = false;
        private string selectedPort;

        /*
         * Set up the application. Configures the main app handler, creates and initializes the
         * systray icon and its context menu, and makes the icon visible.
         */
        public SamplerAppContext()
        {
            SamplerApp = new SamplerApp();
            selectedPort = null;


            MenuItem COMList = new MenuItem("COM List");
            COMlist().ForEach(COM => COMList.MenuItems.Add(new MenuItem(COM, (s, e) => SetCOMPort(s, COM.ToString()))));
            systrayIcon = new NotifyIcon();

            systrayIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                COMList,
                new MenuItem("Update Speed", new MenuItem[] {
                    new MenuItem("Slow (30Hz)", (s, e) => UpdateSpeed_Click(s, Slow_MS)),
                    new MenuItem("Medium (60Hz)", (s, e) => UpdateSpeed_Click(s, Med_MS)),
                    new MenuItem("Fast (120Hz)", (s, e) => UpdateSpeed_Click(s, Fast_MS)),
                    new MenuItem("Full (400Hz)", (s, e) => UpdateSpeed_Click(s, Veryfast_MS)),
                    new MenuItem("Exp (1000Hz)", (s, e) => UpdateSpeed_Click(s, Exp_MS)), // By the time making this program I didn't own a 1kHz refresh rate led strip, 
                    // so not quite sure how it would behave. Code also seem to generate nulls when exceeding 65Hz.
                }),
                new MenuItem("Sound columns", new MenuItem[]{
                    // No fking clue if that's a bad idea to implement a boolean here, seems to work fine.
                    new MenuItem("Bass", (s,e) => AudioRange_Handling(s,true)),
                    new MenuItem("Octaves", (s,e) => AudioRange_Handling(s,false)),
                    }),
                /* Legacy method.
                new MenuItem("Sound handling", new MenuItem[]{
                    new MenuItem("Bass based calculations", (s,e) => AudioCalculations_Method(s,bassHandling)),
                    new MenuItem("Octaves basec calculations", (s,e) => AudioCalculations_Method(s,octavesHandling)),
                    }),
                */
                new MenuItem("Exit SoundSampler", OnApplicationExit)
            });

            // Default options precheck.
            systrayIcon.ContextMenu.MenuItems[0].MenuItems[0].Checked = false;
            systrayIcon.ContextMenu.MenuItems[1].MenuItems[3].Checked = true;
            systrayIcon.ContextMenu.MenuItems[2].MenuItems[1].Checked = true;
            // systrayIcon.ContextMenu.MenuItems[3].MenuItems[1].Checked = true; Legacy method.
            systrayIcon.MouseClick += SystrayIcon_Click;
            systrayIcon.Icon = Icon.FromHandle(Resources.SoundSamplerOFF.GetHicon());
            systrayIcon.Text = "SoundSampler";
            systrayIcon.Visible = true;
        }

        /*
         * Application exit callback handler. Properly dispose of device capture. Set the systray
         * icon to false to keep it from hanging around until the user mouses over it.
         */
        public void OnApplicationExit(object sender, EventArgs e)
        {
            SamplerApp.Shutdown(sender, e);
            systrayIcon.Visible = false;
            Application.Exit();
        }

        // Get a list of serial port names, failsafe in case it will not detect any devices.
        private List<string> COMlist()
        {
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length == 0)
            {
                MessageBox.Show("No connected USB device detected. SoundSampler will shutdown.",
                                "No available ports detected.", MessageBoxButtons.OK);
                return null;                
            }
            else
            {
                List<string> list = ports.ToList();
                return list;
            }
        }

        // Select COM port to send data to.
        private void SetCOMPort(object sender, string port)
        {
            CheckMeAndUncheckSiblings((MenuItem)sender);
            SamplerApp.Selected_COMPort(port);
            selectedPort = port;
        }

        /*
         * Left click callback handler. Enables/disables, switches between icons.
         */
        private void SystrayIcon_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {

                // No device selected handling
                if (selectedPort == null)
                {
                    MessageBox.Show("Please select a port.",
                              "No port selected.", MessageBoxButtons.OK);
                }
                else
                {
                    enabled = !enabled;

                    // Icon switching
                    if (enabled == true)
                        systrayIcon.Icon = Icon.FromHandle(Resources.SoundSamplerON.GetHicon());
                    else
                        systrayIcon.Icon = Icon.FromHandle(Resources.SoundSamplerOFF.GetHicon());

                    SamplerApp.SetEnabled(enabled);
                }
            }            
        }

        /*
         * Speed options callback handler. Sets the tick/render speed in the app.
         */
        private void UpdateSpeed_Click(object sender, double intervalMs)
        {
            CheckMeAndUncheckSiblings((MenuItem)sender);
            SamplerApp.UpdateTickSpeed(intervalMs);
        }

        // Set audio handling.
        private void AudioRange_Handling(object sender, bool Bass)
        {
            CheckMeAndUncheckSiblings((MenuItem)sender);            
            SamplerApp.bassBased = Bass;
        }

        /* Legacy method.
        private void AudioCalculations_Method(object sender, string method)
        {
            CheckMeAndUncheckSiblings((MenuItem)sender);
            SamplerApp.Selected_Method(method);
        }
        */

        // Deactivate other list options after pressing one.
        private void CheckMeAndUncheckSiblings(MenuItem me)
        {
            foreach (MenuItem child in me.Parent.MenuItems)
            {
                child.Checked = child == me;
            }
        }
    }   
}
 