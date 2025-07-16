using DepFinder.UnitTests.TestData.Interfaces;

namespace DepFinder.UnitTests.TestData
{
    public class ClassB
    {
        private readonly ITestServiceB _testServiceB;
        private readonly ITestRepository _repository;
        private readonly ITestConfiguration _configuration;

        public ClassB(ITestServiceB testServiceB, ITestRepository repository, ITestConfiguration configuration)
        {
            _testServiceB = testServiceB;
            _repository = repository;
            _configuration = configuration;
        }

        public async Task<int> PerformCalculationAsync(string input)
        {
            if (!_testServiceB.ValidateInput(input))
            {
                throw new ArgumentException("Invalid input");
            }

            var value = _configuration.GetValue<int>("DefaultValue");
            var result = await _testServiceB.CalculateAsync(value);
            
            await _repository.SaveAsync(result);
            
            return result;
        }
    }
}