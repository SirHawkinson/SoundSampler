using System;
using System.Linq;
namespace SoundSampler
{
    public class Handler
    {
        /* 
        *  During normalization, scale the FFT values by the maximum value seen for smooth
        *  operation. Maximum value in current iteration gets multiplied by the entropy
        *  value, so the transition with each tick is smooth.
        */
        private double entropy = Properties.Settings.Default.entropy;
        public double maxSeenEver = 0;
        int height = 100;

        
        /*
        * Handling of raw (massaged) FFT'ed spectrum data. 
        */

        public void SendData(double[] raw, bool bassBased)
        {
            double[] normalized = Normalize(raw, bassBased);
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
        private double[] Normalize(double[] raw, bool bass)
        {
            
            // Apply 3-column normalization
            if (bass == true) 
        {                
            int bassBasedColumns = 3;
            double[] normalized = new double[bassBasedColumns];

                // Use maxSeenEver to normalize the range into 0-Height
                maxSeenEver = (double)Math.Max(raw.Max(), maxSeenEver);

            for (int i = 0; i < bassBasedColumns; i++)
            {
                normalized[i] = raw[i] / maxSeenEver * height;
            }
            maxSeenEver *= entropy;
                
            return normalized;
        }

            // Apply octaves based normalization
        else
        {
                double[] normalized = new double[raw.Length];

              // Use maxSeenEver to normalize the range into 0-Height
            maxSeenEver = Math.Max(raw.Max(), maxSeenEver);

            for (int i = 0; i < raw.Length; i++)
                {
                    normalized[i] = raw[i] / maxSeenEver * height;
                }
            maxSeenEver *= entropy;
            return normalized;
            }
            // Slowly decrease maxEverSeen to keep things normalizing after a giant spike
            
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