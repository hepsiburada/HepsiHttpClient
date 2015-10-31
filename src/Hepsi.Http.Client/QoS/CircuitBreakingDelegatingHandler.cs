using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace Hepsi.Http.Client.QoS
{
    public class CircuitBreakingDelegatingHandler : DelegatingHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger<CircuitBreakingDelegatingHandler>();

        private readonly int exceptionsAllowedBeforeBreaking;
        private readonly TimeSpan durationOfBreak;
        private readonly Policy circuitBreakerPolicy;

        public CircuitBreakingDelegatingHandler(int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
            this.durationOfBreak = durationOfBreak;

            circuitBreakerPolicy = Policy.Handle<HttpRequestException>().CircuitBreaker(exceptionsAllowedBeforeBreaking, durationOfBreak);
        }

        internal CircuitBreakingDelegatingHandler(int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak)
        {
            this.exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
            this.durationOfBreak = durationOfBreak;

            circuitBreakerPolicy = Policy.Handle<HttpRequestException>().CircuitBreaker(exceptionsAllowedBeforeBreaking, durationOfBreak);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Task<HttpResponseMessage> responseTask = null;

            try
            {
                return circuitBreakerPolicy.Execute(() =>
                {
                    responseTask = base.SendAsync(request, cancellationToken);

                    if (IsTransientFailure(responseTask.Result))
                    {
                        throw new HttpRequestException("transient failure occurred");
                    }

                    return responseTask;
                });
            }
            catch (BrokenCircuitException)
            {
                Logger.WarnFormat("Reached to allowed number of exceptions. Circuit is open. AllowedExceptionCount: {0}, DurationOfBreak: {1}", exceptionsAllowedBeforeBreaking, durationOfBreak);
                throw;
            }
            catch (HttpRequestException)
            {
                return responseTask;
            }
        }

        private static bool IsTransientFailure(HttpResponseMessage result)
        {
            return result.StatusCode >= HttpStatusCode.InternalServerError;
        }
    }
}
