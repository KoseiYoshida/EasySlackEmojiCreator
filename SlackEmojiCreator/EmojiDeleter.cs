using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SlackEmojiCreator
{
    public sealed class EmojiDeleter : IDisposable
    {
        private const string UrlRemoveBase = "https://{0}.slack.com/api/emoji.remove";

        private Uri GetEmojiRemoveUri()
        {
            if (string.IsNullOrWhiteSpace(this.workspace))
            {
                throw new Exception($"workspace is null or empty.");
            }

            if (string.IsNullOrEmpty(this.token))
            {
                throw new Exception($"Token is null or empty.");
            }

            // TODO: パラメータとしてのtokenが必要か確認する。（contentでもtokenをつけているので）
            return new Uri(string.Format(UrlRemoveBase, this.workspace) + "?token=" + this.token);
        }

        private HttpClient client = new HttpClient();

        private string workspace;
        private string token;

        public EmojiDeleter(string workspace, string token)
        {
            this.workspace = workspace;
            this.token = token;
        }

        public async Task DeleteAsync(string emojiName)
        {
            var uri = GetEmojiRemoveUri();

            HttpClient client = new HttpClient();
            MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new StringContent(token), "token");
            content.Add(new StringContent(emojiName), "name");


            Console.WriteLine($"Try deleting {emojiName}");
            HttpResponseMessage msg;
            try
            {
                msg = await client.PostAsync(uri, content);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error occurred during delting {emojiName}. {ex} URI:{uri}, Token:{token}");
                return;
            }

            var message = await msg.Content.ReadAsStringAsync();
            Console.WriteLine($"response : {message}");
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
