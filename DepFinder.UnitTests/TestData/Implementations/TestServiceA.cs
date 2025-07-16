using DepFinder.UnitTests.TestData.Interfaces;

namespace DepFinder.UnitTests.TestData.Implementations
{
    public class TestServiceA : ITestServiceA
    {
        private readonly ITestLogger _logger;
        private readonly ITestConfiguration _configuration;

        public TestServiceA(ITestLogger logger, ITestConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Task<string> GetDataAsync()
        {
            return Task.FromResult("data");
        }

        public void ProcessData(string data)
        {
        }
    }
}