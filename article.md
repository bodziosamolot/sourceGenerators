# Source Generators

## Introduction

In simplest terms a Source Generator is a class that produces code based on other code. The result is available upon 
compilation. It may seem like magic because without creating any new *.cs files the developer can start using classes, 
extension methods, structs or whatever we decide our generator to create. This is because it includes the output in 
compilation artifacts. This new feature is highly configurable so You can easily set it up in such a way that appropriate 
files are persisted to the disk.

## Roslyn

How is all of this possible? Magic of Roslyn is the answer. This .NET Compiler provides a set of APIs allowing very 
powerful features like: code metrics, analyzers and source generators.

![Compiler pipeline from https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/compiler-api-model](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/media/compiler-api-model/compiler-pipeline-api.png)

The picture above shows the compiler pipeline and APIs corresponding to its phases. We will need some fundamental knowledge 
about how the compiler works in order to make use of source generators.

### Syntax Trees

At the most basic level we work with our code as a static text. This text is processed by the parser which produces a Syntax Tree. It is
a hierarchical representation of that text consisting of Syntax Nodes. It is best pictured with the following tools:

- [Syntax Tree Viewer in Rider](https://plugins.jetbrains.com/plugin/16356-syntax-visualizer-for-rider)
- Syntax Visualizer (with Directed Syntax Graph in Ultimate editions of Visual Studio)

Syntax visualisation of everyones favourite [WeatherForecastController](https://github.com/dotnet/AspNetCore.Docs/blob/main/aspnetcore/tutorials/first-web-api/samples/3.0/TodoApi/Controllers/WeatherForecastController.cs):
![Syntax Tree of everyones favourite WeatherForecastController](https://i.imgur.com/AU0veYl.png)

It shows basic building blocks of the tree:
- Node - 
- Token - 
- Trivia - 
- Value - 

## Definitions

- Compiler - 
- Parser - 
- Syntax Tree - 
- Syntax Node - 

### Sources

- https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/compiler-api-model
- https://joshvarty.com/2014/07/06/learn-roslyn-now-part-2-analyzing-syntax-trees-with-linq
- [Syntax vs Semantics](https://stackoverflow.com/questions/17930267/what-is-the-difference-between-syntax-and-semantics-in-programming-languages)