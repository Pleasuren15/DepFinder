using DepFinder.Service.Interfaces;

namespace DepFinder.Service.Implementation
{
    public class InterfaceAImplementation(IInterfaceB interfaceB, IInterfaceC interfaceC) : IInterfaceA
    {
    }
}
