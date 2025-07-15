namespace DepFinder.Interfaces;

public interface IPackageManager
{
    Task InstallNSubstituteAsync(string projectPath);
    Task<bool> IsPackageInstalledAsync(string projectPath, string packageName);
    Task InstallPackageAsync(string projectPath, string packageName, string version = "latest");
}