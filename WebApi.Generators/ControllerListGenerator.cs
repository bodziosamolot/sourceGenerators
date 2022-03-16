using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WebApi.Generators
{
    [Generator]
    public class ControllerListGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            Debugger.Launch();
        
            var controllersSyntax = GetAllClasses(context.Compilation)
                .Where(classNode => classNode.Identifier.Text.EndsWith("Controller"));

            var controllerNames = controllersSyntax.Select(controllerSyntax =>
            {
                var controllerSemanticModel = context.Compilation.GetSemanticModel(controllerSyntax.SyntaxTree);
                var controllerSymbol = controllerSemanticModel.GetDeclaredSymbol(controllerSyntax) as IMethodSymbol;
                var implementationFlagNames = string.Join(",", controllerSymbol.MethodImplementationFlags);
                return controllerSymbol.Name;
            });

            // Build up the source code
            string source = FunctionTextProvider.GetFunctionText(controllerNames);

            // Add the source code to the compilation
            context.AddSource($"ControllerListController.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        private IEnumerable<ClassDeclarationSyntax> GetAllClasses(Compilation compilation)
        {
            IEnumerable<SyntaxNode> allNodes = compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
            return allNodes
                .Where(d => d.IsKind(SyntaxKind.ClassDeclaration))
                .OfType<ClassDeclarationSyntax>();
        }
    }
}