using System;
using System.Threading;
using Nancy;

namespace TestStub
{
    public class TestModule : NancyModule
    {
        private static bool isTimedOut;
        private static double processingTime = 1500;

        public TestModule()
        {
            Get["/test-me"] = x => "test";

            Get["/timeout-for-the-first"] = param =>
            {
                if (!isTimedOut)
                {
                    isTimedOut = true;

                    WaitForProcessing();

                    return HttpStatusCode.OK;
                }
                return HttpStatusCode.OK;
            };

            Post["/configure"] = param =>
            {
                processingTime = Request.Query["processingtime"];
                return null;
            };
        }

        private static void WaitForProcessing()
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(processingTime));
        }
    }
}
