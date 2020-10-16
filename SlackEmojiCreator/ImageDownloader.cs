using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SlackEmojiCreator
{
    /// <summary>
    /// Downloader for image data.
    /// </summary>
    public static class ImageDownloader
    {
        /// <summary>
        /// Try download image as <see cref="BitmapImage"/>.
        /// </summary>
        /// <param name="uri">Image uri</param>
        /// <returns>Tuple which first param is whether download succeeded and second param is bitmap</returns>       
        public static async Task<(bool, BitmapImage)> TryDownloadBitmapImageAsync(Uri uri)
        {
            var (succeeded, imageBytes) = await TryDownloadImageBytes(uri);
            if (!succeeded)
            {
                Console.WriteLine($"Failed to get image bytes.");
                return (false, null);
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(imageBytes);
            bitmap.EndInit();
            return (true, bitmap);
        }

        private static async Task<(bool, byte[])> TryDownloadImageBytes(Uri uri)
        {
            using var client = new HttpClient();

            try
            {
                var msg = await client.GetAsync(uri).ConfigureAwait(false);
                var imageBytes = await msg.Content.ReadAsByteArrayAsync();
                return (true, imageBytes);
            }
            catch
            {
                return (false, null);
            }
        }
    }
}
