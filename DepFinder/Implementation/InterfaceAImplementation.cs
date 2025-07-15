using DepFinder.Interfaces;

namespace DepFinder.Implementation
{
    public class InterfaceAImplementation(IInterfaceB interfaceB, IInterfaceC interfaceC) : Interfaces.IInterfaceA
    {
    }
}
