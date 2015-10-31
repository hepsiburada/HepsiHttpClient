using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Hepsi.Http.Client.Testing
{
    public class RecordingDelegatingHandler : DelegatingHandler
    {
        private readonly HttpMessageRecorder recorder;

        public RecordingDelegatingHandler(HttpMessageRecorder recorder, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.recorder = recorder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            recorder.Requests.Add(request);

            var responseTask = base.SendAsync(request, cancellationToken);

            recorder.Responses.Add(responseTask.Result);

            return responseTask;
        }
    }
}
