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
        public async Task<BitmapSource> Get(string url, int percent = 100)
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
                }
                else
                {

                    if(percent == 100)
                    {
                        data = await new WebClient().DownloadDataTaskAsync(url);
                        Dicc[url + "_" + percent.ToString()] = img = (RawToBitmapImage(data), data);
                    }
                    else if (percent == 0)
                    {
                        var bm = await GetGrayscale(url);
                        bm.Save(@".\\test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                        MemoryStream ms = new MemoryStream();
                        bm.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                        data = ms.GetBuffer();  //byte[]   bytes=   ms.ToArray(); 这两句都可以，至于区别么，下面有解释
                        ms.Close();
                        Dicc[url + "_" + percent.ToString()] = img = (BitmapUtils.Bitmap2BitmapSource(bm), data);
                    }
                    else
                    {
                        var bm = await GetpercentCDImg(url, percent);
                        bm.Save(@".\\test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
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
                }

            }
            Log.debug($"Time: {sw.Elapsed} {url} percent: {percent}%");
            return img.Normal;
        }

        public async Task<Bitmap> GetpercentCDImg(string url,int percent)
        {
            if (!Dicc.ContainsKey(url))
                await Get(url);
            var (n, d) = Dicc[url + "_100"];

            var fullimg = BitmapUtils.ToBitmap(d);
            var cdimg = BitmapUtils.Grayscale(BitmapUtils.ToBitmap(d));

            var width = fullimg.Width;
            var height = fullimg.Height;

            var cdheight = fullimg.Height * percent / 100;

            Bitmap res = new Bitmap(width, height);
            Graphics g1 = Graphics.FromImage(res);
            g1.FillRectangle(Brushes.White, new Rectangle(0, 0, width, height));
            g1.DrawImage(fullimg, new Rectangle(0, 0, width, cdheight), new Rectangle(0, 0, width, cdheight), GraphicsUnit.Pixel);
            g1.DrawImage(cdimg, new Rectangle(0, cdheight, width, height - cdheight), new Rectangle(0, cdheight, width, height - cdheight),GraphicsUnit.Pixel);
            return res;
        }

        public async Task<Bitmap> GetGrayscale(string url)
        {
            if (!Dicc.ContainsKey(url))
                await Get(url);

            var (n, d) = Dicc[url + "_100"];

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
