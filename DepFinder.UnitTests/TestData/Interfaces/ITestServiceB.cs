namespace DepFinder.UnitTests.TestData.Interfaces
{
    public interface ITestServiceB
    {
        Task<int> CalculateAsync(int value);
        bool ValidateInput(string input);
    }
}