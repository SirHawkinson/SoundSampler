using CSCore.DSP;
using System;
using System.Linq;

namespace SoundSampler
{
    public class SampleHandler
    {

        // Basic FFT constants
        const FftSize FFT_SIZE = FftSize.Fft4096;
        const int FFT_SIZE_INT = (int)FFT_SIZE;
        const int MAX_FFT_IDX = FFT_SIZE_INT / 2 - 1;
        const int MIN_FREQ = 20;
        const int MAX_FREQ = 16000;

        /* The weight given to the previous sample for time-based smoothing. High value works great when 
           sending it to the LED stripewhen the software is set to a high refresh rate, making the 
           transition between values much milder, lower values increase accuracy of sampling.
           Setting it too low on a high refreshed stripe introduces VERY annoying flicker. dlbeer 
           (http://dlbeer.co.nz/articles/fftvis.html) recommends setting this based on the
           sample (here, the tick) rate
        */
        const float SMOOTHING = 0.85f;

        // Drop the index to 0 if below this threshold. Helps prevent lingering color after sound
        // has stopped
        const float MIN_THRESHOLD = 0.001f;

        // The number of index points to take from the raw FFT data
        public const int NUM_COLS = 10;
        public const int NUM_IDXS = NUM_COLS + 1; // indexes surround columns

        // FFT fields
        FftProvider fftProvider;
        float[] fftBuf;

        // FFT index fields
        int minFreqIdx;
        int maxFreqIdx;
        int[] logFreqIdxs = new int[NUM_IDXS];

        /*
          Initialize the SampleHandler with the number of audio channels and the sample rate.
          These are used to determine
         */

        // Previous-sample spectrum data
        float[] prevSpectrumValues = new float[NUM_COLS];

        public SampleHandler(int channels, int sampleRate)
        {

            fftProvider = new FftProvider(channels, FFT_SIZE);
            fftBuf = new float[FFT_SIZE_INT];

            // Determine a log-based set of FFT indices
            double f = sampleRate / 2;
            maxFreqIdx = Math.Min((int)(MAX_FREQ / f * FFT_SIZE_INT / 2) + 1, MAX_FFT_IDX);
            minFreqIdx = Math.Min((int)(MIN_FREQ / f * FFT_SIZE_INT / 2), MAX_FFT_IDX);
            int indexCount = maxFreqIdx - minFreqIdx;
            Console.WriteLine("minFreqIdx=" + minFreqIdx + "; maxFreqIdx="
                + maxFreqIdx + "; index count=" + indexCount);

            for (int i = 0; i < NUM_IDXS; i++)
            {
                logFreqIdxs[i] = (int)((1 - Math.Log(NUM_IDXS - i, NUM_IDXS)) * indexCount) + minFreqIdx;
            }
            Console.WriteLine(string.Join(" ", logFreqIdxs));
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
            if (!fftProvider.IsNewDataAvailable)
            {
                Console.WriteLine("no new data available");
                return null;
            }

            else
                // Do the FFT
                fftProvider.GetFftData(fftBuf);
            
         
            float[] spectrumValues = new float[NUM_COLS];
            for (int i = 0; i < NUM_COLS; i++)
            {
                /* Find the max within each frequency band, then apply per-index scaling 
                (to bring up the mid-high end) and a minimum threshold
                */
                int bandSize = logFreqIdxs[i + 1] - logFreqIdxs[i];
                float max = new ArraySegment<float>(fftBuf, logFreqIdxs[i], bandSize).Max();
                float Scaled = Math.Max((float)max, 0);
                float smoothed = prevSpectrumValues[i] * SMOOTHING + Scaled * (1 - SMOOTHING);
                spectrumValues[i] = smoothed < MIN_THRESHOLD ? 0 : smoothed;
            }

            return prevSpectrumValues = spectrumValues;
           
        }
    }
}