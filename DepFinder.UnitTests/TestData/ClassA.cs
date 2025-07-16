using DepFinder.UnitTests.TestData.Interfaces;

namespace DepFinder.UnitTests.TestData
{
    public class ClassA
    {
        private readonly ITestServiceA _testServiceA;
        private readonly ITestLogger _logger;

        public ClassA(ITestServiceA testServiceA, ITestLogger logger)
        {
            _testServiceA = testServiceA;
            _logger = logger;
        }

        public async Task<string> ProcessDataAsync()
        {
            try
            {
                var data = await _testServiceA.GetDataAsync();
                _testServiceA.ProcessData(data);
                _logger.LogInfo("Data processed successfully");
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing data", ex);
                throw;
            }
        }
    }
}