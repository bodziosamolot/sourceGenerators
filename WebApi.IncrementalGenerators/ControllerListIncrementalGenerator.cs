using System.Collections.Immutable;
using System.Diagnostics;
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
            Debugger.Launch(); 
            
            IncrementalValuesProvider<INamedTypeSymbol> controllerDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    (node, token) => node is ClassDeclarationSyntax && ((ClassDeclarationSyntax)node)
                        .Identifier.Value.ToString().EndsWith("Controller"),
                    (syntaxContext, token) =>
                    {
                        var classDeclarationSyntax = syntaxContext.Node as ClassDeclarationSyntax;
                        var controllerSymbol =
                            syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
                        if (controllerSymbol != null && controllerSymbol.BaseType.Name == nameof(ControllerBase))
                        {
                            return controllerSymbol;
                        }

                        return null;
                    }).Where(m => m != null);


            var compilationAndControllers
                = context.CompilationProvider.Combine(controllerDeclarations
                    .Collect());

            context.RegisterSourceOutput(compilationAndControllers,
                (spc, source) => Execute(source.Left, source.Right, spc));
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