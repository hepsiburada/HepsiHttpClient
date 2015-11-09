using System;
using System.Net.Http;

namespace Hepsi.Http.Client
{
    public class ConfigurableHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClientBuilder builder;

        public ConfigurableHttpClientFactory(HttpClientBuilder builder)
        {
            this.builder = builder;
        }

        public HttpClient CreateHttpClient(string baseAddress)
        {
            var httpClient = builder.Build();
            httpClient.BaseAddress = new Uri(baseAddress);

            return httpClient;
        }

        public HttpClient CreateHttpClient()
        {
            return builder.Build();
        }
    }
}
