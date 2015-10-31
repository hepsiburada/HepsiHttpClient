using System.Collections.Generic;
using System.Net;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using TestStub;

namespace Hepsi.Http.Client.ApiTests
{
    public class RecordingBootstrapper : DefaultNancyBootstrapper
    {
        public List<Request> RecordedRequests { get; private set; }
        public List<Response> RecordedResponses { get; private set; }

        public RecordingBootstrapper()
        {
            RecordedRequests = new List<Request>();
            RecordedResponses = new List<Response>();
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            pipelines.BeforeRequest += (ctx) =>
            {
                RecordedRequests.Add(ctx.Request);
                return null;
            };
            pipelines.AfterRequest += (ctx) => RecordedResponses.Add(ctx.Response);

            base.ApplicationStartup(container, pipelines);
        }

        protected override IEnumerable<ModuleRegistration> Modules
        {
            get
            {
                var testModule = new ModuleRegistration(typeof(TestModule));

                return new List<ModuleRegistration>
                {
                    testModule
                };
            }
        }
    }
}
