using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTAVTelemetry
{
    class Tools
    {
        public static float ValidateNAN(float value, float valueIfNan)
        {
            return float.IsNaN(value) ? valueIfNan : value;
        }
    }
}
