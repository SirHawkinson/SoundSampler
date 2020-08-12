using CSCore;
using CSCore.SoundIn;
using CSCore.Streams;
using System;
using System.Timers;
using System.IO.Ports;

namespace SoundSampler
{

    /*
     * The real meat of the app, outside all that Windows junk. Handles audio device initialization,
     * data capture, and audio rendering.
     */
    public class SamplerApp
    {
        //Serial ports initializations
        static SerialPort _serialPort;
        // Our ticker that triggers audio re-rendering. User-controllable via the systray menu
        private Timer ticker;

        // CSCore classes that read the WASAPI data and pass it to the SampleHandler
        private WasapiCapture capture;
        private SingleBlockNotificationStream notificationSource;
        private IWaveSource finalSource;

        // An attempt at only-just-post-stream-of-consciousness code organization
        private SampleHandler SampleHandler;
        private Handler Handler;

        /*
         * Basic initialization. No audio is read until SetEnable(true) is called.
         */
        public SamplerApp()
        {
            // Init the timer
            ticker = new Timer(SamplerAppContext.SLOW_MS);
            ticker.Elapsed += Tick;

            // Create a handler
            Handler = new Handler();
        }

        /*
         * Enable or disable audio capture.
         */
        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                _serialPort = new SerialPort("COM7", 115200);
                _serialPort.ReadTimeout = 250;
                _serialPort.WriteTimeout = 250;

                _serialPort.Open();
                StartCapture();
                ticker.Start();
            }
            else
            {
                StopCapture();
                _serialPort.Close();
                ticker.Stop();
                
            }
        }
        public void COMSend(int data)
        {
            byte[] b = BitConverter.GetBytes(data);
            _serialPort.Write(b,0,1);
        }


        /*
         * Update the timer tick speed, which updates the FFT and sound rendering speeds(?).
         */
        public void UpdateTickSpeed(double intervalMs)
        {
            // No need to stop/start, setting Interval does that
            ticker.Interval = intervalMs;
        }

        /*
         * Cleanly release audio resources.
         */
        public void Shutdown(object sender, EventArgs e)
        {
            SetEnabled(false);
        }

        /*
         * Ticker callback handler. Performs the actual FFT, massages the data into raw spectrum
         * data, and sends it to handler.
         */
        private void Tick(object sender, ElapsedEventArgs e)
        {
            // Get the FFT results and send to Handler
            float[] values = SampleHandler.GetSpectrumValues();
            if (values != null)
            {
                Handler.DataSend(values);
            }
        }

        /*
         * Begin audio capture. Connects to WASAPI, initializes the sample handler, and begins
         * sending captured data to it.
         */
        private void StartCapture()
        {
            // Initialize hardware capture
            capture = new WasapiLoopbackCapture();
            capture.Initialize();

            // Init sample handler
            SampleHandler = new SampleHandler(capture.WaveFormat.Channels, capture.WaveFormat.SampleRate);

            // Configure per-block reads rather than per-sample reads
            notificationSource = new SingleBlockNotificationStream(new SoundInSource(capture).ToSampleSource());
            notificationSource.SingleBlockRead += (s, e) => SampleHandler.Add(e.Left, e.Right);

            finalSource = notificationSource.ToWaveSource();
            capture.DataAvailable += (s, e) => finalSource.Read(e.Data, e.Offset, e.ByteCount);

            // Start capture
            capture.Start();
        }

        /*
         * Stop the audio capture, if currently recording. Properly disposes member objects.
         */
        private void StopCapture()
        {
            if (capture.RecordingState == RecordingState.Recording)
            {
                capture.Stop();

                finalSource.Dispose();
                notificationSource.Dispose();
                capture.Dispose();

                SampleHandler = null;
            }
        }

    }
}