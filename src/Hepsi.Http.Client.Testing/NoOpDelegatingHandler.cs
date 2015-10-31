using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Hepsi.Http.Client.Testing
{
    public class NoOpDelegatingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.OK), cancellationToken);
        }
    }
}
