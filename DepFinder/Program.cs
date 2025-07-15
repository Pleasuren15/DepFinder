using DepFinder.Classes;
using System.Text.Json;

var interfaces = GetInterfacesFromClass(typeof(ClassA));
Console.WriteLine(JsonSerializer.Serialize(interfaces, new JsonSerializerOptions() { WriteIndented = true }));


static Type[] GetInterfacesFromClass(Type classType)
{
    if (classType == null)
        throw new ArgumentNullException(nameof(classType));

    if (!classType.IsClass)
        throw new ArgumentException("Provided type must be a class", nameof(classType));

    return classType.GetInterfaces();
}

