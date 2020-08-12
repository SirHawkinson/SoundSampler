using System;
using System.Linq;
using System.IO.Ports;
namespace SoundSampler
{
    public class Handler
    {


        // During normalization, scale the FFT values by the maximum value seen to get nice,
        // mostly-mid-ranged values. Reduce the maximum ever seen with each tick so giant spikes
        // don't make the pretty colors disappear
        const float MAX_ENTROPY = 0.999f;
        float maxSeenEver = 0;
        int Height = 100;
       
        /*
        * Handling of raw (massaged) FFT'ed spectrum data. 
        */
        public void DataSend(float[] raw)
        {
            float[] normalized = Normalize(raw);
            int filtered = Filter(normalized);
            //COM handling, self explanatory
            SamplerApp samp = new SamplerApp();
            samp.COMSend(filtered);
        }
        
        /*
         * Filter columns to get a highest column, rounded to integer
         */
        private int Filter(float[] normalized)
        {
            return Convert.ToInt32(normalized.Max());
        }
     
        /*
        * Normalize the raw data into values between 0 and the something. The max value is subject to entropy so large spikes don't
        * ruin the cool.
        */
        private float[] Normalize(float[] raw)
        {
            float[] normalized = new float[raw.Length];

            // Use maxSeenEver to normalize the range into 0-Height
            maxSeenEver = Math.Max(raw.Max(), maxSeenEver);

            for (int i = 0; i < raw.Length; i++)
            {
                normalized[i] = raw[i] / maxSeenEver * Height;
            }

            // Slowly decrease maxEverSeen to keep things normalizing after a giant spike
           maxSeenEver *= MAX_ENTROPY;
            
            return normalized;
        }
    }
}