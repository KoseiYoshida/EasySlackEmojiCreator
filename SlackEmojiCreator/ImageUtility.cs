using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SlackEmojiCreator.Utility
{
    public static class ImageUtility
    {
        /// <summary>
        /// Capture <see cref="FrameworkElement"/> from screen.
        /// </summary>
        /// <param name="targetFrameworkElement">Capture target</param>
        /// <returns><see cref="BitmapSource"/> of captured <see cref="FrameworkElement"/>.</returns>
        /// <exception cref="ArgumentNullException">Throw if <paramref name="targetFrameworkElement"/> is null.</exception>
        /// <exception cref="ArgumentException">Throw if creating <see cref="Bitmap"/> failed.</exception>
        /// <exception cref="Win32Exception">Throw if capturing screen was failed.</exception>
        public static BitmapSource CaptureFrameworkElement(FrameworkElement targetFrameworkElement)
        {
            if(targetFrameworkElement is null)
            {
                throw new ArgumentNullException(nameof(targetFrameworkElement));
            }

            System.Windows.Point leftTopCorner = targetFrameworkElement.PointToScreen(new System.Windows.Point(0f, 0f));
            var width = targetFrameworkElement.ActualWidth;
            var height = targetFrameworkElement.ActualHeight;
            Rect targetRect = new Rect(leftTopCorner.X, leftTopCorner.Y, width, height);

            Bitmap screenBitmap;
            try
            {
                screenBitmap = new Bitmap((int)targetRect.Width, (int)targetRect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Failed to create new bitmap. Error Message:{ex.Message}, Framework Element:{targetFrameworkElement.Name}, Width:{(int)targetRect.Width}, Height:{(int)targetRect.Height}");
                throw;
            }

            try
            {
                using var bitmapGraphics = Graphics.FromImage(screenBitmap);
                bitmapGraphics.CopyFromScreen((int)targetRect.X, (int)targetRect.Y, 0, 0, screenBitmap.Size);
                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(screenBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                return bitmapSource;
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine($"Failed to {nameof(Graphics.CopyFromScreen)}. Error Message:{ex.Message}, Framework Element:{targetFrameworkElement.Name}");
                throw;
            }
            finally
            {
                screenBitmap?.Dispose();
            }

        }

        /// <summary>
        /// Get byte array from <see cref="BitmapSource"/>.
        /// </summary>
        /// <param name="bitmapSource">Bitmap source.</param>
        /// <returns>Byte array of <see cref="BitmapSource"/></returns>
        /// <exception cref="ArgumentNullException">Throw if <paramref name="bitmapSource"/> is null.</exception>
        public static byte[] GetByteArray(BitmapSource bitmapSource)
        {
            if(bitmapSource is null)
            {
                throw new ArgumentNullException(nameof(bitmapSource));
            }

            byte[] imageArray;
            var encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using (var ms = new MemoryStream())
            {                
                encoder.Save(ms);
                imageArray = ms.ToArray();
            }

            return imageArray;
        }
    }
}
