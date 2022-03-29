using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace WebApi.IncrementalGenerators
{
    [Generator]
    public class ControllerListIncrementalGenerator : IIncrementalGenerator
    {
        private static IList<string> _logs = new List<string>();

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            _logs.Add("Initialize() called");
            if (!Debugger.IsAttached)
            {
                // Debugger.Launch();
            }

            // 1. Filter for syntax we are interested in
            IncrementalValuesProvider<ClassDeclarationSyntax> controllerDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, token) =>
                    {
                        var result = node is ClassDeclarationSyntax && ((ClassDeclarationSyntax)node)
                            .Identifier.Value.ToString().EndsWith("Controller");

                        _logs.Add($"{Environment.NewLine} ---------------------------------------------------------");
                        _logs.Add($"{Environment.NewLine} [] predicate called on {CSharpExtensions.Kind(node)}:");
                        _logs.Add($"{Environment.NewLine}{node.GetText()}");
                        _logs.Add($"{Environment.NewLine} ----------------- returned: {result}---------------------");


                        return result;
                        // return true;
                    },
                    (syntaxContext, token) =>
                    {
                        _logs.Add("transform called");
                        var classDeclarationSyntax = syntaxContext.Node as ClassDeclarationSyntax;
                        if (classDeclarationSyntax is null)
                        {
                            return null;
                        }

                        // SemanticModel -  Allows asking semantic questions about a tree of syntax nodes in a Compilation
                        // Why can't i get the symbol from syntaxContext.SemanticModel.GetSymbolInfo()

                        // var controllerSymbol = syntaxContext.SemanticModel.GetSymbolInfo(classDeclarationSyntax);
                        var controllerSymbol =
                            syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
                        // What is the SemanticModel in the syntaxContext (GeneratorSyntaxContext)??

                        // SemanticModel provides semantic information. How is it doing that if it does not have access to the compilation? 
                        // It has access to compilation. IncrementalValuesProvider 
                        if (controllerSymbol != null)
                        {
                            _logs.Add(
                                $"{Environment.NewLine} ---------------------------------------------------------");
                            _logs.Add($"{Environment.NewLine} [] transform returned {controllerSymbol.Name}:");
                            _logs.Add(
                                $"{Environment.NewLine} ---------------------------------------------------------");
                            return classDeclarationSyntax;
                        }

                        if (classDeclarationSyntax.Identifier.Value.ToString().EndsWith("Controller") &&
                            classDeclarationSyntax.BaseList.Types.Any(baseType =>
                                baseType.ToString().StartsWith("Controller")))
                        {
                            _logs.Add(
                                $"{Environment.NewLine} ---------------------------------------------------------");
                            _logs.Add($"{Environment.NewLine} [] transform returned {controllerSymbol.Name}:");
                            _logs.Add(
                                $"{Environment.NewLine} ---------------------------------------------------------");
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
                    .Collect()); // Combine seems to be the only way to get to the Compilation
            // in contrast to how in basic Generator, Compilation is
            // available directly on the context. Or maybe it is not? Maybe it is enough to
            // Use compilation with the filtered syntax? TRY DOING THIS!

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

                // context.ReportDiagnostic(Diagnostic.Create(
                //                 new DiagnosticDescriptor(
                //                     "TestId",
                //                     "Non-void method return type",
                //                     "Method {0} returns {1}. All methods must return void.",
                //                     "yeet",
                //                     DiagnosticSeverity.Warning,
                //                     true), controllerSymbol.Locations.FirstOrDefault(), controllerSymbol.Name, controllerSymbol.Arity));

                controllerNames.Add($"{controllerSymbol.Name}");
            }

            var functionInformation = new List<FunctionInformation>();
            foreach (var methodDeclarationSyntax in functions)
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(methodDeclarationSyntax.SyntaxTree);
                var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;

                var implementationFlagNames = string.Join(",", methodSymbol.MethodImplementationFlags);
                functionInformation.Add(new FunctionInformation(methodSymbol.Name, methodSymbol.ContainingType.Name,
                    methodSymbol.ToString(), implementationFlagNames));
            }

            // Task.Delay(6000).ConfigureAwait(false).GetAwaiter().GetResult();
            context.AddSource("ControllerListController.Incremental.g.cs",
                FunctionTextProvider.GetFunctionText(controllerNames, functionInformation));


            // CreateLog(context);
        }

        private void CreateLog(SourceProductionContext context)
        {
            var timestamp =
                $"{DateTimeOffset.UtcNow.Date.ToShortDateString()}_{DateTime.UtcNow.ToLongTimeString().Replace(":", "_")}";
            context.AddSource($"log_{timestamp}.txt", string.Join(Environment.NewLine, _logs));
        }
    }
}