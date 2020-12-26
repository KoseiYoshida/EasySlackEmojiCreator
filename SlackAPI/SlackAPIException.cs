using System;
using System.Runtime.Serialization;

namespace SlackAPI.Exception
{
    /// <summary>
    /// The exception thrown when an error occurs by using slack api.
    /// </summary>
    [Serializable()]
    public class SlackAPIException : System.Exception
    {
        public SlackAPIException() : base() { }

        public SlackAPIException(string message) : base(message) { }

        public SlackAPIException(string message, System.Exception innerException) : base(message, innerException) { }

        protected SlackAPIException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
