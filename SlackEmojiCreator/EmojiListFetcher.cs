using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SlackEmojiCreator
{
    public sealed class EmojiListFetcher
    {
        private static string[] SlackDefaultEmojiNames = new string[]
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

        private const string UrlListBase = "https://{0}.slack.com/api/emoji.list";
        private Uri GetEmojiListUri()
        {
            if (string.IsNullOrWhiteSpace(this.workspace))
            {
                throw new Exception($"workspace is null or empty.");
            }

            if (string.IsNullOrEmpty(this.token))
            {
                throw new Exception($"Token is null or empty.");
            }

            return new Uri(string.Format(UrlListBase, this.workspace) + "?token=" + this.token);
        }

        private string workspace;
        private string token;

        public EmojiListFetcher(string workspace, string token)
        {
            this.workspace = workspace;
            this.token = token;
        }


        /// <summary>
        /// Get original emoji names in workspace;
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Emoji names</returns>
        /// <remarks>
        /// API details <seealso cref="https://api.slack.com/methods/emoji.list"/>
        /// </remarks>
        public async Task<Dictionary<string, Uri>> GetUploadedEmojiInfoAsync()
        {
            var emojiDict = new Dictionary<string, Uri>();

            var emojiListUri = GetEmojiListUri();
            var request = WebRequest.CreateHttp(emojiListUri);
            request.Method = "GET";

            string jsonString;
            try
            {
                using var response = await request.GetResponseAsync();
                using var stream = response.GetResponseStream();
                using var reader = new StreamReader(stream);
                jsonString = await reader.ReadToEndAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to get response json. {ex} workspace:{workspace}, token:{token}");
                throw ex;
            }

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
                Console.WriteLine($"Failed to parse json. {ex}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get emoji list. {ex}");
                throw;
            }

        }
    }
}
