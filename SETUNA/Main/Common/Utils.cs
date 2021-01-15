﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SETUNA.Main
{
    class URLUtils
    {
        public const string OriginURL = "http://www.clearunit.com/clearup/setuna2/";

        public const string NewURL = "https://www.github.com/tylearymf/setuna2/";
    }


    static class BitmapUtils
    {
        public static Bitmap ScaleToSize(this Bitmap bitmap, int width, int height)
        {
            if (bitmap.Width == width && bitmap.Height == height)
            {
                return bitmap;
            }

            var scaledBitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(scaledBitmap))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(bitmap, 0, 0, width, height);
            }

            return scaledBitmap;
        }

        public static Bitmap FromPath(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        var bitmap = new Bitmap(fs);
                        bitmap.MakeTransparent();
                        return bitmap;
                    }
                }
                catch (ArgumentException ex)
                {
                    try
                    {
                        using (var webp = new WebPWrapper.WebP())
                        {
                            var bitmap = webp.Load(path);
                            bitmap.MakeTransparent();
                            return bitmap;
                        }
                    }
                    catch { }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }

            return null;
        }

        public static void DownloadImage(string url, Action<Bitmap> finished)
        {
            var filePath = Path.Combine(Cache.CacheManager.Path, string.Format("TEMP_{0}_{1}.png", DateTime.Now.Ticks, Math.Abs(url.GetHashCode())));
            var client = new WebClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            client.DownloadFileCompleted += (s, e) =>
            {
                Bitmap bitmap = null;

                if (e.Error == null)
                {
                    try
                    {
                        bitmap = BitmapUtils.FromPath(filePath);
                    }
                    catch { }
                }

                try
                {
                    File.Delete(filePath);
                }
                catch { }

                finished?.Invoke(bitmap);
            };
            client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.141 Safari/537.36";
            client.DownloadFileAsync(new Uri(url), filePath);
        }
    }

    static class NetUtils
    {
        public static void Init()
        {
            ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
        }

        static bool RemoteCertificateValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }
    }
}

