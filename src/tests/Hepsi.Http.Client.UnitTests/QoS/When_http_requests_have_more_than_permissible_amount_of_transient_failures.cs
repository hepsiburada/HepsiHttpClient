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
    public class When_http_requests_have_more_than_permissible_amount_of_transient_failures
    {
        private const string TEST_URI = "http://test/my-occasionally-failing-endpoint";
        private const int RETRY_COUNT = 3;

        private HttpClient httpClient;
        private HttpResponseMessage response;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var fakeResponseDelegatingHandler = new FakeResponseDelegatingHandler();

            fakeResponseDelegatingHandler.AddFakeResponse(new Uri(TEST_URI), () => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            fakeResponseDelegatingHandler.AddFakeResponse(new Uri(TEST_URI), () => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            fakeResponseDelegatingHandler.AddFakeResponse(new Uri(TEST_URI), () => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            fakeResponseDelegatingHandler.AddFakeResponse(new Uri(TEST_URI), () => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            fakeResponseDelegatingHandler.AddFakeResponse(new Uri(TEST_URI), () => new HttpResponseMessage(HttpStatusCode.OK));

            httpClient = new HttpClient(new RetryingDelegatingHandler(RETRY_COUNT, fakeResponseDelegatingHandler));

            response = httpClient.GetAsync(TEST_URI).Result;
        }

        [Test]
        public void http_client_should_return_the_relevant_http_error_code()
        {
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}
