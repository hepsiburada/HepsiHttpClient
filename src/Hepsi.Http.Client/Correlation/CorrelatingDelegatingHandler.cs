using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Hepsi.Http.Client.Correlation
{
    public class CorrelatingDelegatingHandler : DelegatingHandler
    {
        public const string CorrelationIdHeader = "X-CorrelationId";

        public CorrelatingDelegatingHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        internal CorrelatingDelegatingHandler() { }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (GetCorrelationIdFromHeader(request) == null)
            {
                var correlationId = GetCorrelationIdFromLogicalCallContext() ?? CreateNewCorrelationId();

                request.Headers.Add(CorrelationIdHeader, correlationId);
            }

            return base.SendAsync(request, cancellationToken);
        }

        private static string GetCorrelationIdFromHeader(HttpRequestMessage request)
        {
            IEnumerable<string> correlationIds;

            return request.Headers.TryGetValues(CorrelationIdHeader, out correlationIds) ? correlationIds.First() : null;
        }

        private static string GetCorrelationIdFromLogicalCallContext()
        {
            try
            {
                return CallContext.LogicalGetData(CorrelationIdHeader) as string;
            }
            catch (SecurityException)
            {
                return null;
            }
        }

        private static string CreateNewCorrelationId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
