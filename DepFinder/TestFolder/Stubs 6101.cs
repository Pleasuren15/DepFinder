using DepFinder.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DepFinder.Classes
{
    public class Stubs
    {
        public IInterfaceA InterfaceA { get; set; }
        public IConfiguration Configuration { get; set; }
        public ILogger<ClassA> Logger { get; set; }
    }
}
