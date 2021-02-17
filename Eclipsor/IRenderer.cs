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
        void Render(IPointObject obj, int time, double angle, DistPoint[,] dists, double[] flux);
    }

    public static class RendererHelper
    {
        public struct StarMoved
        {
            public Star origStar;
            public Geometry.Point center;
            public double radius { get { return origStar.sphere.radius; } }
        }

        private static Dictionary<IPointObject, IList<Star>> stars = new Dictionary<IPointObject, IList<Star>>();

        public static IList<Star> GetStars(IPointObject obj)
        {
            IList<Star> result;
            if (stars.TryGetValue(obj, out result))
            { return result; }

            result = new List<Star>();
            stars[obj] = result;

            FillStars(obj, result);

            return result;
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
                var star = new StarMoved()
                {
                    origStar = origStar,
                    center = new Geometry.Point()
                    {
                        x = origStar.center.x,
                        y = origStar.center.y * cos - origStar.center.z * sin,
                        z = -origStar.center.y * sin - origStar.center.z * cos
                    }
                };
                stars.Add(star);
            }
            return stars;
        }

        public static void GetBoundingBox(IPointObject obj, double angle, out double width, out double height)
        {
            GetBoundingBox(GetAngledStars(obj, angle), out width, out height);
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
