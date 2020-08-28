using System;
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
        public double entropy = Properties.Settings.Default.entropy;
        public double maxSeenEver = 0;
        int height = 100;

        /*
        * Handling of raw (massaged) FFT'ed spectrum data. 
        */
       
        public void SendData(double[] raw)
        {
            double[] normalized = Normalize(raw);
            int filtered = Filter(normalized);
            // Atrocious, but real-time debug only
            // Console.WriteLine(string.Join(" Handler ", normalized));
            // Console.WriteLine("Normalized: " + filtered);

            // Send filtered column to COM
            SamplerApp samp = new SamplerApp();
            samp.COMSend(filtered);
        }
        
        /*
         * Filter columns to get a highest column, rounded to integer
         */
        private int Filter(double[] normalized)
        {
            return Convert.ToInt32(normalized.Max());
        }

        
        /*
        * Normalize the raw data into values between 0 and the something. The max value is subject to entropy so large spikes don't
        * ruin the cool.
        */
        private double[] Normalize(double[] raw)
        {
            double[] normalized = new double[raw.Length];

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
           
            
        

        // Apply corrector, will be used once I get a new LED stripe.

        // Origin: https://www.codeproject.com/Articles/1090765/Octave-bands-and-auditory-filters-in-acoustics

        double RA(double f)
        {
           double a1 = Math.Pow(f, 2) + Math.Pow(20.6, 2);
           double a2 = Math.Sqrt((Math.Pow(f, 2) + Math.Pow(107.7, 2)) * (Math.Pow(f, 2) + Math.Pow(737.9, 2)));
           double a3 = Math.Pow(f, 2) + Math.Pow(12200, 2);
           return (Math.Pow(12200, 2) * Math.Pow(f, 4)) / (a1 * a2 * a3);
        }
      
    }
}