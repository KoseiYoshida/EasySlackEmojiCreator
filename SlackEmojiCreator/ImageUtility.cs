using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SlackEmojiCreator.Utility
{
    public static class ImageUtility
    {
        public static BitmapSource CaptureFrameworkElement(FrameworkElement targetControl)
        {
            System.Windows.Point leftTopCorner = targetControl.PointToScreen(new System.Windows.Point(0f, 0f));
            var width = targetControl.ActualWidth;
            var height = targetControl.ActualHeight;
            Rect targetRect = new Rect(leftTopCorner.X, leftTopCorner.Y, width, height);
            BitmapSource bitmapSource;
            using (var screenBitmap = new Bitmap((int)targetRect.Width, (int)targetRect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bitmapGraphics = Graphics.FromImage(screenBitmap))
                {
                    bitmapGraphics.CopyFromScreen((int)targetRect.X, (int)targetRect.Y, 0, 0, screenBitmap.Size);
                    bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(screenBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }

            return bitmapSource;
        }

        public static byte[] GetByteArray(BitmapSource bitmapSource)
        {
            byte[] imageArray;
            var encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                // TODO: 非同期にする
                imageArray = ms.ToArray();
                //await ms.ReadAsync(imageArray, 0, (int)ms.Length);
            }

            return imageArray;
        }
    }
}
