using System;
using System.Runtime.Serialization;

namespace Hepsi.Http.Client.QoS
{
    [Serializable]
    public class HttpRequestTimeoutException : Exception
    {
        public HttpRequestTimeoutException() { }

        public HttpRequestTimeoutException(string message) : base(message) { }

        public HttpRequestTimeoutException(string message, Exception inner) : base(message, inner) { }

        protected HttpRequestTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
