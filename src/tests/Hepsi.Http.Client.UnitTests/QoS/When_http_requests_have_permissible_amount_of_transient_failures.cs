using System;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Hepsi.Http.Client.QoS;
using Hepsi.Http.Client.Testing;
using NUnit.Framework;

namespace Hepsi.Http.Client.UnitTests.QoS
{
    [TestFixture]
    public class When_http_requests_have_permissible_amount_of_transient_failures
    {
        private const string TestUri = "http://test/my-occasionally-failing-endpoint";
        private const int RetryCount = 3;

        private HttpClient httpClient;
        private HttpResponseMessage response;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var fakeResponseDelegatingHandler = new FakeResponseDelegatingHandler();

            fakeResponseDelegatingHandler.AddFakeResponse(new Uri(TestUri), () => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            fakeResponseDelegatingHandler.AddFakeResponse(new Uri(TestUri), () => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            fakeResponseDelegatingHandler.AddFakeResponse(new Uri(TestUri), () => new HttpResponseMessage(HttpStatusCode.OK));

            httpClient = new HttpClient(new RetryingDelegatingHandler(RetryCount, fakeResponseDelegatingHandler));

            response = httpClient.GetAsync(TestUri).Result;
        }

        [Test]
        public void http_client_should_return_200_ok()
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
