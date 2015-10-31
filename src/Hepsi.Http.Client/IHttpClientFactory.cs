using System.Net.Http;

namespace Hepsi.Http.Client
{
    public interface IHttpClientFactory
    {
        HttpClient CreateHttpClient(string baseAddress);

        HttpClient CreateHttpClient();
    }
}
