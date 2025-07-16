using DepFinder.Entities;

namespace DepFinder.Interfaces;

public interface IStubGenerator
{
    string GenerateClassWithInterfaceProperties(Type sourceClassType, string newClassName);
    string GenerateSutFactoryClass(Type sourceClassType, string stubClassName, string stubFilePath);
}