using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SlackAPI.Fetch
{
    /// <summary>
    /// Fetch emojis.
    /// </summary>
    public sealed class EmojiListFetcher
    {
        private const string UrlListBase = "https://{0}.slack.com/api/emoji.list";

        /// <summary>
        /// Default registered emoji names.
        /// </summary>
        private readonly static string[] SlackDefaultEmojiNames = new string[]
        {
                    "bowtie",
                    "squirrel",
                    "glitch_crab",
                    "piggy",
                    "cubimal_chick",
                    "dusty_stick",
                    "slack",
                    "pride",
                    "thumbsup_all",
                    "slack_call",
                    "shipit",
                    "white_square",
                    "black_square",
                    "simple_smile",
        };

        private readonly string workspace;
        private readonly string token;
        private readonly Uri uri;

        /// <summary>
        /// Constuctor.
        /// </summary>
        /// <param name="workspace">Name of slack workspace</param>
        /// <param name="token">Token for deleting</param>
        /// <exception cref="ArgumentException">Throw if <paramref name="workspace"/> is null or empty.</exception>
        /// <exception cref="ArgumentException">Throw if <paramref name="token"/> is null or empty.</exception>
        public EmojiListFetcher(string workspace, string token)
        {
            if (string.IsNullOrWhiteSpace(workspace))
            {
                throw new ArgumentException($"workspace is null or empty.");
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException($"token is null or empty.");
            }

            this.workspace = workspace;
            this.token = token;
            this.uri = new Uri(string.Format(UrlListBase, this.workspace) + "?token=" + this.token);
        }

        /// <summary>
        /// Get original emoji names in workspace;
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Emoji names</returns>
        /// <remarks>
        /// API details <seealso cref="https://api.slack.com/methods/emoji.list"/>
        /// </remarks>
        /// <exception cref="JsonReaderException">Throw if parsing response json is failed.</exception>
        public async Task<Dictionary<string, Uri>> GetUploadedEmojiInfoAsync()
        {
            var emojiDict = new Dictionary<string, Uri>();

            var request = WebRequest.CreateHttp(this.uri);
            request.Method = "GET";

            string jsonString;
            using var response = await request.GetResponseAsync();
            using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream);
            jsonString = await reader.ReadToEndAsync();

            try
            {
                JObject jsonObject = JObject.Parse(jsonString);
                if (!(bool)jsonObject["ok"])
                {
                    var errorMsg = jsonObject["error"].ToString();
                    Console.WriteLine($"Invalid request. reason : {errorMsg}, workspace:{workspace}, token:{token}");
                    throw new Exception($"{errorMsg}");
                }


                JObject emojis = (JObject)jsonObject["emoji"];
                foreach (var e in emojis)
                {
                    var name = e.Key;
                    if (SlackDefaultEmojiNames.Contains(name))
                    {
                        continue;
                    }

                    var uri = new Uri(e.Value.ToString());
                    emojiDict.Add(name, uri);
                }
                return emojiDict;
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"Failed to parse json. {ex.Message}");
                throw;
            }

        }
    }
}
