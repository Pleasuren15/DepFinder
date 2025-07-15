namespace DepFinder.Domain.Entities;

public class ClassInfo
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public Type ClassType { get; set; } = null!;
    public List<DependencyInfo> Dependencies { get; set; } = new();
}