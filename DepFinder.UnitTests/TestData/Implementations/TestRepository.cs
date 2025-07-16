using DepFinder.UnitTests.TestData.Interfaces;

namespace DepFinder.UnitTests.TestData.Implementations
{
    public class TestRepository : ITestRepository
    {
        private readonly ITestConfiguration _configuration;
        private readonly ITestLogger _logger;

        public TestRepository(ITestConfiguration configuration, ITestLogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task<T> GetByIdAsync<T>(int id)
        {
            return Task.FromResult(default(T));
        }

        public Task SaveAsync<T>(T entity)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            return Task.CompletedTask;
        }
    }
}