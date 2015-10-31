using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace Hepsi.Http.Client.QoS
{
    public class TimeoutingDelegatingHandler : DelegatingHandler
    {
        private static readonly ILog logger = LogManager.GetLogger<TimeoutingDelegatingHandler>();

        private readonly TimeSpan timeout;

        public TimeoutingDelegatingHandler(TimeSpan timeout, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.timeout = timeout;
        }

        internal TimeoutingDelegatingHandler(TimeSpan timeout)
        {
            this.timeout = timeout;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource(timeout);

                var responseTask = base.SendAsync(request, cancellationTokenSource.Token);

                responseTask.Wait();

                return responseTask;
            }
            catch (AggregateException aggregateException)
            {
                if (ContainsTaskCancelledException(aggregateException))
                {
                    logger.WarnFormat("Http request is timed out. Request URI: {0}, Timeout: {1} ms", request.RequestUri, timeout.TotalMilliseconds);
                    throw new HttpRequestTimeoutException("Http request is timed out", aggregateException);
                }

                throw;
            }
        }

        private static bool ContainsTaskCancelledException(AggregateException aex)
        {
            return aex.GetBaseException().GetType() == typeof(TaskCanceledException);
        }
    }
}
