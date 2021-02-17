using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipsor
{
    public static class Extensions
    {
        public static double PhaseToRad(this double phase)
        {
            return phase * 2 * Math.PI;
        }

        public static double DegToRad(this double phase)
        {
            return phase * Math.PI / 180;
        }

        public static double Distance(double a, double b)
        {
            return Math.Sqrt(a * a + b * b);
        }

        public static double Distance2(double a, double b)
        {
            return a * a + b * b;
        }

        public static double Distance(double a, double b, double c)
        {
            return Math.Sqrt(a * a + b * b + c * c);
        }

        public static double Distance2(double a, double b, double c)
        {
            return a * a + b * b + c * c;
        }
    }

    public interface IPointObject
    {
        void PlaceInTime(double time);
        void SetOrigin(double x, double y);
    }

    public class Star : IPointObject
    {
        public const double sigma = 5.67e-8; // Stefan-Boltzmann constant
        public const double tSun = 5778; // Kelvin

        public Geometry.Sphere sphere = new Geometry.Sphere();

        public double intensity;
        public double exitance;

        /// <param name="radius">in Sun radii</param>
        /// <param name="temperature">in Kelvin</param>
        public Star(double radius, double temperature)
        {
            this.sphere.radius = radius;
            double area = 4 * Math.PI * radius * radius;
            double t_tS = temperature / tSun;
            this.intensity = radius * radius * t_tS * t_tS * t_tS * t_tS;
            this.exitance = intensity / area;
        }

        public double radius
        {
            get { return sphere.radius; }
        }

        public Geometry.Point center
        {
            get { return sphere.center; }
        }

        public void PlaceInTime(double time)
        {
        }

        public void SetOrigin(double x, double y)
        {
            sphere.center = new Geometry.Point()
            {
                x = x,
                y = y,
                z = 0
            };
        }

        public override string ToString()
        {
            return sphere.ToString();
        }
    }

    public class Orbit
    {
        public IPointObject point;
        public double radius;
        public double phase0;

        public Orbit(IPointObject point)
        {
            this.point = point;
        }

        public void Place(double time, double phase, Binary parent)
        {
            phase += phase0;

            point.SetOrigin(
                Math.Sin(phase.PhaseToRad()) * radius + parent.x,
                Math.Cos(phase.PhaseToRad()) * radius + parent.y
            );
            point.PlaceInTime(time);
        }
    }

    public class Binary : IPointObject
    {
        public double x;
        public double y;
        public Orbit o1;
        public Orbit o2;
        public double period;
        public double phase0;

        public Binary(IPointObject p1, IPointObject p2)
        {
            this.o1 = new Orbit(p1);
            this.o2 = new Orbit(p2);
        }

        public void PlaceInTime(double time)
        {
            double phase = time / period + phase0;
            o1.Place(time, phase, this);
            o2.Place(time, phase + 0.5, this);
        }

        public void SetOrigin(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
