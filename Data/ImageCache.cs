//using SpellTracker.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using SpellTracker.Control;
using System.Drawing;
using System.Drawing.Drawing2D;
using Point = System.Drawing.Point;


#nullable enable

namespace SpellTracker.Data
{
    public class ImageCache
    {
        public static ImageCache Instance { get; } = new ImageCache("cache");

        private static readonly bool IsDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());

        public bool LocalCache => Directory.Exists(CachePath);

        public string FullCachePath => Path.Combine(Path.GetFullPath("./"), CachePath);

        private readonly IDictionary<string, (BitmapSource Normal,  byte[] Raw)> Dicc = new ConcurrentDictionary<string, (BitmapSource, byte[])>();

        private readonly string CachePath;

        public ImageCache(string cachePath)
        {
            this.CachePath = cachePath;
        }

        /* 100 => not in cd */
        public async Task<BitmapSource> Get(string url, int percent = 360)
        {
            var sw = Stopwatch.StartNew();
           
            if (!Dicc.TryGetValue(url + "_" + percent.ToString(), out var img))
            {
                string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), CachePath, ToMD5(url + "_" + percent.ToString()));
                byte[] data;

                if (File.Exists(file))
                {
                    data = File.ReadAllBytes(file);
                    Dicc[url + "_" + percent.ToString()] = img = (RawToBitmapImage(data), data);
                    Log.Info($"Time: {sw.Elapsed} {url} percent: {percent}% Hit");
                }
                else
                {
                    if(percent == 360)
                    {
                        data = await new WebClient().DownloadDataTaskAsync(url);
                        Dicc[url + "_" + percent.ToString()] = img = (RawToBitmapImage(data), data);
                    }
                    //else if (percent == 0)
                    //{
                    //    var bm = await GetGrayscale(url);
                    //    MemoryStream ms = new MemoryStream();
                    //    bm.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    //    data = ms.GetBuffer();
                    //    ms.Close();
                    //    Dicc[url + "_" + percent.ToString()] = img = (BitmapUtils.Bitmap2BitmapSource(bm), data);
                    //}
                    else
                    {
                        var bm = await GetpercentCDImg(url, percent);
                        MemoryStream ms = new MemoryStream();
                        bm.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                        data = ms.GetBuffer();
                        ms.Close();
                        Dicc[url + "_" + percent.ToString()] = img = (BitmapUtils.Bitmap2BitmapSource(bm), data);
                    }

                    if (!IsDesignMode)
                    {
                        Directory.CreateDirectory(CachePath);
                        File.WriteAllBytes(file, data);
                    }
                    Log.Info($"Time: {sw.Elapsed} {url} percent: {percent}% Loaded");
                }

            }
            
            return img.Normal;
        }

        public async Task<Bitmap> GetpercentCDImg(string url,int percent)
        {
            if (!Dicc.ContainsKey(url))
                await Get(url);
            var (n, d) = Dicc[url + "_360"];

            var fullimg = BitmapUtils.SetBrightness(BitmapUtils.ToBitmap(d),0.5);
            var cdimg = BitmapUtils.Grayscale(BitmapUtils.ToBitmap(d));

            var width = fullimg.Width;
            var height = fullimg.Height;
            
            Point origin = new Point(width / 2, height / 2);
            Point midtop = new Point(width / 2, 0);
            Point lefttop = new Point(0, 0);
            Point righttop = new Point(width, 0);
            Point leftbottom = new Point(0, height);
            Point rightbottom = new Point(width, height);
            double theta = (percent * Math.PI) / 180 ;
            Point target = new Point(width / 2 + (int)(Math.Sin(theta) * width), height / 2 - (int)(Math.Cos(theta) * height));
            Pen pen = new Pen(Color.White,2);
            SolidBrush brush = new SolidBrush(Color.Black);
            var cdheight = fullimg.Height * percent / 100;

            Point[]? poly_gray = null;
            if(percent < 45 && percent >= 0) 
            {
                poly_gray = new Point[] { origin, midtop, lefttop, leftbottom, rightbottom, righttop, target};
            }
            else if ((percent < 135 && percent >= 45))
            {
                poly_gray = new Point[] { origin, midtop, lefttop, leftbottom, rightbottom, target };
            }
            else if ((percent < 225 && percent >= 135))
            {
                poly_gray = new Point[] { origin, midtop, lefttop, leftbottom, target };
            }
            else if ((percent < 315 && percent >= 225))
            {
                poly_gray = new Point[] { origin, midtop, lefttop, target };
            }
            else if (percent > 315 && percent <= 360)
            {
                poly_gray = new Point[] { origin, midtop, target };
            }
            Bitmap res = new Bitmap(width, height);
            Graphics g1 = Graphics.FromImage(res);
            g1.FillRectangle(Brushes.White, new Rectangle(0, 0, width, height));
            g1.DrawImage(fullimg, new Rectangle(0, 0, width, height), new Rectangle(0, 0, width, height), GraphicsUnit.Pixel);
            g1.FillPolygon(brush, poly_gray);
            //g1.DrawImage(fullimg, new Rectangle(0, 0, width, cdheight), new Rectangle(0, 0, width, cdheight), GraphicsUnit.Pixel);
            //g1.DrawImage(cdimg, new Rectangle(0, cdheight, width, height - cdheight), new Rectangle(0, cdheight, width, height - cdheight),GraphicsUnit.Pixel);
            g1.DrawLine(pen, origin, target);
            g1.DrawLine(pen, origin, midtop);
            res.Save("test.bmp");
            return res;
        }

        public async Task<Bitmap> GetGrayscale(string url)
        {
            if (!Dicc.ContainsKey(url))
                await Get(url);

            var (n, d) = Dicc[url + "_360"];

            var gray = BitmapUtils.Grayscale(BitmapUtils.ToBitmap(d));
            var g = BitmapUtils.Bitmap2BitmapSource(gray);
            Dicc[url] = (g, d);

            return gray;
        }

        private static BitmapSource RawToBitmapImage(byte[] data)
        {
            var stream = new MemoryStream(data);
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }

        private static string ToMD5(string txt)
        {
            using (var m = MD5.Create())
            {
                return BitConverter.ToString(m.ComputeHash(Encoding.UTF8.GetBytes(txt))).Replace("-", "");
            }
        }
    }
}
