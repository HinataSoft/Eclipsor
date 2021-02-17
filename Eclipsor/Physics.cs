using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipsor
{
    public class Physics
    {
        public static double LimbDarkening(double I0, double cosTheta)
        {
            return (0.3 + cosTheta * 0.93 - cosTheta * cosTheta * 0.23) * I0;
            //return (1 - 0.5 * (1 - cosTheta)) * I0;
            //return (1 - 0.999 * (1 - cosTheta)) * I0;
        }
    }
}
