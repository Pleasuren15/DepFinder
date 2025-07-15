using DepFinder.Domain.Interfaces;

namespace DepFinder.Application.Services;

public class PackageInstallationService
{
    private readonly IPackageManager _packageManager;

    public PackageInstallationService(IPackageManager packageManager)
    {
        _packageManager = packageManager;
    }

    public async Task EnsureNSubstituteInstalledAsync(string projectPath)
    {
        await _packageManager.InstallNSubstituteAsync(projectPath);
    }

    public async Task<bool> IsPackageInstalledAsync(string projectPath, string packageName)
    {
        return await _packageManager.IsPackageInstalledAsync(projectPath, packageName);
    }
}