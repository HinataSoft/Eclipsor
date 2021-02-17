using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Eclipsor
{
    public partial class MainForm : Form
    {
        readonly object distsLockObject = new object();

        Binary root;
        double[] flux;
        double[] mags;
        DistPoint[,] dists;
        int currentTime;
        int currentTimeFactor;
        IRenderer renderer;
        IRenderer[] renderers;

        int fpsCounter;
        DateTime fpsStart;

        public MainForm()
        {
            InitializeComponent();

            //double fact = 1;// trackBar1.Maximum / 2 / 1.209373;
            double fact = trackBar1.Maximum / 2 / 4;

            currentTimeFactor = 1;

            // CzeV343
            //Binary inner = new Binary(new Star(2, 1.4), new Star(2, 1.6));
            //inner.o1.radius = 4;
            //inner.o2.radius = 4;
            //inner.period = 1.209373 * fact;
            //inner.phase0 = 0.085;

            //Binary inner2 = new Binary(new Star(3, 1), new Star(3, 1));
            //inner2.o1.radius = 4;
            //inner2.o2.radius = 4;
            //inner2.period = 0.806931 * fact;
            //inner2.phase0 = 0;

            //root = new Binary(inner, inner2);
            //root.o1.radius = 4;
            //root.o2.radius = 4;
            //root.period = 605;
            //root.phase0 = 0.3;

            // https://arxiv.org/pdf/2101.03433.pdf
            // P. Zasche
            Binary inner = new Binary(new Star(1.49, 6400), new Star(0.52, 3923));
            inner.o1.radius = 2;
            inner.o2.radius = 2;
            inner.period = 1.57 * fact;
            inner.phase0 = 0.1;

            Binary inner2 = new Binary(new Star(1.62, 6365), new Star(0.62, 4290));
            inner2.o1.radius = 4;
            inner2.o2.radius = 4;
            inner2.period = 1.306 * fact;
            inner2.phase0 = 0.8;

            Binary inner3 = new Binary(new Star(1.45, 6350), new Star(0.56, 3990));
            inner3.o1.radius = 4;
            inner3.o2.radius = 4;
            inner3.period = 8.217 * fact;
            inner3.phase0 = 0.3;

            Binary inner1_2 = new Binary(inner, inner2);
            inner1_2.o1.radius = 16;
            inner1_2.o2.radius = 16;
            inner1_2.period = 3.7 * 365 * fact;
            inner1_2.phase0 = 0.25;

            root = new Binary(inner1_2, inner3);
            root.o1.radius = 32;
            root.o2.radius = 32;
            root.period = 2000 * 365 * fact;
            root.phase0 = 0.5;

            //root = new Binary(inner, inner2);
            //root = new Binary(inner, new Star(25, 50));
            //root = new Binary(new Star(4, 10), new Star(0.5, 11));
            //root = new Binary(new Star(5, 10000), new Star(5, 1));
            //root.o1.radius = 6;
            //root.o2.radius = 6;
            //root.period = 605;
            //root.phase0 = 0.3;

            root.PlaceInTime(0);

            trackBar2.Value = 0;
            angleLabel.Text = trackBar2.Value.ToString() + "°";

            flux = new double[trackBar1.Maximum * currentTimeFactor + 1];
            for (int i = 0; i < flux.Length; i++)
            { flux[i] = double.NaN; }
            mags = new double[flux.Length];

            dists = new DistPoint[pictureBox2.Width, pictureBox2.Height];

            renderers = new IRenderer[] { new SphereTracer(), new SimpleRenderer() };
            //renderers = new IRenderer[] { new SimpleRenderer(), new SphereTracer() };
            //renderers = new IRenderer[] { new SphereTracer(), new SphereTracerOld(), new SimpleRenderer() };
            foreach (IRenderer r in renderers)
            { rendererComboBox.Items.Add(r.GetType().Name); }
            renderer = renderers[0];
            rendererComboBox.SelectedIndex = 0;
        }

        private DistPoint[,] LockDists()
        {
            DistPoint[,] result;
            lock (distsLockObject)
            {
                result = dists;
            }
            return result;
        }
        private void SetDists(DistPoint[,] tmpDists)
        {
            lock (distsLockObject)
            {
                dists = tmpDists;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            angleLabel.Text = trackBar2.Value.ToString() + "°";
            currentTime = trackBar1.Value;
            root.PlaceInTime(currentTime);
            List<RendererHelper.StarMoved> stars = RendererHelper.GetAngledStars(root, currentTime, Extensions.DegToRad(trackBar2.Value));

            DistPoint[,] tmpDists = LockDists();
            renderer.Render(stars, tmpDists, flux, currentTime * currentTimeFactor);
            SetDists(tmpDists);

            pictureBox1.Invalidate();
            pictureBox2.Invalidate();
            pictureBox3.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), pictureBox1.Size));

            // +++ HACK
            //Bitmap bmp = MakeRenderBitmap();
            //g.DrawImage(bmp, (pictureBox1.Width - bmp.Width) / 2, 0);

            //double minFlux = double.PositiveInfinity;
            //double maxFlux = double.NegativeInfinity;
            //for (int i = 0; i < flux.Length; i++)
            //{
            //    if (flux[i] < minFlux)
            //    { minFlux = flux[i]; }
            //    if (flux[i] > maxFlux)
            //    { maxFlux = flux[i]; }
            //}

            //g.DrawString("© Václav Přibík", DefaultFont, Brushes.DarkOrange, new PointF(510, 480));

            //if (double.IsInfinity(minFlux) || (minFlux >= maxFlux))
            //{ return; }

            //// CzeV343
            ////minFlux = 0.0000000000000081410896090826384;
            ////maxFlux = 0.00000000000001076468493898098;
            //// Zasche
            //minFlux = 1.986293394705849;
            //maxFlux = 2.1336499894654914;

            //int maxWidth = pictureBox1.Width;
            //int maxHeight = 200;
            //int yOffs = bmp.Height;

            //for (int i = 0; i < flux.Length; i++)
            //{ mags[i] = -2.5 * Math.Log10(flux[i] / maxFlux); }

            //double minMag = -2.5 * Math.Log10(minFlux / maxFlux);
            //int logStep = (int)Math.Ceiling(Math.Log10(minMag)) - 1;
            //double magStepFactor = Math.Pow(10, logStep);

            //int minX = int.MinValue;
            //for (int i = 0; i < flux.Length; i++)
            //{
            //    int x = i * maxWidth / flux.Length;
            //    double mag = mags[i];
            //    if ((flux[i] > int.MinValue) && !double.IsNaN(mag) && !double.IsInfinity(mag))
            //    {
            //        int y = yOffs + (int)(mag * (maxHeight - 20) / minMag + 10);
            //        g.FillRectangle(Brushes.Yellow, x, y, 2, 2);
            //        if ((minX == int.MinValue) && (mags[i] == minMag))
            //        { minX = x; }
            //    }
            //}

            //return;
            // --- HACK

            double w, h;
            RendererHelper.GetBoundingBoxTop(root, out w, out h);
            float max = (float)Math.Max(w, h);

            float zoom = pictureBox1.Width * 1f / max;

            PaintPoint(g, zoom, pictureBox1.Width / 2, root);
        }

        void PaintPoint(Graphics g, float zoom, float offs, IPointObject p)
        {
            if (p is Binary)
            { PaintBinary(g, zoom, offs, (Binary)p); }
            if (p is Star)
            { PaintStar(g, zoom, offs, (Star)p); }
        }

        void PaintBinary(Graphics g, float zoom, float offs, Binary p)
        {
            float cx = (float)(p.x * zoom + offs);
            float cy = (float)(p.y * zoom + offs);
            g.DrawLine(Pens.Red, cx, cy - 10, cx, cy + 10);
            g.DrawLine(Pens.Red, cx - 10, cy, cx + 10, cy);

            PaintPoint(g, zoom, offs, p.o1.point);
            PaintPoint(g, zoom, offs, p.o2.point);
        }

        void PaintStar(Graphics g, float zoom, float offs, Star p)
        {
            float cx = (float)(p.center.x * zoom + offs);
            float cy = (float)(p.center.y * zoom + offs);
            float r = (float)(p.radius * zoom);

            g.DrawEllipse(Pens.White, cx - r, cy - r, 2 * r, 2 * r);
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), pictureBox2.Size));

            Bitmap bmp = MakeRenderBitmap();

            g.DrawImage(bmp, 0, 0);
        }

        private Bitmap MakeRenderBitmap()
        {
            Color[] palette = new Color[256];
            for (int i = 0; i < palette.Length; i++)
            { palette[i] = Color.FromArgb(i, i, i); }

            double max = double.NegativeInfinity;
            foreach (var star in RendererHelper.GetStars(root))
            {
                if (star.exitance > max)
                { max = star.exitance; }
            }


            DistPoint[,] tmpDists = LockDists();

            max /= 9;
            Bitmap bmp = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            for (int yy = 0; yy < pictureBox2.Height; yy++)
            {
                for (int xx = 0; xx < pictureBox2.Width; xx++)
                {
                    double br = tmpDists[xx, yy].brightness / max;
                    int color = (int)(Math.Log10(1 + br) * 250);
                    if (color > 255)
                    { color = 255; }
                    bmp.SetPixel(xx, yy, palette[color]);
                }
            }
            return bmp;
        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.FillRectangle(Brushes.White, new Rectangle(new Point(0, 0), pictureBox3.Size));
            {
                int x = currentTime * pictureBox3.Width / flux.Length;
                g.DrawLine(Pens.Red, x, 0, x, pictureBox3.Height);
            }

            double minFlux = double.PositiveInfinity;
            double maxFlux = double.NegativeInfinity;
            for (int i = 0; i < flux.Length; i++)
            {
                if (flux[i] < minFlux)
                { minFlux = flux[i]; }
                if (flux[i] > maxFlux)
                { maxFlux = flux[i]; }
            }

            if (double.IsInfinity(minFlux) || (minFlux >= maxFlux))
            { return; }

            for (int i = 0; i < flux.Length; i++)
            { mags[i] = -2.5 * Math.Log10(flux[i] / maxFlux); }

            double minMag = -2.5 * Math.Log10(minFlux / maxFlux);
            int logStep = (int)Math.Ceiling(Math.Log10(minMag)) - 1;
            double magStepFactor = Math.Pow(10, logStep);

            for (int step = 0; step < 10; step++)
            {
                double mag = step * magStepFactor;
                int y = (int)(mag * (pictureBox3.Height - 20) / minMag + 10);
                g.DrawLine(Pens.LightBlue, 10, y, pictureBox3.Width - 10, y);
                g.DrawString(mag.ToString(), this.Font, Brushes.Black, 20, y);
            }

            int minX = int.MinValue;
            for (int i = 0; i < flux.Length; i++)
            {
                int x = i * pictureBox3.Width / flux.Length;
                double mag = mags[i];
                if ((flux[i] > int.MinValue) && !double.IsNaN(mag) && !double.IsInfinity(mag))
                {
                    int y = (int)(mag * (pictureBox3.Height - 20) / minMag + 10);
                    g.FillRectangle(Brushes.Blue, x, y, 2, 2);
                    if ((minX == int.MinValue) && (mags[i] == minMag))
                    { minX = x; }
                }
            }

            if (minX > int.MinValue)
            { g.DrawString(minMag.ToString(), this.Font, Brushes.Black, minX, pictureBox3.Height - 20); }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // +++ HACK
            //Bitmap bmp = new Bitmap(600, 500);
            //Graphics g = Graphics.FromImage(bmp);
            //Rectangle r = new Rectangle(0, 0, 600, 500);
            //PaintEventArgs args = new PaintEventArgs(g, r);
            //double angle = Extensions.DegToRad(trackBar2.Value);
            //for (int i = 0; i < trackBar1.Maximum * currentTimeFactor; i++) // trackBar1.Maximum
            //{
            //    root.PlaceInTime((double)i / currentTimeFactor);
            //    renderer.Render(root, angle, dists, flux, i);
            //    pictureBox1_Paint(this, args);
            //    bmp.Save(@"D:\Work\Astro\Eclipsor\Zasche\frame" + i.ToString("D4") + ".png");
            //}

            //return;
            // --- HACK
            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = false;
            rendererComboBox.Enabled = false;
            backgroundWorker1.RunWorkerAsync(Extensions.DegToRad(trackBar2.Value));
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            fpsCounter = 0;
            fpsStart = DateTime.Now;
            double angle = (double)e.Argument;
            int counter = 0;
            int pctCounterValue = (trackBar1.Maximum * currentTimeFactor) / 100;
            if (pctCounterValue == 0)
            { pctCounterValue = 1; }
            //for (int ii = 0; ii < trackBar1.Maximum * currentTimeFactor; ii++)
            System.Threading.Tasks.Parallel.For(0, trackBar1.Maximum * currentTimeFactor, (ii) =>
            {
                int i = System.Threading.Interlocked.Increment(ref counter);
                if (backgroundWorker1.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                double currentTime = (double)i / currentTimeFactor;
                root.PlaceInTime(currentTime);
                var tmpDists = new DistPoint[pictureBox2.Width, pictureBox2.Height];
                List<RendererHelper.StarMoved> stars = RendererHelper.GetAngledStars(root, currentTime, angle);
                renderer.Render(stars, tmpDists, flux, i);
                SetDists(tmpDists);
                System.Threading.Interlocked.Increment(ref fpsCounter);
                if (i % pctCounterValue == 0)
                {
                    backgroundWorker1.ReportProgress(i * 100 / trackBar1.Maximum, i);
                }
            });
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            currentTime = (Int32)(e.UserState);
            pictureBox1.Invalidate();
            pictureBox2.Invalidate();
            pictureBox3.Invalidate();
            showFPS();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pictureBox1.Invalidate();
            pictureBox2.Invalidate();
            pictureBox3.Invalidate();
            button2.Enabled = false;
            button1.Enabled = true;
            button3.Enabled = true;
            rendererComboBox.Enabled = true;
            showFPS();
        }

        private void showFPS()
        {
            int tmpFpsCounter = fpsCounter;
            fpsLabel.Text = "FPS: " + 
                ((tmpFpsCounter > 0) ? (tmpFpsCounter * 1000.0 / DateTime.Now.Subtract(fpsStart).TotalMilliseconds).ToString("F2") : "");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < flux.Length; i++)
            { flux[i] = double.NaN; }
            pictureBox1.Invalidate();
            pictureBox2.Invalidate();
            pictureBox3.Invalidate();
        }

        private void rendererComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            renderer = renderers[rendererComboBox.SelectedIndex];
            List<RendererHelper.StarMoved> stars = RendererHelper.GetAngledStars(root, currentTime, Extensions.DegToRad(trackBar2.Value));
            DistPoint[,] tmpDists = LockDists();
            renderer.Render(stars, tmpDists, flux, currentTime * currentTimeFactor);
            SetDists(tmpDists);
            pictureBox2.Invalidate();
        }
    }
}
