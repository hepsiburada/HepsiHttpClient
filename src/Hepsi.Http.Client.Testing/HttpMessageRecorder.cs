using System.Collections.Generic;
using System.Net.Http;

namespace Hepsi.Http.Client.Testing
{
    public class HttpMessageRecorder
    {
        /// <summary>
        /// Returns the recorded HttpRequestMessages
        /// </summary>
        public List<HttpRequestMessage> Requests { get; private set; }

        /// <summary>
        /// Returns the recorded HttpResponseMessages
        /// </summary>
        public List<HttpResponseMessage> Responses { get; private set; }

        public HttpMessageRecorder()
        {
            Requests = new List<HttpRequestMessage>();
            Responses = new List<HttpResponseMessage>();
        }
    }
}
