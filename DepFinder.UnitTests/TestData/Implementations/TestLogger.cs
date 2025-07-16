using DepFinder.UnitTests.TestData.Interfaces;

namespace DepFinder.UnitTests.TestData.Implementations
{
    public class TestLogger : ITestLogger
    {
        private readonly ITestConfiguration _configuration;

        public TestLogger(ITestConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void LogInfo(string message)
        {
        }

        public void LogError(string message, Exception ex)
        {
        }

        public void LogWarning(string message)
        {
        }
    }
}