using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipsor
{
    public struct DistPoint
    {
        public double dist;
        public double brightness;

        public static void Reset(DistPoint[,] dists)
        {
            for (int y = 0; y < dists.GetLength(1); y++)
            {
                for (int x = 0; x < dists.GetLength(0); x++)
                {
                    dists[x, y].dist = double.PositiveInfinity;
                    dists[x, y].brightness = 0;
                }
            }
        }
    }

    public interface IRenderer
    {
        void Render(List<RendererHelper.StarMoved> stars, DistPoint[,] dists, double[] flux, int fluxIndex);
    }

    public static class RendererHelper
    {
        public class StarMoved
        {
            public Geometry.Sphere sphere;
            public Geometry.Point center => sphere.center;
            public double radius => sphere.radius;
            public double exitance;

            public StarMoved(double radius, double exitance, double x, double y, double z)
            {
                this.sphere = new Geometry.Sphere() { center = new Geometry.Point(x: x, y: y, z: z ), radius = radius };
                this.exitance = exitance;
            }
        }

        private static readonly object starsLockObject = new object();
        private static Dictionary<IPointObject, IList<Star>> stars = new Dictionary<IPointObject, IList<Star>>();

        public static IList<Star> GetStars(IPointObject obj)
        {
            lock (starsLockObject)
            {
                IList<Star> result;
                if (stars.TryGetValue(obj, out result))
                { return result; }

                result = new List<Star>();
                stars[obj] = result;

                FillStars(obj, result);

                return result;
            }
        }

        private static void FillStars(IPointObject obj, IList<Star> result)
        {
            if (obj is Binary)
            {
                Binary bin = (Binary)obj;
                FillStars(bin.o1.point, result);
                FillStars(bin.o2.point, result);
            }
            if (obj is Star)
            { result.Add((Star)obj); }
        }

        public static List<StarMoved> GetAngledStars(IPointObject obj, double angle)
        {
            var stars = new List<StarMoved>();
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);
            foreach (Star origStar in RendererHelper.GetStars(obj))
            {
                var star = new StarMoved(
                    radius: origStar.radius,
                    exitance: origStar.exitance,
                    x: origStar.center.x,
                    y: origStar.center.y * cos - origStar.center.z * sin,
                    z: -origStar.center.y * sin - origStar.center.z * cos
                );
                stars.Add(star);
            }
            return stars;
        }

        public static List<StarMoved> GetAngledStars(IPointObject obj, double time, double angle)
        {
            var stars = new List<StarMoved>();
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);
            foreach (StarMoved origStar in obj.GetStarsMoved(time, 0, 0, 0))
            {
                var star = new StarMoved(
                    radius: origStar.radius,
                    exitance: origStar.exitance,
                    x: origStar.center.x,
                    y: origStar.center.y * cos - origStar.center.z * sin,
                    z: -origStar.center.y * sin - origStar.center.z * cos
                );
                stars.Add(star);
            }
            return stars;
        }

        public static void GetBoundingBox(List<StarMoved> starsMovedList, out double width, out double height)
        {
            width = 0;
            height = 0;
            foreach (StarMoved star in starsMovedList)
            {
                double x = Math.Max(Math.Abs(star.center.x - star.radius), Math.Abs(star.center.x + star.radius));
                double y = Math.Max(Math.Abs(star.center.z - star.radius), Math.Abs(star.center.z + star.radius));
                if (x > width)
                { width = x; }
                if (y > height)
                { height = y; }
            }
            width *= 2;
            height *= 2;
        }

        public static void GetBoundingBoxTop(IPointObject obj, out double width, out double height)
        {
            width = 0;
            height = 0;
            foreach (Star star in GetStars(obj))
            {
                double x = Math.Max(Math.Abs(star.center.x - star.radius), Math.Abs(star.center.x + star.radius));
                double y = Math.Max(Math.Abs(star.center.y - star.radius), Math.Abs(star.center.y + star.radius));
                if (x > width)
                { width = x; }
                if (y > height)
                { height = y; }
            }
            width *= 2;
            height *= 2;
        }
    }
}
