using DepFinder.UnitTests.TestData.Interfaces;

namespace DepFinder.UnitTests.TestData.Implementations
{
    public class TestServiceB : ITestServiceB
    {
        private readonly ITestRepository _repository;
        private readonly ITestLogger _logger;

        public TestServiceB(ITestRepository repository, ITestLogger logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public Task<int> CalculateAsync(int value)
        {
            return Task.FromResult(value);
        }

        public bool ValidateInput(string input)
        {
            return true;
        }
    }
}