using DepFinder.UnitTests.TestData.Interfaces;

namespace DepFinder.UnitTests.TestData.Implementations
{
    public class TestConfiguration : ITestConfiguration
    {
        private readonly ITestServiceA _serviceA;
        private readonly ITestServiceB _serviceB;

        public TestConfiguration(ITestServiceA serviceA, ITestServiceB serviceB)
        {
            _serviceA = serviceA;
            _serviceB = serviceB;
        }

        public string GetConnectionString()
        {
            return "connection";
        }

        public T GetValue<T>(string key)
        {
            return default(T);
        }

        public bool IsFeatureEnabled(string feature)
        {
            return true;
        }
    }
}