using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WebApi.IncrementalGenerators
{
    [Generator]
    public class ControllerListIncrementalGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<INamedTypeSymbol> controllerDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, token) =>
                    {
                        return node is ClassDeclarationSyntax && ((ClassDeclarationSyntax)node)
                            .Identifier.Value.ToString().EndsWith("Controller");
                    },
                    (syntaxContext, token) =>
                    {
                        var classDeclarationSyntax = syntaxContext.Node as ClassDeclarationSyntax;
                        if (classDeclarationSyntax is null)
                        {
                            return null;
                        }

                        var controllerSymbol =
                            syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
                        if (controllerSymbol != null && controllerSymbol.BaseType.Name == nameof(ControllerBase))
                        {
                            return controllerSymbol;
                        }

                        return null;
                    }).Where(m => m != null);


            IncrementalValuesProvider<MethodDeclarationSyntax> functionDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(predicate: (node, token) => node is MethodDeclarationSyntax,
                    (syntaxContext, token) => { return (MethodDeclarationSyntax)syntaxContext.Node; })
                .Where(m => m != null);

            var compilationAndControllers
                = context.CompilationProvider.Combine(controllerDeclarations
                    .Collect()).Combine(functionDeclarations
                    .Collect());

            context.RegisterSourceOutput(compilationAndControllers,
                (spc, source) => Execute(source.Left.Left, source.Left.Right, spc));
        }

        void Execute(Compilation compilation, ImmutableArray<INamedTypeSymbol> controllerSymbols,
            SourceProductionContext context)
        {
            if (controllerSymbols.IsDefaultOrEmpty)
            {
                return;
            }

            var controllerNames = new List<string>();
            foreach (var controllerSymbol in controllerSymbols)
            {
                controllerNames.Add($"{controllerSymbol.Name}");
            }

            context.AddSource("ControllerListController.Incremental.g.cs",
                FunctionTextProvider.GetFunctionText(controllerNames));
        }
    }
}