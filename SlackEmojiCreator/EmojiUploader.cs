using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SlackEmojiCreator
{
    public sealed class EmojiUploader : IDisposable
    {
        private const string UrlAddBase = "https://{0}.slack.com/api/emoji.add";

        private readonly string workspace;
        private readonly string token;
        private readonly Uri uri;

        private HttpClient client;


        public EmojiUploader(string workspace, string token)
        {
            if (string.IsNullOrWhiteSpace(workspace))
            {
                throw new Exception($"Workspace is null or empty.");
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new Exception($"Token is null or empty.");
            }

            this.workspace = workspace;
            this.token = token;
            this.uri = new Uri(string.Format(UrlAddBase, this.workspace) + "?token=" + this.token);
            client = new HttpClient();
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

            var imageBytes = await File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            await UploadEmojiAsync(imageBytes, fileName);
        }

        public async Task UploadEmojiAsync(byte[] imageArray, string fileName)
        {

            // TODO: 不正な拡張子を防ぐ処理を書く。

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

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
