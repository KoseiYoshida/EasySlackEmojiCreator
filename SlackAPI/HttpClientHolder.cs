using System.Net.Http;

namespace SlackAPI.Net
{
    /// <summary>
    /// Hold a <see cref="HttpClient"/> to keep it only one.
    /// </summary>
    public static class HttpClientHolder
    {
        private readonly static HttpClient client;
        public static HttpClient Client => client;

        static HttpClientHolder()
        {
            client = new HttpClient();
        }
    }
}
