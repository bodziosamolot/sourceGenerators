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
public class MetadataController : ControllerBase
{{
    private readonly ILogger<MetadataController> _logger;

    public MetadataController(ILogger<MetadataController> logger)
    {{
        _logger = logger;
    }}

    [HttpGet(""controllers"", Name = ""GetControllerNames"")]
    public IEnumerable<string> GetControllerNames()
    {{
       return new List<string>() {{{string.Join(",", controllerNames)}}};
    }}

    [HttpGet(""functions"", Name = ""GetFunctionNames"")]
    public IEnumerable<string> GetFunctionNames()
    {{
        return new List<string>()  {{{string.Join(",",functionInfos.Select(functionInfo=>$"\"[{functionInfo.Flags}][{functionInfo.Kind}]{functionInfo.ParentClass}.{functionInfo.Name}\""))}}};
    }}
}}");
        return sourceBuilder.ToString();
    }
    
    public static string GetFunctionText(IEnumerable<string> controllerNames)
        {
            var sourceBuilder = new StringBuilder($@"using Microsoft.AspNetCore.Mvc;
    
    namespace WebApi.Controllers;
    
    [ApiController]
    [Route(""[controller]"")]
    public class MetadataController : ControllerBase
    {{
        private readonly ILogger<MetadataController> _logger;
    
        public MetadataController(ILogger<MetadataController> logger)
        {{
            _logger = logger;
        }}
    
        [HttpGet(""controllers"", Name = ""GetControllerNames"")]
        public IEnumerable<string> GetControllerNames()
        {{
           return new List<string>() {{{string.Join(",", controllerNames)}}};
        }}
    }}");
            return sourceBuilder.ToString();
        }
}