namespace SimpleExample;

public static partial class Test
{
    public static void Execute()
    {
        PartialFunctionNotOnMain("Me");
    }
    
    static partial void PartialFunctionNotOnMain(string name);
}