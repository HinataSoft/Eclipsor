using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipsor
{
    class SphereTracer : IRenderer
    {
        public SphereTracer()
        {
        }

        public void Render(List<RendererHelper.StarMoved> stars, DistPoint[,] dists, double[] flux, int fluxIndex)
        {
            DistPoint.Reset(dists);

            double width, height;
            RendererHelper.GetBoundingBox(stars, out width, out height);

            // +++ HACK
            //width = 45;
            //height = 10;
            // -- HACK

            double viewAngle = 0;

            int xxmax = dists.GetLength(0);
            int yymax = dists.GetLength(1);
            double zoomX = xxmax / width;
            double zoomY = yymax / height;
            double zoom = Math.Min(zoomX, zoomY);
            int xxoffs = xxmax / 2;
            int yyoffs = yymax / 2;
            double brightness = 0;

            double wdy = Math.Cos(viewAngle);
            double wdz = Math.Sin(viewAngle);

            double woy = -200 * wdy;
            double woz = -200 * wdz;

            for (int yy = 0; yy < yymax; yy++)
            {
                double y = (yy - yyoffs) / zoom;

                for (int xx = 0; xx < xxmax; xx++)
                {
                    Geometry.Line ray = new Geometry.Line(
                        origin: new Geometry.Point(
                            x: (xx - xxoffs) / zoom,
                            y: woy - y * wdz,
                            z: woz + y * wdy
                        ),
                        vector: new Geometry.Vector(
                            dx: 0,
                            dy: wdy,
                            dz: wdz
                        )
                    );

                    RendererHelper.StarMoved star;
                    double t = FindIntersection(stars, null, ray, out star);

                    if (star != null)
                    {
                        dists[xx, yy].dist = t;
                        Geometry.Point hitPoint = ray.GetPoint(t);
                        Geometry.Vector normal = star.sphere.GetNormal(hitPoint);
                        Geometry.Line view = new Geometry.Line(hitPoint, normal);
                        double cosTheta = -Geometry.DotProduct(ray.vector, normal); // should be Geometry.CosAngle(...) but as both vectors are normalized this is the same
                        double br = Physics.LimbDarkening(star.exitance, cosTheta);

                        // ILLUMINATION
                        {
                            foreach (RendererHelper.StarMoved star2 in stars)
                            {
                                if (star2 == star)
                                { continue; }

                                Geometry.Vector toStar2 = new Geometry.Vector(hitPoint, star2.center);
                                double toStar2Size = toStar2.Size;
                                toStar2 = toStar2.Normalize();
                                double dp = Geometry.DotProduct(toStar2, normal);
                                if (dp > 0)
                                {
                                    double shadow = double.PositiveInfinity;
                                    foreach (RendererHelper.StarMoved star3 in stars)
                                    {
                                        if ((star3 == star) || (star3 == star2))
                                        { continue; }

                                        double p = Geometry.GetIntersectionMin(hitPoint, toStar2, star3.sphere);
                                        if (double.IsNaN(p) || double.IsInfinity(p))
                                        { continue; }

                                        if (p < shadow)
                                        { shadow = p; }
                                    }
                                    double d = toStar2Size - star2.radius;
                                    //if ((d > star2.radius / 10) && (d < shadow))
                                    if (d < shadow)
                                    { br += star2.exitance * dp / (d * d); }
                                }

                                #region failed attempt to count in nonzero radius
                                /*
                                 * failed attempt to count in nonzero radius
                                 *
                                Geometry.Point star2Closest = view.GetClosestPointTo(star2.center);
                                Geometry.Vector star2ToClosest = new Geometry.Vector(star2.center, star2Closest);
                                double u = Math.Min(star2ToClosest.Size, star2.radius);
                                Geometry.Point star2ViewClosest = new Geometry.Line(star2.center, star2ToClosest).GetPoint(u);
                                Geometry.Vector toStar2ViewClosest = new Geometry.Vector(hitPoint, star2ViewClosest);
                                toStar2ViewClosest.Normalize();
                                double dp2 = -Geometry.DotProduct(toStar2ViewClosest, normal);
                                if (dp2 > 0)
                                {
                                    double p1;
                                    double p2;
                                    Geometry.Line viewToClosest = new Geometry.Line(hitPoint, toStar2ViewClosest);
                                    int results = Geometry.GetIntersections(viewToClosest, star2.sphere, out p1, out p2);
                                    if (results > 0)
                                    {
                                        double p;
                                        if (p1 < 0)
                                        { p = p2; }
                                        else if (p2 < 0)
                                        { p = p1; }
                                        else
                                        { p = Math.Min(p1, p2); }

                                        if (p > 0)
                                        {
                                            br += star2.exitance;// / (p * p));
                                        }
                                    }
                                }
                                */
                                #endregion

                            }
                        }

                        #region NORMAL ILLUMINATION
                        /*
                        {
                            Geometry.Sphere star2;
                            double t2 = FindIntersection(stars, star, star.origin, normal, out star2);
                            if (star2 != null)
                            { br += star2.exitance / (t2 * t2); }
                        }
                        */
                        #endregion

                        #region REFLECTION
                        /*
                        {
                            var reflection = new Geometry.Vector(normal);
                            reflection.Multiply(2 * -Geometry.DotProduct(ray.vector, normal));
                            reflection.Add(ray.vector);

                            RendererHelper.StarMoved star2;
                            double t2 = FindIntersection(stars, star, hitPoint, reflection, out star2);
                            if (star2 != null)
                            { br += star2.exitance / (t2 * t2); }
                        }
                        */
                        #endregion

                        #region REFRACTION
                        /*
                        {
                            double c1 = -Geometry.DotProduct(ray.vector, normal);
                            double n = 1.3;
                            double c2 = Math.Sqrt(1 - n * n * (1 - c1 * c1));
                            var V = new Geometry.Vector(ray.vector);
                            V.Multiply(n);
                            var refraction = new Geometry.Vector(normal);
                            refraction.Multiply(n * c1 - c2);
                            refraction.Add(V);

                            Star star2;
                            double t2 = FindIntersection(stars, star, hitPoint, refraction, out star2);
                            if (star2 != null)
                            { br += star2.exitance / (t2 * t2); }
                        }
                        */
                        #endregion

                        dists[xx, yy].brightness = br;
                        brightness += br;
                    }
                }
            }

            flux[fluxIndex] = brightness / (zoom * zoom);
        }

        private static double FindIntersection(IList<RendererHelper.StarMoved> stars, RendererHelper.StarMoved skipStar, Geometry.Line ray, out RendererHelper.StarMoved outStar)
        {
            return FindIntersection(stars, skipStar, ray.origin, ray.vector, out outStar);
        }

        private static double FindIntersection(IList<RendererHelper.StarMoved> stars, RendererHelper.StarMoved skipStar, Geometry.Point origin, Geometry.Vector vector, out RendererHelper.StarMoved outStar)
        {
            outStar = null;
            double minP = double.PositiveInfinity;
            foreach (RendererHelper.StarMoved star in stars)
            {
                if (star == skipStar)
                { continue; }

                double p = Geometry.GetIntersectionMin(origin, vector, star.sphere);

                if (double.IsNaN(p) || double.IsInfinity(p))
                { continue; }

                if (p < minP)
                {
                    minP = p;
                    outStar = star;
                }
            }
            return minP;
        }
    }
}
