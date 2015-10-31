using Common.Logging;
using Common.Logging.Simple;
using NUnit.Framework;

namespace Hepsi.Http.Client.UnitTests
{
    [SetUpFixture]
    public class UnitTestsFixture
    {
        [SetUp]
        public void SetUp()
        {
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(LogLevel.Warn, true, true, true, "yyyyMMddHHmmss");
        }
    }
}
