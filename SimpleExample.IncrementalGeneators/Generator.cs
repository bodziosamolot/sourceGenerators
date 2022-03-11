using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace IncrementalSourceGenerator
{
    [Generator]
    public class HelloSourceGenerator :IIncrementalGenerator 
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            Console.WriteLine($"{this.GetType()} initialized");
            #if DEBUG
                        if (!Debugger.IsAttached)
                        {
                            Debugger.Launch();
                        }
            #endif
            context.SyntaxProvider.CreateSyntaxProvider(predicate: (node, token) => node is MethodDeclarationSyntax,
                (syntaxContext, token) =>
                {
                    var methodDeclarationSyntax = (MethodDeclarationSyntax)syntaxContext.Node;

                    if (methodDeclarationSyntax.Identifier.Value == "Main")
                    {
                        return methodDeclarationSyntax;
                    }
                    else
                    {
                        return null;
                    }
                });
        }
    }
}