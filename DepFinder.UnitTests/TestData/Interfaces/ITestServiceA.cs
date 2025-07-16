namespace DepFinder.UnitTests.TestData.Interfaces
{
    public interface ITestServiceA
    {
        Task<string> GetDataAsync();
        void ProcessData(string data);
    }
}