using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FunctionTextProvider
{
    public static string GetFunctionText(IEnumerable<string> controllerNames, IEnumerable<FunctionInformation> functionInfos)
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

    [HttpGet(""functions"", Name = ""GetFunctionNamesIncremental"")]
    public IEnumerable<string> GetFunctionNames()
    {{
        return new List<string>()  {{{string.Join(",",functionInfos.Select(functionInfo=>$"\"[{functionInfo.Flags}][{functionInfo.Kind}]{functionInfo.ParentClass}.{functionInfo.Name}\""))}}};
    }}

    public static void {controllerNames.First()}()
    {{
        Console.WriteLine(""This is a test"");
    }}
}}");
        return sourceBuilder.ToString();
    }
}