using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WebApi.IncrementalGenerators
{
    [Generator]
    public class ControllerListIncrementalGenerator : IIncrementalGenerator
    {
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

            IncrementalValuesProvider<MethodDeclarationSyntax> functionDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(predicate: (node, token) => node is MethodDeclarationSyntax,
                    (syntaxContext, token) =>
                    {
                        return (MethodDeclarationSyntax)syntaxContext.Node;
                    })
                .Where(m => m is not null);


            var compilationAndControllers
                = context.CompilationProvider.Combine(controllerDeclarations
                    .Collect()).Combine(functionDeclarations.Collect()); // TODO: What is this combine operation for?

            context.RegisterSourceOutput(compilationAndControllers,
                static (spc, source) => Execute(source.Left.Left, source.Left.Right, source.Right, spc));
        }

        static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> controllers,
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
                var controllerSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                controllerNames.Add($"\"{controllerSymbol.Name}\"");
            }

            var functionInformation = new List<FunctionInformation>();
            foreach (var methodDeclarationSyntax in functions)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(methodDeclarationSyntax.SyntaxTree);
                var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;

                var implementationFlagNames = string.Join(",", methodSymbol.MethodImplementationFlags);
                functionInformation.Add(new FunctionInformation(methodSymbol.Name, methodSymbol.ContainingType.Name, methodSymbol.ToString(), implementationFlagNames));
            }

            context.AddSource("ControllerListController.g.cs", FunctionTextProvider.GetFunctionText(controllerNames, functionInformation));
        }
    }
}