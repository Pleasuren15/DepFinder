namespace DepFinder.UnitTests.TestData.Interfaces
{
    public interface ITestConfiguration
    {
        string GetConnectionString();
        T GetValue<T>(string key);
        bool IsFeatureEnabled(string feature);
    }
}