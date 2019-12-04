using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using RapidQA;

namespace RapidQA
{
    class ImageMerger
    {
        public static BitmapImage MergeImages(List<Asset> chosenImages)
        {
            // Convert images to Bitmaps      
            var bitmap = new Bitmap(1000, 1000);
            Graphics g = Graphics.FromImage(bitmap);          

            foreach (Asset image in chosenImages)
            {
                try
                {
                    int x = image.XPosition;
                    int y = image.YPosition;

                    Bitmap bmp = (Bitmap)Image.FromFile(image.Filepath);
                    var width = bmp.Width;
                    var height = bmp.Height;

                    g.DrawImage(bmp, x, y);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }

            return BitmapToBitmapImage(bitmap);
        }

        // Converts Bitmap to BitmapImage
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
}
