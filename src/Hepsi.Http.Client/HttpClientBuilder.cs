using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Common.Logging;
using Hepsi.Http.Client.Correlation;
using Hepsi.Http.Client.Logging;
using Hepsi.Http.Client.QoS;

namespace Hepsi.Http.Client
{
    public class HttpClientBuilder
    {
        private readonly Dictionary<int, Func<DelegatingHandler>> handlers = new Dictionary<int, Func<DelegatingHandler>>();

        public HttpClientBuilder WithCircuitBreaker(int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak)
        {
            handlers.Add(5000, () => new CircuitBreakingDelegatingHandler(exceptionsAllowedBeforeBreaking, durationOfBreak));
            return this;
        }

        public HttpClientBuilder WithRetry(int retryCount)
        {
            handlers.Add(4000, () => new RetryingDelegatingHandler(retryCount));
            return this;
        }
        public HttpClientBuilder WithWaitAndRetry(int retryCount, Func<int, TimeSpan> sleepDurations)
        {
            handlers.Add(4000, () => new RetryingDelegatingHandler(retryCount, sleepDurations));
            return this;
        }

        public HttpClientBuilder WithTimeout(TimeSpan timeout)
        {
            handlers.Add(3000, () => new TimeoutingDelegatingHandler(timeout));
            return this;
        }

        public HttpClientBuilder WithQosDefaults()
        {
            return WithCircuitBreaker(3, TimeSpan.FromSeconds(5))
                .WithWaitAndRetry(2, retryAttempt => TimeSpan.FromMilliseconds(300))
                .WithTimeout(TimeSpan.FromSeconds(5));
        }

        public HttpClientBuilder WithCorrelation()
        {
            handlers.Add(2000, () => new CorrelatingDelegatingHandler());
            return this;
        }

        public HttpClientBuilder WithLogging(ILog logger)
        {
            handlers.Add(1000, () => new LoggingDelegatingHandler(logger));
            return this;
        }

        public HttpClientBuilder WithLogging()
        {
            handlers.Add(1000, () => new LoggingDelegatingHandler());
            return this;
        }

        internal HttpClient Build()
        {
            return handlers.Any() ? new HttpClient(CreateHttpMessageHandler()) : new HttpClient();
        }

        private HttpMessageHandler CreateHttpMessageHandler()
        {
            HttpMessageHandler httpMessageHandler = new HttpClientHandler();

            handlers.OrderByDescending(handler => handler.Key).Select(handler => handler.Value).Reverse().ToList().ForEach(handler =>
            {
                var delegatingHandler = handler();
                delegatingHandler.InnerHandler = httpMessageHandler;
                httpMessageHandler = delegatingHandler;
            });

            return httpMessageHandler;
        }
    }
}
