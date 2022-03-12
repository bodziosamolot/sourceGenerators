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