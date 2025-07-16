using NUnit.Framework;
using DepFinder.Entities;
using DepFinder.UnitTests.TestData;

namespace DepFinder.UnitTests
{
    [TestFixture]
    public class DepFinderTests
    {
        private Type _testClassType;

        [SetUp]
        public void SetUp()
        {
            _testClassType = typeof(ClassA);
        }

        [Test]
        public async Task AnalyzeDependenciesAsync_WithValidClass_ReturnsDependencyInfoArray()
        {
            // Act
            var result = await DepFinder.AnalyzeDependenciesAsync(_testClassType);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<DependencyInfo[]>());
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public async Task GenerateStubClassAsync_WithValidParameters_ReturnsStubContent()
        {
            // Arrange
            var stubClassName = "TestStubs";

            // Act
            var result = await DepFinder.GenerateStubClassAsync(_testClassType, stubClassName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Does.Contain(stubClassName));
            Assert.That(result, Does.Contain("using NSubstitute;"));
        }

        [Test]
        public void GenerateStubClassAsync_WithNullType_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => DepFinder.GenerateStubClassAsync(null, "TestStubs"));
        }

        [Test]
        public void GenerateStubClassAsync_WithEmptyStubClassName_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => DepFinder.GenerateStubClassAsync(_testClassType, ""));
        }

        [Test]
        public async Task GenerateAndSaveStubAsync_WithValidParameters_ReturnsFilePath()
        {
            // Arrange
            var tempDirectory = Path.GetTempPath();
            var outputDirectory = Path.Combine(tempDirectory, "TestOutput");
            Directory.CreateDirectory(outputDirectory);

            try
            {
                // Act
                var result = await DepFinder.GenerateAndSaveStubAsync(_testClassType, outputDirectory);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Not.Empty);
                Assert.That(result, Does.StartWith(outputDirectory));
                Assert.That(result, Does.EndWith(".cs"));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(outputDirectory))
                {
                    Directory.Delete(outputDirectory, true);
                }
            }
        }

        [Test]
        public async Task GenerateAndSaveStubAsync_CreatesFileInCorrectDirectory()
        {
            // Arrange
            var tempDirectory = Path.GetTempPath();
            var outputDirectory = Path.Combine(tempDirectory, "TestOutput2");
            Directory.CreateDirectory(outputDirectory);

            try
            {
                // Act
                var filePath = await DepFinder.GenerateAndSaveStubAsync(_testClassType, outputDirectory);

                // Assert
                Assert.That(File.Exists(filePath), Is.True);
                Assert.That(Path.GetDirectoryName(filePath), Is.EqualTo(outputDirectory));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(outputDirectory))
                {
                    Directory.Delete(outputDirectory, true);
                }
            }
        }

        [Test]
        public void GenerateAndSaveStubAsync_WithNullType_ThrowsException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => DepFinder.GenerateAndSaveStubAsync(null, "TestDirectory"));
        }

        [Test]
        public async Task GivenAllMethods_WithValidProperteis_ThenShouldCreateStubs()
        {
            var currentDirectory = Directory.GetParent(".")!.Parent!.Parent;

            //var dependencies = await DepFinder.AnalyzeDependenciesAsync(typeof(ClassA));
            //var stubContent = await DepFinder.GenerateStubClassAsync(typeof(ClassA), "UsersControllerStubs");
            var filePath = await DepFinder.GenerateAndSaveStubAsync(typeof(ClassA), $"{currentDirectory}//Testes");

            // Create Sut Factory
            var outputDirectory = Directory.GetCurrentDirectory();
            await DepFinder.GenerateSutFactoryClassAsync(typeof(ClassA), typeof(ClassA).Name, filePath, $"{currentDirectory}//Testes");
        }
    }
}