using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Polly;

namespace Hepsi.Http.Client.QoS
{
    public class RetryingDelegatingHandler : DelegatingHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger<RetryingDelegatingHandler>();

        private readonly int retryCount;
        private readonly Policy retryPolicy;

        public RetryingDelegatingHandler(int retryCount, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.retryCount = retryCount;
            retryPolicy = Policy.Handle<HttpRequestException>().Or<HttpRequestTimeoutException>().Retry(retryCount);
        }

        public RetryingDelegatingHandler(int retryCount, Func<int, TimeSpan> sleepDurations, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.retryCount = retryCount;
            retryPolicy = Policy.Handle<HttpRequestException>().Or<HttpRequestTimeoutException>().WaitAndRetry(retryCount, sleepDurations);
        }

        internal RetryingDelegatingHandler(int retryCount)
        {
            this.retryCount = retryCount;
            retryPolicy = Policy.Handle<HttpRequestException>().Or<HttpRequestTimeoutException>().Retry(retryCount);
        }

        internal RetryingDelegatingHandler(int retryCount, Func<int, TimeSpan> sleepDurations)
        {
            this.retryCount = retryCount;
            retryPolicy = Policy.Handle<HttpRequestException>().Or<HttpRequestTimeoutException>().WaitAndRetry(retryCount, sleepDurations);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Task<HttpResponseMessage> responseTask = null;

            try
            {
                return retryPolicy.Execute(() =>
                {
                    responseTask = base.SendAsync(request, cancellationToken);

                    var httpResponseMessage = responseTask.Result;

                    if (IsTransientFailure(httpResponseMessage))
                    {
                        Logger.WarnFormat(
                            "Transient failure occured for request {0}. Status Code: {1}",
                            request.RequestUri,
                            httpResponseMessage.StatusCode);

                        throw new HttpRequestException("transient failure occurred");
                    }

                    return responseTask;
                });
            }
            catch (HttpRequestException) // happens only when retry policy reaches max # of retries.
            {
                Logger.WarnFormat("Reached to max number of retries for request {0}. RetryCount: {1}", request.RequestUri, retryCount);
                return responseTask;
            }
        }

        private static bool IsTransientFailure(HttpResponseMessage result)
        {
            return result.StatusCode >= HttpStatusCode.InternalServerError;
        }
    }
}
