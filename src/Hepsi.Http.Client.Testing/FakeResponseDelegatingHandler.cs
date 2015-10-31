using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Hepsi.Http.Client.Testing
{
    public class FakeResponseDelegatingHandler : DelegatingHandler
    {
        private readonly Dictionary<Uri, Queue<Func<HttpResponseMessage>>> fakeResponses = new Dictionary<Uri, Queue<Func<HttpResponseMessage>>>();

        public void AddFakeResponse(Uri uri, Func<HttpResponseMessage> response)
        {
            if (!fakeResponses.ContainsKey(uri))
            {
                fakeResponses[uri] = new Queue<Func<HttpResponseMessage>>();
            }

            fakeResponses[uri].Enqueue(response);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!fakeResponses.ContainsKey(request.RequestUri) || fakeResponses[request.RequestUri].Count == 0)
            {
                var notFoundResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };

                return Task.FromResult(notFoundResponseMessage);
            }

            var responseMessage = fakeResponses[request.RequestUri].Dequeue()();

            return Task.FromResult(responseMessage);
        }
    }
}
