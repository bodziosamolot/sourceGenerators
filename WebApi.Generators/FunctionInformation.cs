namespace SourceGenerator;

public class FunctionInformation
{
    public string Name { get; private set; }
    public string ParentClass { get; private set; }
    public string Kind { get; private set; }
    public string Flags { get; private set; }

    public FunctionInformation(string name, string parentClass, string kind, string flags)
    {
        Name = name;
        ParentClass = parentClass;
        Kind = kind;
        Flags = flags;
    }
}