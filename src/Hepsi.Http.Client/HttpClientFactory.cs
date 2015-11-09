using System;
using System.Net.Http;

namespace Hepsi.Http.Client
{
    public class HttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateHttpClient(string baseAddress)
        {
            return new HttpClient { BaseAddress = new Uri(baseAddress) };
        }

        public HttpClient CreateHttpClient()
        {
            return new HttpClient();
        }
    }
}
