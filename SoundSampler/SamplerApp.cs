using CSCore;
using CSCore.SoundIn;
using CSCore.Streams;
using System;
using System.Timers;
using System.IO.Ports;

namespace SoundSampler
{
    /*
     Audio capture, USB handling, settings and variables initializator. 
     */
    public class SamplerApp
    {
        // Serial ports initialization variables
        static SerialPort serialPort;
        private string Port;
        private int baud;

        // Ticker that triggers audio re-rendering. User-controllable via the systray menu
        private System.Timers.Timer ticker;

        // CSCore classes that read the WASAPI data and pass it to the SampleHandler
        private WasapiCapture wasapiCapture;
        private SingleBlockNotificationStream notificationSource;
        private IWaveSource finalSource;

        // In-program class calls
        private SampleHandler SampleHandler;
        private Handler Handler;

        // Declaration of a switch for swapping between 3-columns and octave audio handling, by default it will 
        // use octaves to analyze sound.
        public Boolean bassBased;

        // Column assigning method, by default it gives "octaves" assigning.
        public string method = "octaves";

        /*
         * Basic initialization. No audio is read until SetEnable(true) is called.
         */
        public SamplerApp()
        {
            // Init the timer, there's a conflict between two libraries on which one to use for Times, hence the class clarification
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
                    serialPort = new SerialPort(Port, baud);
                    serialPort.ReadTimeout = 250;
                    serialPort.WriteTimeout = 250;
                    serialPort.Open();
                    StartCapture();
                    ticker.Start();
            }
            else
            {
                StopCapture();

                // Send zero bit through port, this will force LEDs to blackout
                byte[] end =BitConverter.GetBytes(0);
                serialPort.Write(end, 0, 1);
                if (serialPort.IsOpen == true)
                {
                    serialPort.Close();
                }
                ticker.Stop();
                
            }
        }

        // Convert data to bits then send through selected COM
        public void COMSend(int data)
        {
            byte[] b = BitConverter.GetBytes(data);
            serialPort.Write(b, 0, 1);
        }

        /*
         * Update the timer tick speed, which updates the FFT and sound rendering speeds(?).
         */
        public void UpdateTickSpeed(double intervalMs)
        {
            ticker.Interval = intervalMs;
        }

        // Overwrite the default null COMPort
        public void Selected_COMPort(string COMPort)
        {
            Port = COMPort;
        }

        public void Selected_Method(string calcMethod)
        {
            method = calcMethod;
        }
        /*
         * Disable the program upon shutting down to clear data and close ports without having to pause the program beforehand.
         * With no exception catching the program would throw an error upon shutting it down. Gotta love the try-catch.
         */
        public void Shutdown(object sender, EventArgs e)
        {
            try
            {
                SetEnabled(false);
            }
            catch (Exception)
            {
                
                
            }
        }

        /*
         * Ticker callback handler. Performs the actual FFT, massages the data into raw spectrum
         * data, and sends it to handler.
         */
        private void Tick(object sender, ElapsedEventArgs e)
        {
            // Get the FFT results and send to Handler with method as a results handling variable.
            float[] values = SampleHandler.GetSpectrumValues(method);
           
                Handler.SendData(values, bassBased,method);
        }

        /*
         * Initializes WASAPI, initializes the sample handler, and sends captured data to it.
         */
        private void StartCapture()
        {
            // Initialize hardware capture
            wasapiCapture = new WasapiLoopbackCapture(10);
            wasapiCapture.Initialize();

            // Initialize sample handler
            SampleHandler = new SampleHandler(wasapiCapture.WaveFormat.Channels, wasapiCapture.WaveFormat.SampleRate);

            // Configure per-block reads rather than per-sample reads
            notificationSource = new SingleBlockNotificationStream(new SoundInSource(wasapiCapture).ToSampleSource());
            notificationSource.SingleBlockRead += (s, e) => SampleHandler.Add(e.Left, e.Right);
            finalSource = notificationSource.ToWaveSource();
            wasapiCapture.DataAvailable += (s, e) => finalSource.Read(e.Data, e.Offset, e.ByteCount);

            // Start capture
            wasapiCapture.Start();
        }

        /*
         * Stop the audio capture, if currently recording. Properly disposes member objects.
         */
        private void StopCapture()
        {
            if (wasapiCapture.RecordingState == RecordingState.Recording)
            {
                wasapiCapture.Stop();

                finalSource.Dispose();
                notificationSource.Dispose();
                wasapiCapture.Dispose();

                SampleHandler = null;
            }
        }

    }
}