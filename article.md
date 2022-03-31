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

At the most basic level we work with our code as a static text. This text is processed by the parser which produces a Syntax Tree. Referring 
to the Compiler Pipeline illustration it corresponds to the "Parser" box. A Syntax Tree is
a hierarchical representation of text consisting of Syntax Nodes. It is best pictured with the following tools:

- [Syntax Tree Viewer in Rider](https://plugins.jetbrains.com/plugin/16356-syntax-visualizer-for-rider)
- Syntax Visualizer (with Directed Syntax Graph in Ultimate editions of Visual Studio)

Syntax visualisation of everyones favourite [WeatherForecastController](https://github.com/dotnet/AspNetCore.Docs/blob/main/aspnetcore/tutorials/first-web-api/samples/3.0/TodoApi/Controllers/WeatherForecastController.cs):
![Syntax Tree of everyones favourite WeatherForecastController](https://i.imgur.com/AU0veYl.png)

It shows basic building blocks of the tree:
- Node - Building blocks of the syntax tree consisting of combination of tokens, trivia and other nodes
- Token - Leaves of the syntax tree. These are elements like keywords or identifiers
- Trivia - Parts of syntax with really low significance like whitespace or comments
- Value - Some tokens store the characters they consist of in a separate field called Value 

Syntax trees are used in what is called *syntax analysis*. You could compare a syntax tree to a diagram of code in one source file. It can be 
useful but You are missing the context. In order to get more information we need to get to *semantic analysis*.

### Compilation and Semantic Analysis 

The key to obtaining semantic information about our code is the Compilation. With it we see our constructs not in isolation like in the case of 
Syntax Trees but in a broader context. This context can be imagined as a compilation unit: an assembly or a project in our solution. So in other 
words: Compilation can be understood as a bunch of Syntax Trees stuck together allowing to get more information. This is the Semantic Model. It
allows us to get information through Symbols. 

## Types of Source Generators

### Regular Source Generators

## When does the Generator run?

Source generators execute with pretty much every keystroke. In order to observe that behavior You can use my sample code. It has a static
method in the IncrementalMetadataController that uses the name of a random controller from the project. Whenever You change the name 
the function name will also change immediately. There is an easy to make mistake here. If the generator does not target the 
"netstandard2.0" it may not work as expected. It's important to highlight the fact that this requirement concerns the Generator project,
not the project using the generator. 

Keeping the generated code in sync with source code requires a lot of processing on the Generator part. This creates an obvious problem
for the IDE. The more work the generator has to do, the more noticable it is in the IDE. The generator code benefits from filtering of
the syntax tree to limit the amount of work. This became part of the contract in the new type of Generator introduced in .NET 6.0.

### Incremental Source Generator

?? When does an incremental source generator run?

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