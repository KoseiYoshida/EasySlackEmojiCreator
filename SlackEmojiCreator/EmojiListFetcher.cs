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


        // TODO: 失敗理由を返す。
        /// <summary>
        /// Get original emoji names in workspace;
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Emoji names</returns>
        /// <remarks>
        /// API details <seealso cref="https://api.slack.com/methods/emoji.list"/>
        /// </remarks>
        public async Task<string[]> GetEmojiNamesAsync()
        {
            var emojiDict = new Dictionary<string, string>();

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
                Console.WriteLine($"Failed to get response json. {ex}");
                return Array.Empty<string>();
            }


            try
            {
                JObject jsonObject = JObject.Parse(jsonString);
                if (!(bool)jsonObject["ok"])
                {
                    var errorMsg = jsonObject["error"].ToString();
                    Console.WriteLine($"Invalid request. reason : {errorMsg}");
                    return Array.Empty<string>();
                }


                JObject emojis = (JObject)jsonObject["emoji"];
                foreach (var e in emojis)
                {
                    var name = e.Key;
                    var uri = e.Value.ToString();
                    emojiDict.Add(name, uri);
                }  
                return emojiDict.Keys.ToArray();
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"Failed to parse json. {ex}");
                return Array.Empty<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get emoji list. {ex}");
                return Array.Empty<string>();
            }

        }
    }
}
