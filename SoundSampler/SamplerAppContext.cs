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
     * access its menu (COM Port,refresh rate selection and exit)
     */
    public class SamplerAppContext : ApplicationContext
    {
        // Refresh rate options

        public const double Slow_MS = 1000 / 30.0;
        public const double Med_MS = 1000 / 60.0;
        public const double Fast_MS = 1000 / 120.0;
        public const double Veryfast_MS = 1000 / 400.0;

        // The systray icon and main app control
        private NotifyIcon systrayIcon;
        private SamplerApp SoundSampler;

        // Master enabled state
        private Boolean enabled = false;

        /*
         * Set up the application. Configures the main app handler, creates and initializes the
         * systray icon and its context menu, and makes the icon visible.
         */
        public SamplerAppContext()
        {
            SoundSampler = new SamplerApp();

            MenuItem COMList = new MenuItem("COM List");
            COMlist().ForEach(COM => COMList.MenuItems.Add(
                    new MenuItem(COM, (s, e) => SetCOMPort(s, COM.ToString()))));

            systrayIcon = new NotifyIcon();

            systrayIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                COMList,
                new MenuItem("Update Speed", new MenuItem[] {
                    new MenuItem("Slow (30Hz)", (s, e) => UpdateSpeed_Click(s, Slow_MS)),
                    new MenuItem("Medium (60Hz)", (s, e) => UpdateSpeed_Click(s, Med_MS)),
                    new MenuItem("Fast (120Hz)", (s, e) => UpdateSpeed_Click(s, Fast_MS)),
                    new MenuItem("Full (400Hz)", (s, e) => UpdateSpeed_Click(s, Veryfast_MS)),
                }),
                new MenuItem("Exit SpectrumLED", OnApplicationExit)
            });

            systrayIcon.ContextMenu.MenuItems[0].MenuItems[0].Checked = false;
            systrayIcon.ContextMenu.MenuItems[1].MenuItems[3].Checked = true;
            systrayIcon.MouseClick += SystrayIcon_Click;
            systrayIcon.Icon = Icon.FromHandle(Resources.SoundSampler.GetHicon());
            systrayIcon.Text = "SoundSampler";
            systrayIcon.Visible = true;
        }

        /*
         * Application exit callback handler. Properly dispose of device capture. Set the systray
         * icon to false to keep it from hanging around until the user mouses over it.
         */
        public void OnApplicationExit(object sender, EventArgs e)
        {
            SoundSampler.Shutdown(sender, e);
            systrayIcon.Visible = false;
            Application.Exit();
        }

        // Get a list of serial port names
        private List<string> COMlist()
        {

            string[] ports = SerialPort.GetPortNames();
            List<string> list = ports.ToList();
            return list;
        }

        // Select COM port
        private void SetCOMPort(object sender, string port)
        {
            CheckMeAndUncheckSiblings((MenuItem)sender);
            SoundSampler.Selected_COMPort(port);
        }

        /*
         * Left click callback handler. Enables/disables.
         */
        private void SystrayIcon_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                enabled = !enabled;
                SoundSampler.SetEnabled(enabled);
            }
        }

        /*
         * Speed options callback handler. Sets the tick/render speed in the app.
         */
        private void UpdateSpeed_Click(object sender, double intervalMs)
        {
            CheckMeAndUncheckSiblings((MenuItem)sender);
            SoundSampler.UpdateTickSpeed(intervalMs);
        }

        // The definition of self-documenting code. Does this comment negate that?
        private void CheckMeAndUncheckSiblings(MenuItem me)
        {
            foreach (MenuItem child in me.Parent.MenuItems)
            {
                child.Checked = child == me;
            }
        }

        // Exceptions handling
    }   
}