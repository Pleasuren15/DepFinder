namespace DepFinder.Entities;

public class ClassInfo
{
    public string Namespace { get; set; } = string.Empty;
    public List<DependencyInfo> Dependencies { get; set; } = new();
}