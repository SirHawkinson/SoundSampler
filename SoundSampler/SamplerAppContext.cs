using SoundSampler.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SoundSampler
{

    /*
     * Main application context. SpectrumLED only runs in the systray (no forms). The icon is the
     * main interaction with the functionality: enable/disable on click, right click for context
     * menu to set options and exit.
     */
    public class SamplerAppContext : ApplicationContext
    {
        
     

        // Refresh rate options
        public const double VERYSLOW_MS = 1000 / 8.0;
        public const double SLOW_MS = 1000 / 16.0;
        public const double MED_MS = 1000 / 30.0;
        public const double FAST_MS = 1000 / 60.0;
        public const double FULL_MS= 1000 / 120.0;
        public const double FULLSPEEDAHEAD_MS = 1000 / 400.0;


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

            systrayIcon = new NotifyIcon();
            systrayIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Update Speed", new MenuItem[] {
                    new MenuItem("Very slow (8Hz)", (s, e) => UpdateSpeed_Click(s, VERYSLOW_MS)),
                    new MenuItem("Slow (16Hz)", (s, e) => UpdateSpeed_Click(s, SLOW_MS)),
                    new MenuItem("Medium (30Hz)", (s, e) => UpdateSpeed_Click(s, MED_MS)),
                    new MenuItem("Fast (60Hz)", (s, e) => UpdateSpeed_Click(s, FAST_MS)),
                    new MenuItem("Full (120Hz)", (s, e) => UpdateSpeed_Click(s, FULL_MS)),
                    new MenuItem("FullSpeed (400Hz)", (s, e) => UpdateSpeed_Click(s, FULLSPEEDAHEAD_MS))
                }),
                new MenuItem("Exit SpectrumLED", OnApplicationExit)
            });
            systrayIcon.ContextMenu.MenuItems[0].MenuItems[0].Checked = true;
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

    }
}