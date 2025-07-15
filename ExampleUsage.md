# DepFinder NuGet Package Usage Examples

This document shows how to use the DepFinder NuGet package in another solution.

## Installation

```bash
dotnet add package DepFinder
```

## Example 1: Console Application

### MyConsoleApp.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="DepFinder" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### Program.cs
```csharp
using DepFinder.Service;

// Example classes to analyze
public interface IUserService { }
public interface IEmailService { }
public interface ILoggerService { }

public class UserController
{
    public UserController(IUserService userService, IEmailService emailService, ILoggerService logger)
    {
        // Constructor injection
    }
}

// === SIMPLE STATIC USAGE (No DI setup required) ===
Console.WriteLine("=== Static Usage Example ===");

// Analyze dependencies
var dependencies = await DepFinder.AnalyzeDependenciesAsync(typeof(UserController));
Console.WriteLine($"Found {dependencies.Length} dependencies:");
foreach (var dep in dependencies)
{
    Console.WriteLine($"  - {dep.TypeName} (Namespace: {dep.Namespace})");
}

// Generate stub class
var stubContent = await DepFinder.GenerateStubClassAsync(typeof(UserController), "UserControllerStubs");
Console.WriteLine("\nGenerated stub class:");
Console.WriteLine(stubContent);

// Save to file
var filePath = await DepFinder.GenerateAndSaveStubAsync(typeof(UserController), "./TestStubs");
Console.WriteLine($"\nStub saved to: {filePath}");

// Ensure NSubstitute is installed
await DepFinder.EnsureNSubstituteInstalledAsync("./MyConsoleApp.csproj");
```

## Example 2: ASP.NET Core Web API

### MyWebApi.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="DepFinder" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### Program.cs
```csharp
using DepFinder.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add DepFinder with one line!
builder.Services.AddDepFinder();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseRouting();
app.MapControllers();

// Example endpoint to generate stubs
app.MapPost("/generate-stubs", async (Type targetType, DepFinderService depFinder) =>
{
    var dependencies = await depFinder.AnalyzeDependenciesAsync(targetType);
    var stubContent = await depFinder.GenerateStubClassAsync(targetType, $"{targetType.Name}Stubs");
    
    return Results.Ok(new { dependencies, stubContent });
});

app.Run();
```

### Controllers/ProductController.cs
```csharp
using Microsoft.AspNetCore.Mvc;

public interface IProductService { }
public interface IInventoryService { }
public interface ICacheService { }

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IInventoryService _inventoryService;
    private readonly ICacheService _cacheService;

    public ProductController(
        IProductService productService,
        IInventoryService inventoryService,
        ICacheService cacheService)
    {
        _productService = productService;
        _inventoryService = inventoryService;
        _cacheService = cacheService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        // Use DepFinder to generate test stubs for this controller
        var stubContent = await DepFinder.GenerateStubClassAsync(typeof(ProductController), "ProductControllerStubs");
        
        return Ok(new { 
            message = "Products retrieved", 
            testStubs = stubContent 
        });
    }
}
```

## Example 3: Unit Test Project

### MyProject.Tests.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.4.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.4.5" />
    <PackageReference Include="DepFinder" Version="1.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../MyProject/MyProject.csproj" />
  </ItemGroup>
</Project>
```

### StubGeneratorTests.cs
```csharp
using DepFinder.Service;
using Xunit;

public class StubGeneratorTests
{
    [Fact]
    public async Task GenerateStubs_ForComplexController_ShouldCreateAllDependencies()
    {
        // Arrange
        var targetType = typeof(OrderController);
        
        // Act - Using static method (no setup required)
        var dependencies = await DepFinder.AnalyzeDependenciesAsync(targetType);
        var stubContent = await DepFinder.GenerateStubClassAsync(targetType, "OrderControllerStubs");
        
        // Assert
        Assert.NotEmpty(dependencies);
        Assert.Contains("NSubstitute", stubContent);
        Assert.Contains("OrderControllerStubs", stubContent);
    }
    
    [Fact]
    public async Task GenerateStubs_WithDisposal_ShouldWork()
    {
        // Arrange & Act
        using var depFinder = DepFinder.Create();
        var result = await depFinder.GenerateStubClassAsync(typeof(OrderController), "TestStubs");
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("public class TestStubs", result);
    }
}

// Example classes being tested
public interface IOrderService { }
public interface IPaymentService { }
public interface IShippingService { }
public interface INotificationService { }

public class OrderController
{
    public OrderController(
        IOrderService orderService,
        IPaymentService paymentService,
        IShippingService shippingService,
        INotificationService notificationService)
    {
        // Dependencies injected
    }
}
```

## Example 4: Build Tool / Code Generator

### BuildTool.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="DepFinder" Version="1.0.0" />
    <PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
  </ItemGroup>
</Project>
```

### Program.cs
```csharp
using DepFinder.Service;
using System.CommandLine;
using System.Reflection;

var rootCommand = new RootCommand("Generate test stubs for classes");

var classNameOption = new Option<string>(
    name: "--class",
    description: "The class name to generate stubs for");

var outputOption = new Option<string>(
    name: "--output",
    description: "Output directory for generated stubs",
    getDefaultValue: () => "./GeneratedStubs");

var assemblyOption = new Option<string>(
    name: "--assembly",
    description: "Assembly path to load types from");

rootCommand.AddOption(classNameOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(assemblyOption);

rootCommand.SetHandler(async (string className, string output, string assemblyPath) =>
{
    try
    {
        // Load assembly
        var assembly = Assembly.LoadFrom(assemblyPath);
        var type = assembly.GetTypes().FirstOrDefault(t => t.Name == className);
        
        if (type == null)
        {
            Console.WriteLine($"Class '{className}' not found in assembly");
            return;
        }

        // Generate stubs using DepFinder
        Console.WriteLine($"Analyzing {className}...");
        var dependencies = await DepFinder.AnalyzeDependenciesAsync(type);
        Console.WriteLine($"Found {dependencies.Length} dependencies");

        var stubClassName = $"{className}Stubs";
        var stubContent = await DepFinder.GenerateStubClassAsync(type, stubClassName);
        
        // Save to file
        var filePath = await DepFinder.GenerateAndSaveStubAsync(type, output);
        Console.WriteLine($"Generated stub saved to: {filePath}");
        
        // Ensure NSubstitute is installed in target project
        var projectFiles = Directory.GetFiles(".", "*.csproj", SearchOption.AllDirectories);
        foreach (var project in projectFiles)
        {
            await DepFinder.EnsureNSubstituteInstalledAsync(project);
        }
        
        Console.WriteLine("✅ Stub generation completed successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
}, classNameOption, outputOption, assemblyOption);

await rootCommand.InvokeAsync(args);
```

## Example 5: Minimal API with Factory Pattern

### MinimalApiExample.cs
```csharp
using DepFinder.Service;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Simple endpoint using static methods
app.MapGet("/analyze/{typeName}", async (string typeName) =>
{
    var type = Type.GetType(typeName);
    if (type == null) return Results.NotFound($"Type '{typeName}' not found");
    
    var dependencies = await DepFinder.AnalyzeDependenciesAsync(type);
    return Results.Ok(dependencies);
});

// Endpoint using factory with disposal
app.MapPost("/generate-stub", async (GenerateStubRequest request) =>
{
    var type = Type.GetType(request.TypeName);
    if (type == null) return Results.NotFound($"Type '{request.TypeName}' not found");
    
    using var depFinder = DepFinder.Create();
    var stubContent = await depFinder.GenerateStubClassAsync(type, request.StubName);
    
    return Results.Ok(new { stubContent });
});

app.Run();

public record GenerateStubRequest(string TypeName, string StubName);
```

## Key Benefits for Users

1. **Zero Configuration**: No DI setup required for basic usage
2. **Multiple Patterns**: Static methods, factory, or DI integration
3. **Async/Await**: All methods are properly async
4. **Automatic Cleanup**: Proper disposal handling
5. **One-Line DI**: Simple `AddDepFinder()` extension method
6. **Framework Agnostic**: Works with Console, Web API, Blazor, etc.

## Common Usage Patterns

```csharp
// Quick one-liner for simple cases
var stubs = await DepFinder.GenerateStubClassAsync(typeof(MyController), "MyStubs");

// With proper disposal
using var depFinder = DepFinder.Create();
var result = await depFinder.AnalyzeDependenciesAsync(typeof(MyService));

// In DI container
builder.Services.AddDepFinder();
var depFinder = services.GetRequiredService<DepFinderService>();
```