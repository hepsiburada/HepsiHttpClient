using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace Hepsi.Http.Client.Logging
{
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILog logger;

        public LoggingDelegatingHandler(ILog logger, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.logger = logger;
        }

        public LoggingDelegatingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            logger = LogManager.GetLogger<LoggingDelegatingHandler>();
        }

        internal LoggingDelegatingHandler(ILog logger)
        {
            this.logger = logger;
        }

        internal LoggingDelegatingHandler()
        {
            logger = LogManager.GetLogger<LoggingDelegatingHandler>();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            logger.InfoFormat("Request Metadata: {0}", request);

            if (request.Content != null)
            {
                logger.DebugFormat("Request Content: {0}", await request.Content.ReadAsStringAsync());
            }

            HttpResponseMessage response = null;

            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.WarnFormat("Exception occurred during sending the request. Request URI: {0}, Exception: {1}", request.RequestUri, exception);
                throw;
            }
            finally
            {
                if (response != null)
                {
                    logger.InfoFormat("Response Metadata: {0}", response);

                    if (response.Content != null)
                    {
                        logger.DebugFormat("Response Content: {0}", response.Content.ReadAsStringAsync().Result);
                    }
                }
            }

            return response;
        }
    }
}
