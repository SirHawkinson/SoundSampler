using CSCore.DSP;
using System;
using System.Linq;

namespace SoundSampler
{
    public class SampleHandler
    {

        // Basic FFT constants
        const FftSize fftSize = FftSize.Fft4096;
        const int fftSizeInt = (int)fftSize;
        const int maxFftIdx = fftSizeInt / 2 - 1;
        const int minFreq = 20;
        const int maxFreq = 16000;

        /* The weight given to the previous sample for time-based smoothing. High value works great when 
         * sending it to the LED strip ewhen the software is set to a high refresh rate, making the 
         * transition between values much milder, lower values increase accuracy of sampling.
         * Setting it too low on a high refreshed stripe introduces VERY annoying flicker. 
         */
        const float smoothing = 0.85f;

        // Drop the index to 0 if below this threshold. Helps prevent lingering color after sound
        // has stopped
        const float minThreshold = 0.001f;

        /* 
         * The number of index points to take from the raw FFT data. Number of columns corresponds
         * with a standard 10 bands equalizers
         */

        public const int columns = 10;
        public const int indexes = columns + 1; 

        // FFT fields for CSCore FFT
        FftProvider fftProvider;
        float[] fftBuf;

        // FFT index fields
        int minFreqIdx;
        int maxFreqIdx;
        int[] logFreqIdxs = new int[indexes];

        /*
         * Initialize the SampleHandler with the number of audio channels and the sample rate
         * taken from system config
         */

        // Previous-sample spectrum data, used for smoothing out the output
        float[] prevSpectrumValues = new float[columns];

        public SampleHandler(int channels, int sampleRate)
        {

            fftProvider = new FftProvider(channels, fftSize);
            fftBuf = new float[fftSizeInt];

            // Determine a log-based set of FFT indices
            double f = sampleRate / 2;
            maxFreqIdx = Math.Min((int)(maxFreq / f * fftSizeInt / 2) + 1, maxFftIdx);
            minFreqIdx = Math.Min((int)(minFreq / f * fftSizeInt / 2), maxFreqIdx);
            int indexCount = maxFreqIdx - minFreqIdx;

            // Debug only
            /* Console.WriteLine("minFreqIdx=" + minFreqIdx + "; maxFreqIdx="
             *   + maxFreqIdx + "; index count=" + indexCount);
             */

            for (int i = 0; i < indexes; i++)
            {
                logFreqIdxs[i] = (int)((1 - Math.Log(indexes - i, indexes)) * indexCount) + minFreqIdx;
            }
            // Debug only
            // Console.WriteLine(string.Join(" ", logFreqIdxs));
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
        public float[] GetSpectrumValues()
        {

            // Check for no data coming through FFT and send nulls if true
            if (!fftProvider.IsNewDataAvailable)
            {
                // Debug only
                // Console.WriteLine("no new data available");
                return null;
            }

            else
            // Do the FFT
            fftProvider.GetFftData(fftBuf);
            
            // Assign  to frequency bands
            float[] spectrumValues = new float[columns];
            for (int i = 0; i < columns; i++)
            {
                /* Find the max within each frequency band, then apply per-index scaling 
                 *(to bring up the mid-high end) and a minimum threshold
                 */
                int bandSize = logFreqIdxs[i + 1] - logFreqIdxs[i];
                float max = new ArraySegment<float>(fftBuf, logFreqIdxs[i], bandSize).Max();
                float Scaled = Math.Max((float)max, 0);
                float smoothed = prevSpectrumValues[i] * smoothing + Scaled * (1 - smoothing);
                spectrumValues[i] = smoothed < minThreshold ? 0 : smoothed;
            }

            return prevSpectrumValues = spectrumValues;
           
        }
    }
}