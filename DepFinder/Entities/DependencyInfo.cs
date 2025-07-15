namespace DepFinder.Entities;

public class DependencyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public bool IsGeneric { get; set; }
    public List<string> GenericArguments { get; set; } = new();
}