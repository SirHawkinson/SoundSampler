using System;
using System.Diagnostics;
using System.Linq;
namespace SoundSampler
{
    public class Handler
    {
        /* 
        *  During normalization, scale the FFT values by the maximum value seen to get nice,
        *  mostly-mid-ranged values. Reduce the maximum ever seen with each tick so giant spikes
        *  don't make the pretty colors disappear
        */
        public const float entropy = 0.999f;
        float maxSeenEver = 0;
        int height = 100;
       
        /*
        * Handling of raw (massaged) FFT'ed spectrum data. 
        */
        public void SendData(float[] raw)
        {
            float[] normalized = Normalize(raw);
            int filtered = Filter(normalized);
            
            // Atrocious, but debug only
            /* Console.WriteLine("post stuff" +
                " 1= " + Convert.ToInt32(normalized[0]) + " 2= " + Convert.ToInt32(normalized[1]) + " 3= " + Convert.ToInt32(normalized[2]) + " 4= " + Convert.ToInt32(normalized[3]) + " 5= " + Convert.ToInt32(normalized[4]) + " 6= " + Convert.ToInt32(normalized[5]) + " 7= " + Convert.ToInt32(normalized[6]) +
                " 8= " + Convert.ToInt32(normalized[7]) + " 9= " + Convert.ToInt32(normalized[8]) + " 10= " + Convert.ToInt32(normalized[9]) + " filt " + filtered);
            */

            // Send filtered column to COM
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
                normalized[i] = raw[i] / maxSeenEver * height;
             }

        // Slowly decrease maxEverSeen to keep things normalizing after a giant spike
        maxSeenEver *= entropy;
            return normalized;
        }
    }
}