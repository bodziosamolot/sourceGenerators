using System.Text;
using Microsoft.CodeAnalysis;

namespace SourceGenerator;

public class FunctionTextProvider
{
    public static string GetFunctionText(IEnumerable<string> controllerNames)
    {
        var sourceBuilder = new StringBuilder($@"using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route(""[controller]"")]
public class ControllerListController : ControllerBase
{{
    private readonly ILogger _logger;

    public WeatherForecastController(ILogger logger)
    {{
        _logger = logger;
    }}

    [HttpGet(Name = ""GetControllerNames"")]
    public IEnumerable<string> Get()
    {{
       return new List<string>() {{{string.Join(",", controllerNames)}}};
    }}
}}");
        return sourceBuilder.ToString();
    }
}