using System.Text;
using Microsoft.CodeAnalysis;

namespace SourceGenerator;

public class FunctionTextProvider
{
    public static string GetFunctionText(string name)
    {
        var sourceBuilder = new StringBuilder($@"using System;
namespace HelloWorldGenerated
{{
    public static class HelloWorld
    {{
        public static void SayHello() 
        {{
            Console.WriteLine(""Hello from generated code! {name}"");
            Console.WriteLine(""The following syntax trees existed in the compilation that created this program:"");
        }}
    }}
}}");
        return sourceBuilder.ToString();
    }
}