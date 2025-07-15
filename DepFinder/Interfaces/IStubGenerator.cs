using DepFinder.Entities;

namespace DepFinder.Interfaces;

public interface IStubGenerator
{
    string GenerateClassWithInterfaceProperties(Type sourceClassType, string newClassName);
    string GenerateStubClass(ClassInfo classInfo, string stubClassName);
}