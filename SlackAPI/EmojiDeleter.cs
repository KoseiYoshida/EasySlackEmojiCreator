using SlackAPI.Exception;
using SlackAPI.Net;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SlackAPI.Delete
{
    // TODO: IDisposableの使い方修正
    public sealed class EmojiDeleter
    {
        private const string UrlRemoveBase = "https://{0}.slack.com/api/emoji.remove";

        private readonly string workspace;
        private readonly string token;
        private readonly Uri uri;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="workspace">Name of slack workspace</param>
        /// <param name="token">Token for deleting</param>
        /// <exception cref="ArgumentException">Throw if <paramref name="workspace"/> is null or empty.</exception>
        /// <exception cref="ArgumentException">Throw if <paramref name="token"/> is null or empty.</exception>
        public EmojiDeleter(string workspace, string token)
        {
            if (string.IsNullOrWhiteSpace(workspace))
            {
                throw new ArgumentException($"workspace is null or empty.");
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException($"Token is null or empty.");
            }

            this.workspace = workspace;
            this.token = token;

            // TODO: パラメータとしてのtokenが必要か確認する。（contentでもtokenをつけているので）
            this.uri = new Uri(string.Format(UrlRemoveBase, this.workspace) + "?token=" + this.token);
        }

        /// <summary>
        /// Delete emoji.
        /// </summary>
        /// <param name="emojiName">Name of delete target</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Throw if <paramref name="emojiName"/> is null or empty.</exception>
        /// <exception cref="HttpRequestException">Throw if http request error occurred.</exception>
        /// <exception cref="SlackAPIException">Throw if some kind of error occurred when using Slack API.</exception>
        public async Task DeleteAsync(string emojiName)
        {
            if (string.IsNullOrEmpty(emojiName))
            {
                throw new ArgumentException($"{nameof(emojiName)} is null or empty.");
            }

            MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new StringContent(token), "token");
            content.Add(new StringContent(emojiName), "name");

            Console.WriteLine($"Try deleting {emojiName}");

            try
            {
                using HttpResponseMessage msg = await HttpClientHolder.Client.PostAsync(this.uri, content);
                var message = await msg.Content.ReadAsStringAsync();
                Console.WriteLine($"response : {message}");
            }
            catch(HttpRequestException ex)
            {
                Console.WriteLine($"Request error occurred. {ex.Message}, Emoji:{emojiName},  URI:{uri}, Token:{token}");
                throw;                
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"An error occurred. {ex.Message}, Emoji:{emojiName},  URI:{uri}, Token:{token}");
                throw new SlackAPIException($"An error occurred when deleting {emojiName};", ex);
            }
        }
    }
}
