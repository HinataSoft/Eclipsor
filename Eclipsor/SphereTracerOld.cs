using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipsor
{
    class SphereTracerOld : IRenderer
    {
        private int nsize;

        public SphereTracerOld(int nsize)
        {
            this.nsize = nsize;
        }

        public void Render(IPointObject obj, int time, double angle, DistPoint[,] dists, double[] flux)
        {
            DistPoint.Reset(dists);

            var stars = RendererHelper.GetStars(obj);

            int xxmax = dists.GetLength(0);
            int yymax = dists.GetLength(1);
            float zoom = xxmax * 1f / nsize;
            int xxoffs = xxmax / 2;
            int yyoffs = yymax / 2;
            double brightness = 0;

            for (int yy = 0; yy < yymax; yy++)
            {
                for (int xx = 0; xx < xxmax; xx++)
                {
                    double x = (xx - xxoffs) / zoom;
                    double y = -100;
                    double z = (yy - yyoffs) / zoom;

                    double dx = 0;
                    double dy = 1;
                    double dz = 0;

                    Star star;
                    double t = FindIntersection(stars, null, x, y, z, dx, dy, dz, out star);

                    if (star != null)
                    {
                        dists[xx, yy].dist = t;

                        double px = x + t * dx;
                        double py = y + t * dy;
                        double pz = z + t * dz;
                        double nx = (px - star.center.x) / star.radius;
                        double ny = (py - star.center.y) / star.radius;
                        double nz = (pz - star.center.z) / star.radius;

                        double cosTheta = (dx * nx + dy * ny + dz * nz); // both == 1 / (Extensions.Distance(dx, dy, dz) * Extensions.Distance(nx, ny, nz));
                        double br = Physics.LimbDarkening(star.exitance, cosTheta);

                        Star star2;
                        double t2 = FindIntersection(stars, star, px, py, pz, -nx, -ny, -nz, out star2);
                        if (star2 != null)
                        {
                            double br2 = star2.exitance / (t2 * t2);
                            br += br2;
                        }

                        //double u = -b / 2 / a;
                        //double theta = Extensions.Distance(x + dx * u - star.x, y + dy * u - star.y, z + dz * u - star.z) / star.radius;
                        //int br = (int)(Math.Cos(theta * Math.PI / 2) * 200) + 50;

                        dists[xx, yy].brightness = br;
                        brightness += br;
                    }
                }
            }

            flux[time] = brightness;
        }

        private static double FindIntersection(IList<Star> stars, Star skipStar, double x, double y, double z, double dx, double dy, double dz, out Star outStar)
        {
            outStar = null;
            double minT = double.PositiveInfinity;
            foreach (Star star in stars)
            {
                if (star == skipStar)
                { continue; }

                double a = dx * dx + dy * dy + dz * dz;
                double b = 2 * dx * (x - star.center.x) + 2 * dy * (y - star.center.y) + 2 * dz * (z - star.center.z);
                double c = star.center.x * star.center.x + star.center.y * star.center.y + star.center.z * star.center.z + x * x + y * y + z * z -
                            2 * (star.center.x * x + star.center.y * y + star.center.z * z) - star.radius * star.radius;
                double disc = b * b - 4 * a * c;
                if (disc < 0)
                { continue; }

                double t1 = (-b + Math.Sqrt(disc)) / (2 * a);
                double t2 = (-b + Math.Sqrt(disc)) / (2 * a);
                double t = Math.Min(t1, t2);

                if ((t > 0) && (t < minT))
                {
                    minT = t;
                    outStar = star;
                }
            }
            return minT;
        }
    }
}