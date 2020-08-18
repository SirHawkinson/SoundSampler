using CSCore;
using CSCore.SoundIn;
using CSCore.Streams;
using System;
using System.Timers;
using System.IO.Ports;
using System.Windows.Forms;

namespace SoundSampler
{
    /*
     Audio capture, USB handling, settings and variables initializator. 
     */
    public class SamplerApp
    {
        //Serial ports initialization variables
        static SerialPort _serialPort;
        private string Port;
        private int baud;

        // Ticker that triggers audio re-rendering. User-controllable via the systray menu
        private System.Timers.Timer ticker;

        // CSCore classes that read the WASAPI data and pass it to the SampleHandler
        private WasapiCapture capture;
        private SingleBlockNotificationStream notificationSource;
        private IWaveSource finalSource;

        // In-program class calls
        private SampleHandler SampleHandler;
        private Handler Handler;

        /*
         * Basic initialization. No audio is read until SetEnable(true) is called.
         */
        public SamplerApp()
        {
            // Init the timer
            ticker = new System.Timers.Timer(SamplerAppContext.Veryfast_MS);
            ticker.Elapsed += Tick;

            Port = null;
            baud = 115200;

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
                    _serialPort = new SerialPort(Port, baud);
                    _serialPort.ReadTimeout = 250;
                    _serialPort.WriteTimeout = 250;
                    _serialPort.Open();
                    StartCapture();
                    ticker.Start();
            }
            else
            {
                StopCapture();

                // Send zero bit through port, this will force LEDs to blackout
                byte[] end =BitConverter.GetBytes(0);
                _serialPort.Write(end, 0, 1);
                if (_serialPort.IsOpen == true)
                {
                    _serialPort.Close();
                }
                ticker.Stop();
                
            }
        }

        // Convert data to bits then send through selected COM
        public void COMSend(int data)
        {
            byte[] b = BitConverter.GetBytes(data);
            _serialPort.Write(b, 0, 1);
        }

        /*
         * Update the timer tick speed, which updates the FFT and sound rendering speeds(?).
         */
        public void UpdateTickSpeed(double intervalMs)
        {
            // No need to stop/start, setting Interval does that
            ticker.Interval = intervalMs;
        }

        // Initiate a new COM port and open it
        public void Selected_COMPort(string COMPort)
        {
            Port = COMPort;
        }

        /*
         * Disable the program upon shutting down to clear data and close ports without having to pause the program beforehand.
         * Dirty, but couldn't think of anything else.
         */
        public void Shutdown(object sender, EventArgs e)
        {
            try
            {
                SetEnabled(false);
            }
            catch (Exception exc)
            {
                
                
            }
        }

        /*
         * Ticker callback handler. Performs the actual FFT, massages the data into raw spectrum
         * data, and sends it to handler.
         */
        private void Tick(object sender, ElapsedEventArgs e)
        {
            // Get the FFT results and send to Handler
            float[] values = SampleHandler.GetSpectrumValues();
           
                Handler.DataSend(values);
        }

        /*
         * Begin audio capture. Connects to WASAPI, initializes the sample handler, and begins
         * sending captured data to it.
         */
        private void StartCapture()
        {
            // Initialize hardware capture
            capture = new WasapiLoopbackCapture(10);
            capture.Initialize();

            // Initialize sample handler
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