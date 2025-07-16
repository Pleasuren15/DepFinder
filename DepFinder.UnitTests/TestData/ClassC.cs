using DepFinder.UnitTests.TestData.Interfaces;

namespace DepFinder.UnitTests.TestData
{
    public class ClassC
    {
        private readonly ITestServiceA _testServiceA;
        private readonly ITestServiceB _testServiceB;
        private readonly ITestRepository _repository;
        private readonly ITestLogger _logger;
        private readonly ITestConfiguration _configuration;

        public ClassC(
            ITestServiceA testServiceA,
            ITestServiceB testServiceB,
            ITestRepository repository,
            ITestLogger logger,
            ITestConfiguration configuration)
        {
            _testServiceA = testServiceA;
            _testServiceB = testServiceB;
            _repository = repository;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> ExecuteComplexOperationAsync(string input)
        {
            try
            {
                _logger.LogInfo("Starting complex operation");

                if (!_testServiceB.ValidateInput(input))
                {
                    _logger.LogWarning("Invalid input provided");
                    return false;
                }

                var data = await _testServiceA.GetDataAsync();
                var calculatedValue = await _testServiceB.CalculateAsync(data.Length);

                if (_configuration.IsFeatureEnabled("SaveResults"))
                {
                    await _repository.SaveAsync(calculatedValue);
                }

                _testServiceA.ProcessData(data);
                _logger.LogInfo("Complex operation completed successfully");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in complex operation", ex);
                return false;
            }
        }
    }
}