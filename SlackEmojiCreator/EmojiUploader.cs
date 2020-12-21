using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SlackEmojiCreator.Upload
{
    // TODO: IDisposable不要
    public sealed class EmojiUploader : IDisposable
    {
        private static readonly string UrlAddBase = "https://{0}.slack.com/api/emoji.add";
        private static readonly string[] AvailableExtensions = new string[4]
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".gif"
        };
        // Failure reason for upload request. See https://api.slack.com/methods/admin.emoji.add
        private static readonly Dictionary<string, FailureReason> failureReasonDict = new Dictionary<string, FailureReason>()
        {
            { "error_bad_name_i18n", FailureReason.NameIsInvalid },
            { "error_name_taken", FailureReason.NameIsAlreadyExist },
            { "error_name_taken_i18n", FailureReason.NameIsInvalid },
            { "error_missing_name", FailureReason.NameIsInvalid },
            { "error_no_image", FailureReason.ImageIsInvalid },
            { "error_bad_format", FailureReason.ImageIsInvalid },
            { "no_image_uploaded", FailureReason.ImageIsInvalid },
            { "error_too_big", FailureReason.ImageIsInvalid },
            { "error_bad_wide", FailureReason.ImageIsInvalid },
            { "too_many_frames", FailureReason.ImageIsInvalid },
            { "resized_but_still_too_large", FailureReason.ImageIsInvalid },
            { "ratelimited", FailureReason.RequestError },
            { "emoji_limit_reached", FailureReason.OverLimit },
            { "not_authed", FailureReason.AuthError },
            { "invalid_auth", FailureReason.AuthError },
            { "account_inactive", FailureReason.AuthError },
            { "token_revoked", FailureReason.AuthError },
            { "no_permission", FailureReason.AuthError },
            { "missing_scope", FailureReason.AuthError },
            { "not_allowed_token_type", FailureReason.AuthError },
            { "is_bot", FailureReason.UploadedFromBot },
            { "request_timeout", FailureReason.TimeOut },
        };

        private readonly string workspace;
        private readonly string token;
        private readonly Uri uri;

        // HttpClientは一つのインスタンスを使いまわす。
        private static HttpClient client = new HttpClient();


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="workspace">Workspace name</param>
        /// <param name="token">Token to add emoji</param>
        /// <exception cref="ArgumentException">Throw if <paramref name="workspace"/> is null or whitespace.</exception>
        /// <exception cref="ArgumentException">Throw if <paramref name="token"/> is null or whitespace.</exception>
        public EmojiUploader(string workspace, string token)
        {
            if (string.IsNullOrWhiteSpace(workspace))
            {
                throw new ArgumentException($"Workspace is null or whitespace. Please set.");
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException($"Token is null or whitespace. Please set.");
            }

            this.workspace = workspace;
            this.token = token;
            this.uri = new Uri(string.Format(UrlAddBase, this.workspace) + "?token=" + this.token);
        }

        /// <summary>
        /// Upload file as emoji.
        /// </summary>
        /// <param name="filePath">Path of image file</param>
        /// <returns><see cref="UploadResult"/></returns>        
        public async Task<UploadResult> UploadEmojiAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return new UploadResult(false, FailureReason.FilepathIsInvalid);
            }

            if (!File.Exists(filePath))
            {
                return new UploadResult(false, FailureReason.FileNotExist);
            }

            var fileExtension = Path.GetExtension(filePath);
            var isAvailableExtension = AvailableExtensions.Contains(fileExtension);
            if (!isAvailableExtension)
            {
                return new UploadResult(false, FailureReason.FileExtensionIsNotAvailable);
            }

            var imageBytes = await File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            return await UploadEmojiAsync(imageBytes, fileName);
        }

        /// <summary>
        /// Upload byte array as emoji.
        /// </summary>
        /// <param name="imageBytes">Byte array of image</param>
        /// <param name="fileName">Name of image</param>
        /// <returns><see cref="UploadResult"/></returns>
        public async Task<UploadResult> UploadEmojiAsync(byte[] imageBytes, string fileName)
        {
            if (string.IsNullOrWhiteSpace(uri.ToString()))
            {
                return new UploadResult(false, FailureReason.UriIsInvalid);
            }


            // 大文字のアルファベットは絵文字の名前で使えないので小文字にする。
            fileName = fileName.ToLowerInvariant();

            MultipartFormDataContent content = new MultipartFormDataContent();
            ByteArrayContent imageContent = new ByteArrayContent(imageBytes);
            content.Add(imageContent, "image", fileName);
            content.Add(new StringContent("data"), "mode");
            content.Add(new StringContent(token), "token");
            content.Add(new StringContent(fileName), "name");

            try
            {
                var msg = await client.PostAsync(uri, content);

                // HttpResponseMessage.Status will be "ok" even if  the post failed. Check the responseContent
                var responseContent = await msg.Content.ReadAsStringAsync();
                var isSucceeded = responseContent == "{\"ok\":true}";
                if (isSucceeded)
                {
                    Console.WriteLine($"Add {fileName} succeeded.");
                    return new UploadResult(true, FailureReason.NotFailed);
                }
                else
                {
                    Console.WriteLine($"Post failed. {responseContent}");

                    var failureResponse = JsonSerializer.Deserialize<FailureResponse>(responseContent);
                    if(failureReasonDict.TryGetValue(failureResponse.Error, out var reason))
                    {
                        return new UploadResult(false, reason);
                    }
                    else
                    {
                        return new UploadResult(false, FailureReason.NoDetail);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"error : {ex}");
                return new UploadResult(false, FailureReason.RequestError);
            }
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable"/>
        /// </summary>
        public void Dispose()
        {
            client.Dispose();
        }
    }
}
