using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace SourceGenerator
{
    [Generator]
    public class HelloSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            Console.WriteLine($"{this.GetType()} executing");
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            // Find the main method
            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

            // Build up the source code
            string source = $@" // Auto-generated code
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
            var typeName = mainMethod.ContainingType.Name;

            // Add the source code to the compilation
            context.AddSource($"{typeName}.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            Console.WriteLine($"{this.GetType()} initialized");
            // No initialization required for this one
        }
    }
}