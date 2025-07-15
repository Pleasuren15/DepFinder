using DepFinder.Interfaces;
using System.Diagnostics;
using System.Xml.Linq;

namespace DepFinder.Services;

public class PackageManager : IPackageManager
{
    public async Task InstallNSubstituteAsync(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
            throw new ArgumentException("Project path cannot be null or empty", nameof(projectPath));

        if (!File.Exists(projectPath))
            throw new FileNotFoundException($"Project file not found: {projectPath}");

        try
        {
            var hasNSubstitute = await IsPackageInstalledAsync(projectPath, "NSubstitute");

            if (hasNSubstitute)
            {
                Console.WriteLine("NSubstitute package already exists in the project. Skipping installation.");
                return;
            }

            Console.WriteLine("NSubstitute package not found. Installing latest version...");

            var projectDirectory = Path.GetDirectoryName(projectPath);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "add package NSubstitute",
                WorkingDirectory = projectDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("NSubstitute package installed successfully.");
                Console.WriteLine(output);
            }
            else
            {
                Console.WriteLine($"Failed to install NSubstitute package. Exit code: {process.ExitCode}");
                Console.WriteLine($"Error: {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error installing NSubstitute: {ex.Message}");
        }
    }

    public async Task<bool> IsPackageInstalledAsync(string projectPath, string packageName)
    {
        try
        {
            var projectContent = await File.ReadAllTextAsync(projectPath);
            var doc = XDocument.Parse(projectContent);

            return doc.Descendants("PackageReference")
                .Any(x => x.Attribute("Include")?.Value.Equals(packageName, StringComparison.OrdinalIgnoreCase) == true);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task InstallPackageAsync(string projectPath, string packageName, string version = "latest")
    {
        if (string.IsNullOrWhiteSpace(projectPath))
            throw new ArgumentException("Project path cannot be null or empty", nameof(projectPath));

        if (string.IsNullOrWhiteSpace(packageName))
            throw new ArgumentException("Package name cannot be null or empty", nameof(packageName));

        if (!File.Exists(projectPath))
            throw new FileNotFoundException($"Project file not found: {projectPath}");

        try
        {
            var hasPackage = await IsPackageInstalledAsync(projectPath, packageName);

            if (hasPackage)
            {
                Console.WriteLine($"{packageName} package already exists in the project. Skipping installation.");
                return;
            }

            Console.WriteLine($"{packageName} package not found. Installing version {version}...");

            var projectDirectory = Path.GetDirectoryName(projectPath);
            var arguments = version == "latest" || string.IsNullOrEmpty(version) 
                ? $"add package {packageName}"
                : $"add package {packageName} --version {version}";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = projectDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                Console.WriteLine($"{packageName} package installed successfully.");
                Console.WriteLine(output);
            }
            else
            {
                Console.WriteLine($"Failed to install {packageName} package. Exit code: {process.ExitCode}");
                Console.WriteLine($"Error: {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error installing {packageName}: {ex.Message}");
        }
    }
}