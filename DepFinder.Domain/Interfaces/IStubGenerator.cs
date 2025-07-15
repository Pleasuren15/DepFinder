using DepFinder.Domain.Entities;

namespace DepFinder.Domain.Interfaces;

public interface IStubGenerator
{
    string GenerateClassWithInterfaceProperties(Type sourceClassType, string newClassName);
    string GenerateStubClass(ClassInfo classInfo, string stubClassName);
}