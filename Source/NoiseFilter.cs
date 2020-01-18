using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTAVTelemetry
{
    public class NoiseFilter
    {
        private float[] samples;
        private int maxSampleCount;
        private int liveSampleCount;
        private int currSample = 0;
        private float maxInputDelta = float.MaxValue; //maximum input delta between last and new sample.

        //higher _maxSampleCount = more smoothing
        public NoiseFilter(int _maxSampleCount, float _maxInputDelta=float.MaxValue) 
        {
            maxSampleCount = Math.Max(1, _maxSampleCount);
            samples = new float[maxSampleCount];
            maxInputDelta = _maxInputDelta;
        }

        public float Filter(float sample)
        {
            //early out
            if (maxSampleCount == 1)
                return sample;

            if(maxInputDelta != float.MaxValue && liveSampleCount > 0)
            {
                float sampleDiff = sample - samples[currSample];
                float absSampleDiff = Math.Abs(sampleDiff);
                if(absSampleDiff > maxInputDelta)
                {
                    float direction = sampleDiff / absSampleDiff;

                    sample = samples[currSample] + (direction * maxInputDelta);
                }
            }

            samples[currSample] = sample;

            liveSampleCount = (liveSampleCount + 1) >= maxSampleCount ? maxSampleCount : liveSampleCount + 1;

            //average all samples
            float total = 0.0f;
            for (int i = 0; i < liveSampleCount; ++i)
            {
                total += samples[i];
            }

            currSample = (currSample + 1) >= maxSampleCount ? 0 : currSample + 1;

            return total / liveSampleCount;
        }

        public void Reset()
        {
            currSample = 0;
            liveSampleCount = 0;
        }
    }

    public class KalmanFilter
    {
        private float A, H, Q, R, P, x;

        public KalmanFilter(float A, float H, float Q, float R, float initial_P, float initial_x)
        {
            this.A = A;
            this.H = H;
            this.Q = Q;
            this.R = R;
            this.P = initial_P;
            this.x = initial_x;
        }

        public float Filter(float input)
        {
            // time update - prediction
            x = A * x;
            P = A * P * A + Q;

            // measurement update - correction
            float K = P * H / (H * P * H + R);
            x = x + K * (input - H * x);
            P = (1 - K * H) * P;

            return x;
        }
    }
}
