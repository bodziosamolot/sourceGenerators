In order to see the generated code files:

`<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)\GeneratedFiles</CompilerGeneratedFilesOutputPath>`

BaseIntermediateOutputPath - The top-level folder where all configuration-specific intermediate output folders are created. The default value is obj\. The following code is an example: <BaseIntermediateOutputPath>c:\xyz\obj\</BaseIntermediateOutputPath>

[Other MSBuild parameters](https://docs.microsoft.com/en-us/visualstudio/msbuild/common-msbuild-project-properties?view=vs-2022)

# To debug

- Rebuild the project which has the attached source generator
- Choose "csc"

# Roslyn
[Roslyn](https://github.com/dotnet/roslyn)

[Explanation of how compiler works](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/compiler-api-model)

[Syntax trees](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/work-with-syntax)

[Syntax Visualizer For Rider](https://plugins.jetbrains.com/plugin/16356-syntax-visualizer-for-rider)
but this one is better [Rossynt](https://plugins.jetbrains.com/plugin/16902-rossynt)

C# compiler with an API allowing code analysis.

## Syntax Model

[Explained](https://joshvarty.com/2014/07/06/learn-roslyn-now-part-2-analyzing-syntax-trees-with-linq/)

Syntax trees are immutable.

The main idea is that given a string containing C# code, the compiler creates a tree representation (called a syntax tree) of the string

Syntax nodes are one of the primary elements of syntax trees. These nodes represent syntactic constructs such as declarations, statements, clauses, and expressions. Each category of syntax nodes is represented by a separate class derived from SyntaxNode.

Syntax tokens are the terminals of the language grammar, representing the smallest syntactic fragments of the code. They are never parents of other nodes or tokens. Syntax tokens consist of keywords, identifiers, literals, and punctuation.

Syntax Tokens cannot be broken into simpler pieces. They are the atomic units that make up a C# program. They are the leaves of a syntax tree. They always have a parent Syntax Node (as their parent cannot be a Syntax Token).

Syntax Nodes, on the other hand, are combinations of other Syntax Nodes and Syntax Tokens. They can always be broken into smaller pieces. In my experience, you’re most interested in Syntax Nodes when trying to reason about a syntax tree.

The four primary building blocks of syntax trees are:
- The Microsoft.CodeAnalysis.SyntaxTree class, an instance of which represents an entire parse tree. SyntaxTree is an abstract class that has language-specific derivatives. You use the parse methods of the Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree (or Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree) class to parse text in C# (or Visual Basic).
- The Microsoft.CodeAnalysis.SyntaxNode class, instances of which represent syntactic constructs such as declarations, statements, clauses, and expressions.
- The Microsoft.CodeAnalysis.SyntaxToken structure, which represents an individual keyword, identifier, operator, or punctuation.
- And lastly the Microsoft.CodeAnalysis.SyntaxTrivia structure, which represents syntactically insignificant bits of information such as the white space between tokens, preprocessing directives, and comments.

It may have sense to analyse syntax in isolation. Semantic analysis only has sense in a context (a program or an assembly) and therefore can provide more information.

Use the syntax model to find the structure of the code; you use the semantic model to understand its meaning

## Semantic Model

A compilation allows you to find Symbols - entities such as types, namespaces, members, and variables which names and other expressions refer to. The process of associating names and expressions with Symbols is called Binding.

The Syntax API allows you to look at the structure of a program. However, often you want richer information about the semantics or meaning of a program.

Querying the semantic model is typically more expensive than querying syntax trees. This is because requesting a semantic model often triggers a compilation.

There are 3 different ways to request the semantic model:

1. Document.GetSemanticModel()
2. Compilation.GetSemanticModel(SyntaxTree)
3. Various Diagnostic AnalysisContexts including CodeBlockStartAnalysisContext.SemanticModel and SemanticModelAnalysisContext.SemanticModel

The semantic model is our bridge between the world of syntax and the world of symbols.

[What is a compilation?](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/get-started/semantic-analysis#understanding-compilations-and-symbols)

## Symbols

Before continuing, it’s worth taking a moment to discuss Symbols.

C# programs are comprised of unique elements, such as types, methods, properties and so on. Symbols represent most everything the compiler knows about each of these unique elements.

At a high level, every symbol contains information about:

Where this elements is declared in source or metadata (It may have come from an external assembly)
What namespace and type this symbol exists within
Various truths about the symbol being abstract, static, sealed etc.
More information may be found in ISymbol.
Other, more context-dependent information may also be uncovered. When dealing with methods, IMethodSymbol allows us to determine:

Whether the method hides a base method.
The symbol representing the return type of the method.
The extension method from which this symbol was reduced.

# Rider versions

- 2022.1 EAP 5 - couldn't make the generated files be included in the build
- 2021.3.3 - works fine

# Links 

- [Source generators vs Incremental Source Generators by Andrew Locke](https://andrewlock.net/exploring-dotnet-6-part-9-source-generator-updates-incremental-generators/)
- [Smart Enums](https://www.thinktecture.com/en/net/roslyn-source-generators-introduction/)
- [Incremental Source Generators documentation that comes with Roslyn](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md)
- [Syntax Tree and Symbol](https://medium.com/@dullohan/the-roslyn-compiler-relating-the-syntax-tree-to-symbols-949eeed59a30)
- [Nick Chapsas - Build Duration](https://youtu.be/anesVdQg6Dk)
- [What version of the framework must be targeted by generator project](https://github.com/dotnet/roslyn/issues/49249#issuecomment-782516845)
- [Syntax tree per file](https://www.codingzee.com/2021/04/c-code-analysis-with-roslyn-syntax-trees.html)
- [Metadata import and Symbols](https://www.syncfusion.com/succinctly-free-ebooks/roslyn/walking-through-roslyn-architecture-apis-syntax)

# Laws of source generators (from [Roslyn Source Generators Never send a human to do a machine's job - Stefan Pölz](https://youtu.be/lJCfPhnFLQs?t=592))

- retrieve representation of user code
  - syntax trees
  - semantic model
- can only add code
- can produce diagnostics
- products of one source generator are not visible to the other generators

SyntaxReceiver is called on each key press ([explained](https://youtu.be/lJCfPhnFLQs?t=1186))

# Shipped with .NET 6

- System.Text.Json ([Explained](https://youtu.be/lJCfPhnFLQs?t=813))
- Microsoft.Extensions.Logging.Abstractions ([Explained](https://youtu.be/lJCfPhnFLQs?t=699))

# Remarks

- don't make the generator project reference other projects because it causes [an issue](https://github.com/dotnet/roslyn/issues/52017)

# Source Generators

The three stages:
- filter: for syntax we are interested in
- transformation: transform syntax into semantic model (a symbol) using compilation, can provide custom data structure containing whaever is needed for source generation
- generate the code using rich information from the semantic model

# Source Generators vs Incremental Source Generators

[Comparison](https://youtu.be/lJCfPhnFLQs?t=2725)