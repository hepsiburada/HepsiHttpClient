using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using FluentAssertions;
using Hepsi.Http.Client.QoS;
using Hepsi.Http.Client.Testing;
using NUnit.Framework;
using Polly.CircuitBreaker;

namespace Hepsi.Http.Client.UnitTests.QoS
{
    [TestFixture]
    public class When_unsuccessful_http_requests_break_the_circuit
    {
        private const string TEST_URI = "http://test/my-temporarily-breaking-endpoint";
        private const int EXCEPTIONS_ALLOWED_BEFORE_BREAKING = 2;

        private readonly TimeSpan circuitOpenDuration = TimeSpan.FromMilliseconds(200);

        private HttpClient httpClient;
        private HttpResponseMessage response1;
        private HttpResponseMessage response2;
        private HttpResponseMessage response4;
        private BrokenCircuitException httpClientException;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var fakeResponseDelegatingHandler = new FakeResponseDelegatingHandler();

            fakeResponseDelegatingHandler.AddFakeResponse(new Uri(TEST_URI), () => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            fakeResponseDelegatingHandler.AddFakeResponse(new Uri(TEST_URI), () => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            fakeResponseDelegatingHandler.AddFakeResponse(new Uri(TEST_URI), () => new HttpResponseMessage(HttpStatusCode.OK));

            httpClient = new HttpClient(new CircuitBreakingDelegatingHandler(EXCEPTIONS_ALLOWED_BEFORE_BREAKING, circuitOpenDuration, fakeResponseDelegatingHandler));

            // trigger circuit break
            response1 = httpClient.GetAsync(TEST_URI).Result;
            response2 = httpClient.GetAsync(TEST_URI).Result;

            // circuit should be open
            try
            {
                var response3 = httpClient.GetAsync(TEST_URI).Result;
            }
            catch (BrokenCircuitException brokenCircuitException)
            {
                httpClientException = brokenCircuitException;
            }

            WaitCircuitToBeClosed();

            response4 = httpClient.GetAsync(TEST_URI).Result;
        }

        [Test]
        public void http_client_should_return_the_relevant_http_error_code_for_the_first_failed_request()
        {
            response1.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public void http_client_should_return_the_relevant_http_error_code_for_the_second_failed_request()
        {
            response2.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public void http_client_should_throw_broken_circuit_exception_while_circuit_is_open()
        {
            httpClientException.Should().BeOfType<BrokenCircuitException>();
        }

        [Test]
        public void http_client_should_return_200_ok_after_circuit_get_closed()
        {
            response4.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private void WaitCircuitToBeClosed()
        {
            Thread.Sleep(circuitOpenDuration.Add(TimeSpan.FromMilliseconds(100)));
        }
    }
}
