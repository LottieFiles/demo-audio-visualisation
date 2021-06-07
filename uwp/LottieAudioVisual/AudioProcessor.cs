using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LottieAudioVisual
{
    class AudioProcessor
    {
        public AudioProcessor(double alpha = 0.9, double preGain = 0.5, double minimum_dB = 45, double maximum_dB = 80)
        {
            this.alpha = alpha;
            this.preGain = preGain;
            this.minimum_dB = minimum_dB;
            this.maximum_dB = maximum_dB;
        }

        private readonly double alpha;
        private readonly double preGain;
        private readonly double minimum_dB;
        private readonly double maximum_dB;

        private double ema = 0;
        
        public double GetLevel(double rms)
        {
            ema = ema * alpha + (1 - alpha) * rms;

            var preGained = preGain * ema;

            return 20 * Math.Log10(preGained);
        }

        public double GetProgress(double dB)
        {
            //80 => 0
            //30 => 1
            //45 => 
            
            //Clip
            dB = dB < maximum_dB ? dB : maximum_dB;
            dB = dB > minimum_dB ? dB : minimum_dB;

            double numerator = dB - minimum_dB;
            double denominator = maximum_dB - minimum_dB;
            double percentage = numerator / denominator;
            return percentage;
        }
    }
}
