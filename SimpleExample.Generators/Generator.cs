using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator
{
    [Generator]
    public class HelloSourceIncrementalGenerator : IIncrementalGenerator
    {
        public HelloSourceIncrementalGenerator()
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                // Debugger.Launch();
            }
#endif
        }

        public void Execute(GeneratorExecutionContext context)
        {
            Console.WriteLine($"{this.GetType()} executing");

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

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            Console.WriteLine($"{this.GetType()} initialized");
            if (!Debugger.IsAttached)
            {
                // Debugger.Launch();
            }

            var mainDeclarations = context.SyntaxProvider.CreateSyntaxProvider(predicate: (node, token) => node is MethodDeclarationSyntax,
                (syntaxContext, token) =>
                {
                    var methodDeclarationSyntax = (MethodDeclarationSyntax)syntaxContext.Node;

                    if (methodDeclarationSyntax.Identifier.Value?.ToString() == "Main")
                    {
                        return methodDeclarationSyntax;
                    }
                    else
                    {
                        return null;
                    }
                }).Where(m => m is not null);
            
            IncrementalValueProvider<(Compilation, ImmutableArray<MethodDeclarationSyntax>)> compilationAndEnums
                = context.CompilationProvider.Combine(mainDeclarations.Collect());
            
            context.RegisterSourceOutput(compilationAndEnums,
                static (spc, source) => Execute(source.Item1, source.Item2, spc)); 
        }
        
        static void Execute(Compilation compilation, ImmutableArray<MethodDeclarationSyntax> mains, SourceProductionContext context)
        {
            if (mains.IsDefaultOrEmpty)
            {
                // nothing to do yet
                return;
            }

            // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
            IEnumerable<MethodDeclarationSyntax> distinctMains = mains.Distinct();
            var mainMethodSyntax = distinctMains.First();
            
                
            SemanticModel semanticModel = compilation.GetSemanticModel(mainMethodSyntax.SyntaxTree);
            var mainMethodSymbol = semanticModel.GetDeclaredSymbol(mainMethodSyntax); 
                    
            context.AddSource("Chuj.g.cs", FunctionTextProvider.GetFunctionText(mainMethodSymbol.ToString()));
        }
    }
}