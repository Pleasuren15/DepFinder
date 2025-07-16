namespace DepFinder.UnitTests.TestData.Interfaces
{
    public interface ITestRepository
    {
        Task<T> GetByIdAsync<T>(int id);
        Task SaveAsync<T>(T entity);
        Task DeleteAsync(int id);
    }
}