using DepFinder.Application.Services;
using DepFinder.Domain.Interfaces;
using DepFinder.Infrastructure.Services;
using DepFinder.Service.Classes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Register services
builder.Services.AddScoped<IDependencyAnalyzer, DependencyAnalyzer>();
builder.Services.AddScoped<IStubGenerator, StubGenerator>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IPackageManager, PackageManager>();
builder.Services.AddScoped<DependencyAnalysisService>();
builder.Services.AddScoped<PackageInstallationService>();

var host = builder.Build();

// Get services
var dependencyAnalysisService = host.Services.GetRequiredService<DependencyAnalysisService>();
var packageInstallationService = host.Services.GetRequiredService<PackageInstallationService>();

// Main logic
var currentDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!
                                    .Parent!.Parent;

Console.WriteLine($"Current Directory: {currentDirectory}");

// Install NSubstitute if needed
await packageInstallationService.EnsureNSubstituteInstalledAsync($"{currentDirectory}//DepFinder.Service//DepFinder.Service.csproj");

// Generate stubs
var stubContent = await dependencyAnalysisService.GenerateStubsAsync(typeof(ClassA), $"{currentDirectory}//DepFinder.Service//TestFolder");
Console.WriteLine(stubContent);

Console.WriteLine("Dependency analysis completed successfully!");


