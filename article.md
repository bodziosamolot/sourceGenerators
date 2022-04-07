# Questions

- When does the incremental source generator run? 
  - Is it at every keystroke? 
  - How to verify it?
  - It is supposed to run with compilation but i can't see the output files chage when changing names in code
    - maybe it is because the output files emitted to disk only change on build? The Source incorporated in the compilation may change more often?
- When does the source generator run?

# Source Generators

## Introduction

In simplest terms a Source Generator is a class that produces code based on other code. The result is available upon 
compilation. It may seem like magic because without creating any new *.cs files the developer can start using classes, 
extension methods, structs or whatever we decide our generator to create. This is because it includes the output in 
compilation artifacts. 

## Compilation and Build process 

We will be refering to the compilation a lot. It is important not to confuse it with a build. Build can
be understood as creation of an executable. In order to build a .NET 
executable or assembly we must use a specific tool. Most often it is MSBuild. What it does is it runs the 
compiler providing it with all the inputs it requires like referenced assemblies, source files, etc. Language specific compiler 
produces Intermediate Language out of the source code and is one of the steps in the build process. Compilation is lighter than build
and is just one of the steps. This is good because we need compilation to be executed often if we want to use a feature like 
source generation.

## Roslyn

How is all of this possible? Magic of Roslyn is the answer. This .NET Compiler provides a set of APIs allowing very 
powerful features like: code metrics, analyzers and source generators.

![Compiler pipeline from https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/compiler-api-model](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/media/compiler-api-model/compiler-pipeline-api.png)

The picture above shows the compiler pipeline and APIs corresponding to its phases. We will need some fundamental knowledge 
about how the compiler works in order to make use of source generators.

### Syntax Trees and Syntax Analysis

At the most basic level we work with our code as static text. This text is processed by the a parser which produces Syntax Trees. Plural because 
each source file corresponds to a separate Syntax Tree. Referring 
to the Compiler Pipeline illustration it corresponds to the "Parser" box. A Syntax Tree is
a hierarchical representation of text consisting of Syntax Nodes. It is best pictured with the following tools:

- [Syntax Tree Viewer in Rider](https://plugins.jetbrains.com/plugin/16356-syntax-visualizer-for-rider)
- Syntax Visualizer (with Directed Syntax Graph in Ultimate editions of Visual Studio)

Syntax visualisation of everyones favourite [WeatherForecastController](https://github.com/dotnet/AspNetCore.Docs/blob/main/aspnetcore/tutorials/first-web-api/samples/3.0/TodoApi/Controllers/WeatherForecastController.cs):
![Syntax Tree of everyones favourite WeatherForecastController](https://i.imgur.com/AU0veYl.png)

It shows elements out of which the Tree is constructed:
- Node - Basic building blocks of the syntax tree consisting of combination of tokens, trivia and other nodes.
- Token - Leaves of the syntax tree. These are elements like keywords or identifiers.
- Trivia - Parts of syntax with really low significance like whitespace or comments.
- Value - Some tokens store the characters they consist of in a separate field called Value.

Syntax trees are used in what is called *syntax analysis*. You could compare a syntax tree to a diagram of code in one source file. It can be 
useful but You are missing the context. In order to get more information we need to get to *semantic analysis*.

### Compilation and Semantic Analysis 

Next up in the compilation pipeline there are two separate boxes: Symbols and Metadata Import. The metadata allows the formation of Symbols. 
They are the key to obtaining semantic information about our code from the Compilation. Why is the metadata required? Some elements are imported into our program
from assemblies. Metadata allows to get information about those *foreign* objects. There are various types of Symbols. To illustrate what we can learn from a symbol
lets use an example. [This](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.inamedtypesymbol?view=roslyn-dotnet-4.1.0) is the documentation for
INamedTypeSymbol. We can learn about such properties as:
- Arity of the type,
- List of its constructors,
- List of interfaces this type implements,
- If it is static.

This is just a minor part of all things we can learn about the associated code element. With semantic analysis see our constructs not in isolation like in the case of Syntax Trees 
but in a broader context. This context can be imagined as a compilation unit: an assembly or a project in our solution. So in other 
words: Compilation can be understood as a bunch of Syntax Trees stuck together with added metadata.

## Analyzers

Source generators are the topic of this article. If we want to build a knowledge base to work with them it is worth mentioning the mechanism they are derived from. Namely: analyzers. 
They use the same concepts of Syntax Trees and Compilation to inspect the code. They allow to report Diagnostics through the user of [DiagnosticAnalyzer](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.diagnostics.diagnosticanalyzer?view=roslyn-dotnet-4.1.0).
Diagnostics are those very helpful squiggles that we get in our IDE everytime we do something fishy. The other helpful feature of the IDE enabled by Analyzers are Code Fixes.
They are implemented with [CodeFixProvider](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.codefixes.codefixprovider?view=roslyn-dotnet-4.1.0) which allows us to get
useful suggestions on how to fix problems.

## Types of Source Generators

### Regular Source Generators

In this article we are focusing on Incremental Source Generators. The name has "Incremental" in it. Are there Non-Incremental Source Generators?
Yes, there are. They were introduced in .NET 5 but there was a problem with them. All of that processing that happens with each compilation occurs
very often. Pretty much with every keystroke. This caused the developer experience in the IDE to deteriorate badly. It could be improved by aggressively 
filtering the syntax processed by our generator but still was not good enough. The mechanism could be improved by introduction of caching and 
including the filtration in its contract.

### Incremental Source Generator

That caching is realized through IncrementalValueProvider<T> (and its sibling IncrementalValuesProvider<T>). When working with a generator we will have 
access to IncrementalGeneratorInitializationContext which allows to get the following providers:
- CompilationProvider
- AdditionalTextsProvider
- AnalyzerConfigOptionsProvider
- MetadataReferencesProvider
- ParseOptionsProvider

It is best explained with an example. I will skim over some important parts of an Incremental Source Generator to get to the vital parts. 
The Generator has to implement the [IIncrementalGenerator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.iincrementalgenerator?view=roslyn-dotnet-4.1.0)
interface. The interface consists of only one method:

`Initialize(IncrementalGeneratorInitializationContext)`

This *IncrementalGeneratorInitializationContext* is what gives us access to all the providers mentioned before. 

All of which are utilizing IValueProvider<TSource> e.g., CompilationProvider is IncrementalValueProvider<Compilation>. It realises filtering and transformations
through a set of operators similar to LINQ:
- Select
- SelectMany
- Where
- Collect
- Combine

The provider hides all of the implementation details related with caching. What's important for us is that the provider operators run only for changes. Most of the
operators listed before are pretty obvious apart from two specific ones.

#### Collect

Can be thought of as similar to materializing operators from LINQ. It allows to obtain ImmutableArray<T> from the provider instead of just individual items one by one.

#### Combine

Like the name says it allows to create a conjunction of two providers. The result will be series of tuples containing values from both providers.

[With each keystroke a new Syntax Tree is produced for the edited part of code. If there is a new Syntax Tree this 
means the Generator has to execute again. What differentiates the Incremental Source Generator from a regular 
one is that it has the predicate phase. Predicate is executed for the changes to get the nodes we are interested in. 
We already know the nodes the predicate selected for changes in other syntax trees so those don't need to be 
analyzed again. This caching saves a lot of time and improves the IDE experience. The Incremental Generator
uses the compilation and transforms]

### When does the Generator run?

Source generators execute with pretty much every keystroke. In order to observe that behavior You can use my sample code. It has a static
method in the IncrementalMetadataController that uses the name of a random controller from the project. Whenever You change the name
the function name will also change immediately. There is an easy to make mistake here. If the generator does not target the
"netstandard2.0" it may not work as expected. It's important to highlight the fact that this requirement concerns the Generator project,
not the project using the generator.

Keeping the generated code in sync with source code requires a lot of processing on the Generator part. This creates an obvious problem
for the IDE. The more work the generator has to do, the more noticable it is in the IDE. The generator code benefits from filtering of
the syntax tree to limit the amount of work. This became part of the contract in the new type of Generator introduced in .NET 6.0.

## Definitions

- Compiler - Takes code in, passes it through a pipeline and produces a  
- Roslyn - 
- Parser - Part of the compiler responsible for building the Syntax Tree from linear text (???)
- Syntax Tree - 
- Syntax Node - 
- Compilation - 
- Symbol - 
- Intermediate Language - 
- Compiler - Turns source code into Intermediate Language
- JIT Compiler - Turns Intermediate Language into machine code that can be executed

### Sources

- https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/compiler-api-model
- https://joshvarty.com/2014/07/06/learn-roslyn-now-part-2-analyzing-syntax-trees-with-linq
- [Syntax vs Semantics](https://stackoverflow.com/questions/17930267/what-is-the-difference-between-syntax-and-semantics-in-programming-languages)
- [Incremental Source Generators documentation that comes with Roslyn](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md)