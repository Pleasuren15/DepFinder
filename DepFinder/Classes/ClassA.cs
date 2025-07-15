using DepFinder.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DepFinder.Classes
{
    public class ClassA(IInterfaceA interfaceA, IConfiguration configuration, ILogger<ClassA> logger) { }
}
