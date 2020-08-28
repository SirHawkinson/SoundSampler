using CSCore.DSP;
using System;
//  using System.Linq;
using System.Collections.Generic;

namespace SoundSampler
{
    
    public class SampleHandler
    {
        // Different data handling approach.
        private List<float> _spectrumData;

        // Basic FFT constants.
        const FftSize fftSize = FftSize.Fft2048;
        const int fftSizeInt = (int)fftSize;

        /* The weight given to the previous sample for time-based smoothing. High value works great when 
         * sending it to the LED strip ewhen the software is set to a high refresh rate, making the 
         * transition between values much milder, lower values increase accuracy of sampling.
         * Setting it too low on a high refreshed stripe introduces VERY annoying flicker. 
         */
        private float smoothing = (float)Properties.Settings.Default.smoothing;

        // Drop the index to 0 if below this threshold. Helps prevent lingering color after sound
        // has stopped.
        const float minThreshold = 0.001f;

        /* 
         * The number of index points to take from the raw FFT data. Number of columns corresponds
         * with a standard 10 bands equalizers and octaves.
         */

        public const int columns = 10;
        public const int indexes = columns + 1; 

        // FFT fields for CSCore FFT.
        FftProvider fftProvider;
        float[] fftBuf;

        /*
         * Initialize the SampleHandler with the number of audio channels and the sample rate
         * taken from system config.
         */

        // Previous-sample spectrum data, used for smoothing out the output.
        double[] prevSpectrumValues = new double[columns];

        public SampleHandler(int channels)
        {
            // Setup an FFT data provider with number of channels taken from system audio settings, buffer size and
            // a list of FFT results assigned.
            fftProvider = new FftProvider(channels, fftSize);
            fftBuf = new float[fftSizeInt];
            _spectrumData = new List<float>();
        }

        /*
         * Add a single block to the FFT data.
         */
        public void Add(float left, float right)
        {
            fftProvider.Add(left, right);
        }

        /*
         Get the current array of sample data by running the FFT and massaging the output. 
        */ 
        public double[] GetSpectrumValues()
        {

            // Check for no data coming through FFT and send nulls if true
            if (!fftProvider.IsNewDataAvailable)
            {
                // Real-time debug only
                // Console.WriteLine("no new data available");
                return null;
            }

            else

            // Do the FFT
            fftProvider.GetFftData(fftBuf);

            // Assign to frequency bands
            double[] spectrumValues = new double[columns];
           
            // This assigns results kind of properly to 10-band octaves but still have ginormous leakage when presented 
<<<<<<< HEAD
            // with a single frequency. Taken from a bass_wasapi sample.
          
=======
            // with a single frequency. Code taken from https://github.com/m4r1vs/Audioly
            else
           */
>>>>>>> 3a5f55a5f7daacc274c35b26e36423c789449741
            {
                int spectrumColumn, peak;
                int indexTick = 0;
                int fftIdxs = 1023;
                for (spectrumColumn = 0; spectrumColumn < columns; spectrumColumn++)
                {
                    double max = 0;
                    int Idxs = (int)Math.Pow(2, spectrumColumn * 10.0 / (columns - 1));
                            if (Idxs > fftIdxs)
                        Idxs = fftIdxs;
                            if (Idxs <= indexTick) 
                        Idxs = indexTick + 1;
                            for (; indexTick < Idxs; indexTick++)
                            {
                                if (max < fftBuf[1 + indexTick])
                                    max = fftBuf[1 + indexTick];
                            }
                        peak = (int)(Math.Sqrt(max) * 8);

                                // Peak exceeding 0-100 handling.
                            if (peak > 100) peak = 100;
                            if (peak < 0) peak = 0;
                    _spectrumData.Add(peak);
                }

                 for (int i = 0; i < _spectrumData.ToArray().Length; i++)
                 {
                        try
                        {
                            double newSmoothed = prevSpectrumValues[i] * smoothing + _spectrumData[i] * (1 - smoothing);
                            spectrumValues[i] = newSmoothed < minThreshold ? 0 : newSmoothed;
                        }
                        catch (Exception)
                        {
                        }
                 }
            }

            // Real-time debug only
            // Console.WriteLine(string.Join("new ", spectrumValues));

            // Clear the _spectrumData from any previous results for new FFT data.
            _spectrumData.Clear();
            return prevSpectrumValues = spectrumValues;           
        }
    }
}
