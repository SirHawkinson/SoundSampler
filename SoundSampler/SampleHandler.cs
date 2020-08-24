using CSCore.DSP;
using System;
//  using System.Linq;
using System.Collections.Generic;

namespace SoundSampler
{
    
    public class SampleHandler
    {
        // Different data handling approach.
        public List<float> _spectrumData;

        // Basic FFT constants.
        const FftSize fftSize = FftSize.Fft2048;
        const int fftSizeInt = (int)fftSize;

        // Legacy method.
        /*
         const int maxFftIdx = fftSizeInt / 2 - 1;
        const int minFreq = 20;
        const int maxFreq = 16000;
        */

        /* The weight given to the previous sample for time-based smoothing. High value works great when 
         * sending it to the LED strip ewhen the software is set to a high refresh rate, making the 
         * transition between values much milder, lower values increase accuracy of sampling.
         * Setting it too low on a high refreshed stripe introduces VERY annoying flicker. 
         */
        const float smoothing = 0.85f;

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
        // FFT index fields. Legacy method.
        int minFreqIdx;
        int maxFreqIdx;
        int[] logFreqIdxs = new int[indexes];
        */

        /*
         * Initialize the SampleHandler with the number of audio channels and the sample rate
         * taken from system config.
         */

        // Previous-sample spectrum data, used for smoothing out the output.
        float[] prevSpectrumValues = new float[columns];

        public SampleHandler(int channels, int sampleRate)
        {
            // Setup an FFT data provider with number of channels taken from system audio settings, buffer size and
            // a list of FFT results assigned.

            fftProvider = new FftProvider(channels, fftSize);
            fftBuf = new float[fftSizeInt];
            _spectrumData = new List<float>();
            /*
                // Determine a log-based set of FFT indices. Legacy method.
                double f = sampleRate / 2;
                maxFreqIdx = Math.Min((int)(maxFreq / f * fftSizeInt / 2) + 1, maxFftIdx);
                minFreqIdx = Math.Min((int)(minFreq / f * fftSizeInt / 2), maxFreqIdx);
                int indexCount = maxFreqIdx - minFreqIdx;

                for (int i = 0; i < indexes; i++)
                {
                    logFreqIdxs[i] = (int)((1 - Math.Log(indexes - i, indexes)) * indexCount) + minFreqIdx;
                }
            */
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
        public float[] GetSpectrumValues(string method)
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
            float[] spectrumValues = new float[columns];

            // Column assigning method, by default it will do the "octave" assigning. Seems like this method must be ran right after WASAPI
            // initialization, else it has a ground level output. Left as a legacy method as it's still fun.
           /*
            if (method == "bass") 
            {                
                for (int i = 0; i < columns; i++)
                 {
                    // Mostly bass detection, most likely borked tremendously but still gives interesting results. Or just how strong bass 
                    // frequency is, as a signal (Eiffle tower sized doubts). Based on https://github.com/glowboy/SpectrumLED

                 int bandSize = logFreqIdxs[i + 1] - logFreqIdxs[i];
                 float max = new ArraySegment<float>(fftBuf, logFreqIdxs[i], bandSize).Max();
                 float Scaled = Math.Max((float)max, 0);
                 float smoothed = prevSpectrumValues[i] * smoothing + Scaled * (1 - smoothing);
                 spectrumValues[i] = smoothed < minThreshold ? 0 : smoothed;
                 }
            }
           
            // This assigns results kind of properly to 10-band octaves but still have ginormous leakage when presented 
            // with a single frequency. Code taken from https://github.com/m4r1vs/Audioly
            else
           */
            {
                int spectrumColumn, peak;
                int indexTick = 0;
                int fftIdxs = 1023;
                for (spectrumColumn = 0; spectrumColumn < columns; spectrumColumn++)
                {
                    float max = 0;
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
                            float newSmoothed = prevSpectrumValues[i] * smoothing + _spectrumData[i] * (1 - smoothing);
                            spectrumValues[i] = newSmoothed < minThreshold ? 0 : newSmoothed;
                        }
                        catch (Exception)
                        {
                        }
                 }
            }

            // Real-time debug only
            // Console.WriteLine(string.Join("new ", spectrumValues));

            // Clear the _spectrumData from any previous results, for new FFT data.
            _spectrumData.Clear();
            return prevSpectrumValues = spectrumValues;           
        }
    }
}
