using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SlackEmojiCreator
{
    public sealed class LocalFileEmojiUploader
    {
        private const string UrlAddBase = "https://{0}.slack.com/api/emoji.add";

        private Uri GetEmojiAddUri()
        {
            if (string.IsNullOrWhiteSpace(this.workspace))
            {
                throw new Exception($"workspace is null or empty.");
            }

            if (string.IsNullOrEmpty(this.token))
            {
                throw new Exception($"Token is null or empty.");
            }

            return new Uri(string.Format(UrlAddBase, this.workspace) + "?token=" + this.token);
        }

        private HttpClient client = new HttpClient();

        private string workspace;
        private string token;

        public LocalFileEmojiUploader(string workspace, string token)
        {
            this.workspace = workspace;
            this.token = token;           
        }

        /// <summary>
        /// Upload file as emoji.
        /// </summary>
        /// <param name="filePath">Path of image file</param>
        /// <returns></returns>
        public async Task UploadEmojiAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            // TODO: 不正な拡張子を防ぐ処理を書く。

            var uri = GetEmojiAddUri();

            MultipartFormDataContent content = new MultipartFormDataContent();
            ByteArrayContent imageBytes = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            content.Add(imageBytes, "image", fileName);
            content.Add(new StringContent("data"), "mode");
            content.Add(new StringContent(token), "token");
            content.Add(new StringContent(fileName), "name");

            try
            {                
                var msg = await client.PostAsync(uri, content);
                // HttpResponseMessage.StatusはPostが失敗してもOKになってしまう。なので、Contentを確認する。
                var responseContent = await msg.Content.ReadAsStringAsync();
                var isSucceeded = responseContent == "{\"ok\":true}";
                if (isSucceeded)
                {
                    Console.WriteLine($"Add {filePath} succeeded.");
                }
                else
                {
                    Console.WriteLine($"Post failed. {responseContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error : {ex}");
            }
        }


        // TODO: 
        public async Task UploadEmojiAsync(byte[] imageArray, string fileName)
        {           

            // TODO: 不正な拡張子を防ぐ処理を書く。

            var uri = GetEmojiAddUri();

            // 大文字のアルファベットは絵文字の名前で使えないので小文字にする。
            fileName = fileName.ToLowerInvariant();

            MultipartFormDataContent content = new MultipartFormDataContent();
            ByteArrayContent imageBytes = new ByteArrayContent(imageArray);
            content.Add(imageBytes, "image", fileName);
            content.Add(new StringContent("data"), "mode");
            content.Add(new StringContent(token), "token");
            content.Add(new StringContent(fileName), "name");

            try
            {
                var msg = await client.PostAsync(uri, content);
                // HttpResponseMessage.StatusはPostが失敗してもOKになってしまう。なので、Contentを確認する。
                var responseContent = await msg.Content.ReadAsStringAsync();
                var isSucceeded = responseContent == "{\"ok\":true}";
                if (isSucceeded)
                {
                    Console.WriteLine($"Add {fileName} succeeded.");
                }
                else
                {
                    Console.WriteLine($"Post failed. {responseContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error : {ex}");
            }
        }
    }
}
