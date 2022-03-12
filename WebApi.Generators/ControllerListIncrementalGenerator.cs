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
    public class ControllerListIncrementalGenerator : IIncrementalGenerator
    {
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

            IncrementalValuesProvider<ClassDeclarationSyntax> controllerDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(predicate: (node, token) => node is ClassDeclarationSyntax,
                    (syntaxContext, token) =>
                    {
                        var classDeclarationSyntax = (ClassDeclarationSyntax)syntaxContext.Node;

                        if (classDeclarationSyntax.Identifier.Value?.ToString().EndsWith("Controller") ?? false)
                        {
                            return classDeclarationSyntax;
                        }
                        else
                        {
                            return null;
                        }
                    }).Where(m => m is not null);

            IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndControllers
                = context.CompilationProvider.Combine(controllerDeclarations
                    .Collect()); // TODO: What is this combine operation for?

            context.RegisterSourceOutput(compilationAndControllers,
                static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> controllers,
            SourceProductionContext context)
        {
            if (controllers.IsDefaultOrEmpty)
            {
                // nothing to do yet
                return;
            }

            var controllerNames = new List<string>();
            // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
            IEnumerable<ClassDeclarationSyntax> distinctControllers = controllers.Distinct();
            foreach (var classDeclarationSyntax in distinctControllers)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
                var controllerSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                controllerNames.Add($"\"{controllerSymbol.Name}\"");
            }


            context.AddSource("ControllerListController.g.cs", FunctionTextProvider.GetFunctionText(controllerNames));
        }
    }
}