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

# Rider versions

- 2022.1 EAP 5 - couldn't make the generated files be included in the build
- 2021.3.3 - works fine

# Links 

- [Source generators vs Incremental Source Generators by Andrew Locke](https://andrewlock.net/exploring-dotnet-6-part-9-source-generator-updates-incremental-generators/)

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