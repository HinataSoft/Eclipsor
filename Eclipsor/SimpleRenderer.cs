using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipsor
{
    public class SimpleRenderer : IRenderer
    {
        private int nsize;

        public SimpleRenderer(int nsize)
        {
            this.nsize = nsize;
        }

        public void Render(IPointObject obj, int time, double angle, DistPoint[,] dists, double[] flux)
        {
            DistPoint.Reset(dists);

            var stars = RendererHelper.GetAngledStars(obj, angle);
            stars.Sort((a, b) => a.center.y.CompareTo(b.center.y));

            double width, height;
            RendererHelper.GetBoundingBox(stars, out width, out height);

            // +++ HACK
            //width = 35;
            //height = 10;
            // -- HACK

            int xxmax = dists.GetLength(0);
            int yymax = dists.GetLength(1);
            double zoomX = xxmax / width;
            double zoomY = yymax / height;
            double zoom = Math.Min(zoomX, zoomY);
            int xxoffs = xxmax / 2;
            int yyoffs = yymax / 2;
            double brightness = 0;

            foreach (RendererHelper.StarMoved star in stars)
            {
                int x1 = (int)((star.center.x - star.radius) * zoom + xxoffs);
                int x2 = (int)((star.center.x + star.radius) * zoom + xxoffs + 1);
                int y1 = (int)((star.center.z - star.radius) * zoom + yyoffs);
                int y2 = (int)((star.center.z + star.radius) * zoom + yyoffs + 1);

                for (int yy = Math.Max(y1, 0); yy < Math.Min(y2, yymax); yy++)
                {
                    for (int xx = Math.Max(x1, 0); xx < Math.Min(x2, xxmax); xx++)
                    {
                        //if (!Double.IsInfinity(dists[xx, yy].dist))
                        //{ continue; }

                        double x = (xx - xxoffs) / zoom;
                        double z = (yy - yyoffs) / zoom;

                        double dist = Extensions.Distance(x - star.center.x, z - star.center.z);
                        if ((dist <= star.radius))
                        {
                            double theta = Math.Asin(dist / star.radius);
                            double cosTheta = Math.Cos(theta);

                            //double ydist = star.center.y - dist;
                            double ydist = star.center.y - star.radius * cosTheta;
                            if (dists[xx, yy].dist < ydist)
                            { continue; }

                            double br = Physics.LimbDarkening(star.origStar.exitance, cosTheta);
                            dists[xx, yy].dist = ydist;
                            dists[xx, yy].brightness = br;
                            brightness += br;
                        }

                    }
                }
            }

            #region Naivni

            /*
            foreach (Star star in stars)
            {
                for (int yy = 0; yy < yymax; yy++)
                {
                    for (int xx = 0; xx < xxmax; xx++)
                    {
                        double x = (xx - xxoffs) / zoom;
                        double z = (yy - yyoffs) / zoom;

                        double dist = Extensions.Distance(x - star.x, z);
                        double ydist = star.y;
                        if ((dist <= star.radius) && (dists[xx, yy].dist > ydist))
                        {
                            double theta = dist / star.radius;
                            dists[xx, yy].dist = ydist;
                            dists[xx, yy].brightness = (int)(Math.Cos(theta * Math.PI / 2) * 200) + 50;
                        }

                    }
                }
            }
            for (int yy = 0; yy < yymax; yy++)
            {
                for (int xx = 0; xx < xxmax; xx++)
                { brightness += dists[xx, yy].brightness; }
            }
            */

            #endregion

            flux[time] = brightness / (zoom * zoom);
        }
    }
}
