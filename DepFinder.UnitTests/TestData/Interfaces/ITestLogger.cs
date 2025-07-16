namespace DepFinder.UnitTests.TestData.Interfaces
{
    public interface ITestLogger
    {
        void LogInfo(string message);
        void LogError(string message, Exception ex);
        void LogWarning(string message);
    }
}