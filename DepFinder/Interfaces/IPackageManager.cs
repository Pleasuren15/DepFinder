namespace DepFinder.Interfaces;

public interface IPackageManager
{
    Task InstallNSubstituteAsync(string projectPath);
    Task<bool> IsPackageInstalledAsync(string projectPath, string packageName);
}