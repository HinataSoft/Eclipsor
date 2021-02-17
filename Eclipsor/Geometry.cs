using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipsor
{
    public class Geometry
    {
        public class Point
        {
            public readonly double x;
            public readonly double y;
            public readonly double z;

            public Point(double x, double y, double z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public Point GetPointByAdding(Vector v)
            {
                return new Point(
                    x: x + v.dx,
                    y: y + v.dy,
                    z: z + v.dz
                );
            }

            public Point GetPointByAdding(Vector v, double koef)
            {
                return new Point(
                    x: x + v.dx * koef,
                    y: y + v.dy * koef,
                    z: z + v.dz * koef
                );
            }

            public override string ToString()
            {
                return "[" + x.ToString("F3") + ", " + y.ToString("F3") + ", " + z.ToString("F3") + "]";
            }
        }

        public class Vector
        {
            public readonly double dx;
            public readonly double dy;
            public readonly double dz;
            public readonly bool unity;

            public Vector(Vector v)
            {
                dx = v.dx;
                dy = v.dy;
                dz = v.dz;
                unity = v.unity;
            }

            public Vector(Point p1, Point p2)
            {
                dx = p2.x - p1.x;
                dy = p2.y - p1.y;
                dz = p2.z - p1.z;
                unity = false;
            }

            public Vector(double dx, double dy, double dz, bool unity = false)
            {
                this.dx = dx;
                this.dy = dy;
                this.dz = dz;
                this.unity = unity;
            }

            public double Size
            {
                get { return unity ? 1 : Extensions.Distance(dx, dy, dz); }
            }

            public double Size2
            {
                get { return unity ? 1 : Extensions.Distance2(dx, dy, dz); }
            }

            public Vector Normalize()
            {
                double size = Size;
                return new Vector(
                    dx: dx / size,
                    dy: dy / size,
                    dz: dz / size,
                    unity: true
                );
            }

            public Vector Add(Vector v2)
            {
                return new Vector(
                    dx: dx + v2.dx,
                    dy: dy + v2.dy,
                    dz: dz + v2.dz
                );
            }

            public Vector Subtract(Vector v2)
            {
                return new Vector(
                    dx: dx - v2.dx,
                    dy: dy - v2.dy,
                    dz: dz - v2.dz
                );
            }

            public Vector Divide(double factor)
            {
                return new Vector(
                    dx: dx / factor,
                    dy: dy / factor,
                    dz: dz / factor
                );
            }

            public Vector Multiply(double factor)
            {
                return new Vector(
                    dx: dx * factor,
                    dy: dy * factor,
                    dz: dz * factor
                );
            }

            public override string ToString()
            {
                return "(" + dx.ToString("F3") + ", " + dy.ToString("F3") + ", " + dz.ToString("F3") + ")";
            }

            public static Vector Subtract(Vector v1, Vector v2)
            {
                return new Vector(
                    dx: v1.dx - v2.dx,
                    dy: v1.dy - v2.dy,
                    dz: v1.dz - v2.dz
                );
            }
        }

        public class Line
        {
            public Point origin;
            public Vector vector;

            public Line(Point origin, Vector vector)
            {
                this.origin = origin;
                this.vector = vector;
            }

            public Point GetPoint(double factor)
            {
                return new Point(
                    x: origin.x + factor * vector.dx,
                    y: origin.y + factor * vector.dy,
                    z: origin.z + factor * vector.dz
                );
            }

            public Point GetClosestPointTo(Point p)
            {
                double u = ((p.x - origin.x) * vector.dx + (p.y - origin.y) * vector.dy + (p.z - origin.z) * vector.dz) / vector.Size;
                return GetPoint(u);
            }

            public override string ToString()
            {
                return origin.ToString() + " + u * " + vector.ToString();
            }
        }

        public class Sphere
        {
            public Point center;
            public double radius;

            public Geometry.Vector GetNormal(Geometry.Point p)
            {
                var v = new Geometry.Vector(center, p);
                return v.Normalize();
            }

            public override string ToString()
            {
                return center.ToString() + "/" + radius.ToString("F3");
            }
        }

        public static double DotProduct(Vector v1, Vector v2)
        {
            return v1.dx * v2.dx + v1.dy * v2.dy + v1.dz * v2.dz;
        }

        public static double CosAngle(Vector v1, Vector v2)
        {
            return DotProduct(v1, v2) / (v1.Size * v2.Size);
        }

        public static int GetIntersections(Line l, Sphere s, out double p1, out double p2)
        {
            return GetIntersections(l.origin, l.vector, s, out p1, out p2);
        }

        public static double GetIntersectionMin(Point origin, Vector vector, Sphere s)
        {
            double p1;
            double p2;
            int results = GetIntersections(origin, vector, s, out p1, out p2);

            if (results == 0)
            { return double.NaN; }

            if ((p1 < 0) && (p2 < 0))
            { return double.NegativeInfinity; }

            double p;
            if (p1 < 0)
            { p = p2; }
            else if (p2 < 0)
            { p = p1; }
            else
            { p = Math.Min(p1, p2); }

            return p;
        }

        public static int GetIntersections(Point origin, Vector vector, Sphere s, out double p1, out double p2)
        {
            double a = vector.dx * vector.dx + vector.dy * vector.dy + vector.dz * vector.dz;
            double b = 2 * vector.dx * (origin.x - s.center.x) +
                       2 * vector.dy * (origin.y - s.center.y) +
                       2 * vector.dz * (origin.z - s.center.z);
            double c = s.center.x * s.center.x + s.center.y * s.center.y + s.center.z * s.center.z +
                       origin.x * origin.x + origin.y * origin.y + origin.z * origin.z -
                       2 * (s.center.x * origin.x + s.center.y * origin.y + s.center.z * origin.z) -
                       s.radius * s.radius;

            double disc = b * b - 4 * a * c;

            if (disc < 0)
            {
                p1 = double.NaN;
                p2 = double.NaN;
                return 0;
            }

            if (disc == 0)
            {
                p1 = -b / (2 * a);
                p2 = p1;
                return 1;
            }

            double discSqrt = Math.Sqrt(disc);
            p1 = (-b + discSqrt) / (2 * a);
            p2 = (-b - discSqrt) / (2 * a);

            return 2;
        }
    }
}   
