# Source Generators

## Introduction

In simple terms a source generator is a class that produces code based on other code. The result is available upon 
compilation. It may seem like magic because without creating any new *.cs files the developer can start using classes, 
extension methods, structs or whatever we decide our Generator to create. This is because it includes the output in 
compilation artifacts. There is a lot the developer has to know about what the compiler is; how it sees and processes
the code we feed to it. Understanding of those aspects if crucial to work with Code Generators.

In this article I want to provide everything required to write a simple Incremental source generator. You will learn
about Roslyn, what differentiates source generators from incremental source generators and finally we will build a Generator.

## Compilation and Build process 

We will be referring to the compilation a lot. It is important not to confuse it with a build. Build can
be understood as creation of an executable. In order to build a .NET 
executable or assembly we must use a specific tool. Most often it is MSBuild. What it does is it runs the 
compiler providing it with all the inputs it requires like referenced assemblies, source files, etc. Language specific compiler 
produces Intermediate Language out of the source code and is one of the steps in the build process. Compilation is lighter than build
and is just one of its components. This is good because we need compilation to be executed often if we want to use the Compiler API and 
its richness.

## Roslyn

Roslyn is the name used for .NET Compiler. It is open source and includes versions for C# and Visual Basic. Roslyn exposes various types of APIs:
- Compiler APIs - Corresponding to phases of the Compiler Pipeline. We will use mostly those api for our Generator.
- Diagnostic APIs - If You see colored squiggles in Your IDE that's thanks to the Diagnostic API.
- Scripting APIs - Allow to use C# as a scripting language.
- Workspace APIs - Allow to work with how our program is structured i.e. Solution, Project.

![Compiler pipeline from https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/compiler-api-model](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/media/compiler-api-model/compiler-pipeline-api.png)

The picture above shows the Compiler Pipeline and APIs corresponding to its phases. We will need some fundamental knowledge 
about how the Compiler works in order to make use of source generators.

### Syntax Trees and Syntax Analysis

At the most basic level we work with our code as static text. This text is processed by the a parser which produces syntax trees. Plural because 
each source file corresponds to a separate syntax tree. Referring 
to the Compiler Pipeline illustration it corresponds to the "Parser" box. A syntax tree is
a hierarchical representation of text consisting of Syntax Nodes. It is best pictured with the following tools:

- [Syntax Tree Viewer in Rider](https://plugins.jetbrains.com/plugin/16356-syntax-visualizer-for-rider)
- Syntax Visualizer (with Directed Syntax Graph in Ultimate editions of Visual Studio)

Syntax visualisation of everyone's favourite [WeatherForecastController](https://github.com/dotnet/AspNetCore.Docs/blob/main/aspnetcore/tutorials/first-web-api/samples/3.0/TodoApi/Controllers/WeatherForecastController.cs):

![Syntax Tree of everyones favourite WeatherForecastController](https://i.imgur.com/AU0veYl.png)

It shows elements out of which the Tree is constructed:
- Nodes - Basic building blocks of the syntax tree consisting of combination of tokens, trivia and other nodes.
- Tokens - Leaves of the syntax tree. These are elements like keywords or identifiers.
- Trivia - Parts of syntax with really low significance like whitespace or comments.
- Values - Some tokens store the characters they consist of in a separate field called Value.

Syntax trees are used in what is called *syntax analysis*. You could compare a syntax tree to a diagram of code in one source file. Lets assume this file has a definition of a class. 
Syntax analysis can tell us a lot about that class but we won't learn about how it is used in the broader context of the entire program. In order to get that kind of information we need  
*semantic analysis*.

### Compilation and Semantic Analysis 

Next up in the compilation pipeline there are two separate boxes: Symbols and Metadata Import. The metadata allows the formation of Symbols. 
They are the key to obtaining semantic information about our code from the compilation. Why is the metadata required? Some elements are imported into our program
from assemblies. Metadata allows to get information about those *foreign* objects. There are various types of Symbols. To illustrate what we can learn from a symbol
lets use an example. [This](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.inamedtypesymbol?view=roslyn-dotnet-4.1.0) is the documentation for
INamedTypeSymbol. We can learn about such properties as:
- Arity of the type,
- List of its constructors,
- List of interfaces this type implements,
- If it is static.

This is just a minor part of all the information we can get about the associated code element. With semantic analysis we see our constructs not in isolation like in the case of syntax trees 
but in a broader landscape. This context can be imagined as a compilation unit: an assembly or a project in our solution. So in other 
words: compilation can be understood as a bunch of syntax trees stuck together with added metadata.

## Analyzers

Source generators are the topic of this article. If we want to build a knowledge base to work with them it is worth mentioning the mechanism they are derived from. 
Namely: analyzers. They use the same concepts of syntax trees and compilation to inspect the code. 
They allow to report Diagnostics through the use of [DiagnosticAnalyzer](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.diagnostics.diagnosticanalyzer?view=roslyn-dotnet-4.1.0).
Diagnostics are those very helpful squiggles that we get in our IDE everytime we do something fishy. The other helpful feature of the IDE enabled by Analyzers are Code Fixes.
They are implemented with [CodeFixProvider](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.codefixes.codefixprovider?view=roslyn-dotnet-4.1.0) which allows us to get
useful suggestions on how to fix problems. A source generator is an unusual Analyzer which apart from inspecting code, produces it based on the results of that inspection.

## Types of Source Generators

### Regular Source Generators

In this article we are focusing on incremental source generators. You may be wondering if there are non-incremental source generators then? Yes, there are!
They were introduced in .NET 5 but there was a problem. All of that processing that happens with each compilation occurs
very often. Pretty much with every keystroke. This caused the developer experience in the IDE to deteriorate badly. It could be improved by aggressively 
filtering the syntax processed by our generator but still was not good enough. The mechanism could be improved by introduction of caching and 
including the filtration in its contract. Let's see what are the improvements introduced in the next iteration of source generators.

### Incremental Source Generator

The requirement of caching is realized through IncrementalValueProvider<T> (and its sibling IncrementalValuesProvider<T>). When working with a generator we will have 
access to IncrementalGeneratorInitializationContext which allows to get the to a set of providers. They are the points through which we can work with different
components of our program:
- SyntaxProvider - this provider will serve us changes in the syntax of our program.
- CompilationProvider - is the gateway to Semantic Analysis.
- AdditionalTextsProvider - allows to obtain files with static content included in the project.
- MetadataReferencesProvider - providers information about referenced assemblies.
- AnalyzerConfigOptionsProvider & ParseOptionsProvider - allows to read configuration values.

All of which are utilizing IValueProvider<TSource> e.g., CompilationProvider is IncrementalValueProvider<Compilation>.
The provider hides all of the implementation details related with caching. What's important for us is that the provider operators run only for changes.

#### Let's write our own Incremental Source Generator

Best way to learn something is to create it on Your own. I will skim over some important parts of an incremental source Generator to get to the vital ones first.
The Generator has to implement the [IIncrementalGenerator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.iincrementalgenerator?view=roslyn-dotnet-4.1.0)
interface. The interface consists of only one method:

`Initialize(IncrementalGeneratorInitializationContext)`

This *IncrementalGeneratorInitializationContext* is what gives us access to all the providers mentioned before. It is worth mentioning that the implementation of incremental source generator
in this article is a functional one but it distilled so that we can focus on the most important things. It lacks some checks and operations You would normally add. 

          public void Initialize(IncrementalGeneratorInitializationContext context)
          {
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

                ...
          }

What the code above does is:
- It uses the `context.SyntaxProvider.CreateSyntaxProvider()` to construct the filtering pipeline. It consists of two lambda functions:
  - the first one is called the *predicate* and is the first level of filtration which processes the syntax,
  - the second one is called the *transform* and is used to obtain semantic information from the syntax that got through the predicate
- In our generator the *predicate* looks through the syntax for nodes which represent a class whose name ends with "Controller"
- The *transform* step uses the node to obtain the semantic information and check if the base of the class we are checking is of the *ControllerBase* type
- The `context.SyntaxProvider.CreateSyntaxProvider()` returns the `IncrementalValuesProvider<INamedTypeSymbol>` which we already know does all of the caching magic.

It's important to underline that splitting the process into the predicate and transform is a window optimisation. It should do a lightweight check to quickly filter the incoming
syntax. If the work it does is time consuming the experience in the IDE will quickly become unbearable.

The code ends with a `Where(m => m != null)`. This is not a LINQ operator. It behaves in a similar way but it is an *IValueProvider* extension method.
There are other similar ones:
- Select
- SelectMany
- Where
- Collect
- Combine
all of which are described in more detail in the following [document](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md#select)

Collect and combine don't have their counterparts in LINQ.

#### Collect

Can be thought of as similar to materializing operators from LINQ. If we use it we will get a collection of all items being processed by the provider instead of obtaining them one by one.
It will be represented as ImmutableArray<T>.

#### Combine

Like the name says it allows to create a conjunction of two providers. The result will be a series of tuples containing values from both providers.
It behaves different based on various multiplicity scenarios of it's arguments:
- collection & collection, 
- single item & single item
- collection & single item

In our case we will use this operator with CompilationProvider which has a single value of a Compilation. We will Combine it with the result of calling the 
Collect operator on `IncrementalValuesProvider<INamedTypeSymbol>` which holds the symbols of Controllers we found earlier. This operator is the way to access a 
Compilation object from IncrementalValuesProvider:

            var compilationAndControllers
                = context.CompilationProvider.Combine(controllerDeclarations
                    .Collect());

After adding those elements our generator will look like this:

    [Generator]
    public class ControllerListIncrementalGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
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

        ...
    }

The details of how the code is generated are hidden in the Execute method. What is important to underline is the fact that the code we see
in this Initialize() method only deals with *registering* the pipeline. All of those registered lambdas will execute whenever the context provides relevant changes in
the syntax. All of those changes will be passed to the function used in RegisterSourceOutput. In our case it passed processing further to the Execute method:

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

After some examination You will see that in case of our simple generator the Compilation is not utilized in the Execute method. A lot of more advanced examples use the Compilation in this 
final step to obtain additional information. I've used this pattern as an opportunity to explain how the Combine and Collect operators work.

The most relevant part here is the SourceProductionContext and its AddSource() method. The method accepts the name of the output file and the template to inject
values into. The template is nothing sophisticated in case of our example. It is just a class with a static method that provides the text and placeholders
for values provided from the generator:

    public static string GetFunctionText(IEnumerable<string> controllerNames)
    {
        var sourceBuilder = new StringBuilder($@"using Microsoft.AspNetCore.Mvc;

            namespace WebApi.Controllers;

            [ApiController]
            [Route(""[controller]"")]
            public class IncrementalMetadataController : ControllerBase
            {{
                private readonly ILogger<IncrementalMetadataController> _logger;

                public IncrementalMetadataController(ILogger<IncrementalMetadataController> logger)
                {{
                    _logger = logger;
                }}

                [HttpGet(""incremental/controllers"", Name = ""GetControllerNamesIncremental"")]
                public IEnumerable<string> GetControllerNames()
                {{
                   return new List<string>() {{{string.Join(",", controllerNames.Select(x=>$"\"{x}\""))}}};
                }}

                public static void {controllerNames.First()}()
                {{
                    Console.WriteLine(""This is a test"");
                }}
            }}");
        return sourceBuilder.ToString();
    }

By looking at the template we can easily come up with what the generator actually does: it provides a very useful functionality of 
listing the controllers defined in our ASP.NET application. That shows that most of the work done in the generator is extracting the values
to combine with the template.

## Generators at work

How do we know if our generator works? All You need to do is execute the app after cloning it from the [repo](https://github.com/bodziosamolot/sourceGenerators).
You will notice that nowhere does it define the IncrementalMetadataController but after running it and visiting the `https://localhost:7259/IncrementalMetadata/incremental/controllers`
address You will get a response listing all of the controllers defined.

There is also a different way of verifying what was produced:

      <Project Sdk="Microsoft.NET.Sdk.Web">

          <PropertyGroup>
              <TargetFramework>net6.0</TargetFramework>
              ... 
              <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
              <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)\GeneratedFiles</CompilerGeneratedFilesOutputPath>
          </PropertyGroup>

          ...

          <ItemGroup>
              <ProjectReference Include="..\WebApi.IncrementalGenerators\WebApi.IncrementalGenerators.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
          </ItemGroup>

      </Project>

The EmitCompilerGeneratedFiles and CompilerGeneratedFilesOutputPath properties allow to save the generated code to disk. 
There is a caveat: the generator works pretty much on every keystroke but the files are saved only on build. To observe that behaviour 
I've added a static method in the controller that has the name of the first Controller in our app. If You change the name of the `DummyController` the
name of the static method on the IncrementalMetadataController should update immediately in the IDE but not on disk. It will synchronise on disk only
after a build. I've noticed some irregularities in how this mechanism is acting so I wouldn't rely on it. I was surprised that although breaking from time to time 
it worked better in Rider (version 2021.3.3) than in Visual Studio 2022 Community (version 17.1.3).

# Debugging the Source Generator

Unfortunately the code that we write does not always produce the results we have expected. How can we debug a source generator? It is a bit 
awkward. In order to break on Generator execution You need to add the following line to it:

`Debugger.Launch()`

When the Generator executes You will be presented with a prompt with the choice of IDE's to use for the debugging session:

![JIT Debugger Prompt](https://i.imgur.com/lEseLeh.png)

When using Rider, make sure You have the correct Debugger option selected:

![Rider JIT Debugger settings](https://i.imgur.com/9oaw7X6.png)

# Summary

There are more and more developers contributing their source generators to .NET ecosystem through open-source. The [list of Source Generators](https://github.com/amis92/csharp-source-generators) 
to use is growing. I hope that after reading this article You will have enough information and resources to make use of this fantastic new tool and maybe add a new position to this list.

# Sources

- [Roslyn documentation](https://github.com/dotnet/roslyn)
- [Syntax trees from Microsoft Roslyn SDK documentation](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/work-with-syntax)
- [Syntax Visualizer For Rider](https://plugins.jetbrains.com/plugin/16356-syntax-visualizer-for-rider)
- [Blog post about Syntax Analysis](https://joshvarty.com/2014/07/06/learn-roslyn-now-part-2-analyzing-syntax-trees-with-linq/)
- [Description of Roslyn APIs](https://www.syncfusion.com/succinctly-free-ebooks/roslyn/walking-through-roslyn-architecture-apis-syntax)
- [What is a compilation?](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/get-started/semantic-analysis#understanding-compilations-and-symbols)
- [Part of Roslyn documentation about Source Generators](https://github.com/dotnet/roslyn/blob/main/docs/features/generators.md)
- [Part of Roslyn documentation about Incremental Source Generators](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md)
- [Nick Chapsas showing how a Generator influences build time](https://youtu.be/anesVdQg6Dk)
- [Andrew Lock's post explaining Incremental Generators](https://andrewlock.net/exploring-dotnet-6-part-9-source-generator-updates-incremental-generators/)
- [List of Source Generators](https://github.com/amis92/csharp-source-generators)
- [Implementation of "Smart Enums" generator](https://www.thinktecture.com/en/net/roslyn-source-generators-introduction/)
- [Incremental Source Generators documentation that comes with Roslyn](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md)
- [Example how to write an Analyzer](https://www.codingzee.com/2021/04/c-code-analysis-with-roslyn-syntax-trees.html)
