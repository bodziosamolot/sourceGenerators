using Microsoft.CodeAnalysis;

namespace IncrementalSourceGenerator;

public class HelloWorldFunctionText
{
    public static string FunctionText(IMethodSymbol mainMethod) => $@" // Auto-generated code
using System;

namespace {mainMethod.ContainingNamespace.ToDisplayString()}
{{
    public static partial class {mainMethod.ContainingType.Name}
    {{
        static partial void HelloFrom(string name) =>
            Console.WriteLine($""Generator says: Hi from '{{name}}'"");
    }}
}}
";
}