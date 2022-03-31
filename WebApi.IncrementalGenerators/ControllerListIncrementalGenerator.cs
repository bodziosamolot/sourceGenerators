using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WebApi.IncrementalGenerators
{
    [Generator]
    public class ControllerListIncrementalGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // if (!Debugger.IsAttached)
            // {
            // Debugger.Launch();
            // }

            IncrementalValuesProvider<ClassDeclarationSyntax> controllerDeclarations = context.SyntaxProvider
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
                        if (controllerSymbol != null)
                        {
                            return classDeclarationSyntax;
                        }

                        if (classDeclarationSyntax.Identifier.Value.ToString().EndsWith("Controller") &&
                            classDeclarationSyntax.BaseList.Types.Any(baseType =>
                                baseType.ToString().StartsWith("Controller")))
                        {
                            return classDeclarationSyntax;
                        }
                        else
                        {
                            return null;
                        }
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
                (spc, source) => Execute(source.Left.Left, source.Left.Right, source.Right, spc));
        }

        void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> controllers,
            ImmutableArray<MethodDeclarationSyntax> functions,
            SourceProductionContext context)
        {
            if (controllers.IsDefaultOrEmpty)
            {
                return;
            }

            var controllerNames = new List<string>();
            // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
            IEnumerable<ClassDeclarationSyntax> distinctControllers = controllers.Distinct();
            foreach (var classDeclarationSyntax in distinctControllers)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
                var controllerSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;

                controllerNames.Add($"{controllerSymbol.Name}");
            }

            context.AddSource("ControllerListController.Incremental.g.cs",
                FunctionTextProvider.GetFunctionText(controllerNames));
        }
    }
}