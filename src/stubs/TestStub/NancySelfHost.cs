using System;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;

namespace TestStub
{
    public class NancySelfHost
    {
        private readonly INancyBootstrapper bootstrapper;
        private NancyHost nancyHost;

        public NancySelfHost(INancyBootstrapper bootstrapper)
        {
            this.bootstrapper = bootstrapper;
        }

        public void Stop()
        {
            nancyHost.Stop();
            nancyHost.Dispose();
        }

        public void Start()
        {
            nancyHost = new NancyHost(bootstrapper, new HostConfiguration
            {
                UrlReservations = new UrlReservations
                {
                    CreateAutomatically = true
                },
            }, new Uri("http://localhost:9999"));

            nancyHost.Start();
        }
    }
}
