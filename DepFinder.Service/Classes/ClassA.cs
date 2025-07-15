using DepFinder.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DepFinder.Service.Classes
{
    public class ClassA(IInterfaceA interfaceA, IConfiguration configuration, ILogger<ClassA> logger)
    {
    }
}
