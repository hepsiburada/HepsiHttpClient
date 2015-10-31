using System;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Hepsi.Http.Client.QoS;
using NUnit.Framework;
using TestStub;

namespace Hepsi.Http.Client.ApiTests
{
    [TestFixture]
    public class When_accessing_endpoint_that_returns_timeout_for_the_first_attempt
    {
        private NancySelfHost selfHost;
        private HttpClient httpClient;
        private HttpResponseMessage response2;
        private Exception httpClientException;

        [TestFixtureSetUp]
        public void SetUp()
        {
            selfHost = new NancySelfHost(new RecordingBootstrapper());
            selfHost.Start();

            // setup processiong time on TestModule
            var configurationClient = new HttpClient() { BaseAddress = new Uri("http://localhost:9999") };
            var configurationResponse = configurationClient.PostAsync("configure?processing-time=1000", new StringContent("")).Result;

            httpClient = new HttpClient(new TimeoutingDelegatingHandler(TimeSpan.FromMilliseconds(500), new HttpClientHandler())) { BaseAddress = new Uri("http://localhost:9999") };

            try
            {
                var response1 = httpClient.GetAsync("timeout-for-the-first").Result;
            }
            catch (Exception ex)
            {
                httpClientException = ex;
            }

            response2 = httpClient.GetAsync("timeout-for-the-first").Result;
        }

        [Test]
        public void http_client_should_throw_timeout_exception_for_the_first_request()
        {
            httpClientException.Should().BeOfType<HttpRequestTimeoutException>();
        }

        [Test]
        public void http_client_should_return_200_ok_after_timeout_period()
        {
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            if (selfHost != null)
            {
                selfHost.Stop();
            }
        }
    }
}
