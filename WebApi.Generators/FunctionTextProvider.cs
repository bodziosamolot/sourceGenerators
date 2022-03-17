using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FunctionTextProvider
{
    public static string GetFunctionText(IEnumerable<string> controllerNames)
        {
            var sourceBuilder = new StringBuilder($@"using Microsoft.AspNetCore.Mvc;
    
    namespace WebApi.Controllers;
    
    [ApiController]
    [Route(""[controller]"")]
    public class BasicMetadataController : ControllerBase
    {{
        private readonly ILogger<BasicMetadataController> _logger;
    
        public BasicMetadataController(ILogger<BasicMetadataController> logger)
        {{
            _logger = logger;
        }}
    
        [HttpGet(""basic/controllers"", Name = ""GetControllerNamesBasic"")]
        public IEnumerable<string> GetControllerNames()
        {{
           return new List<string>() {{{string.Join(",", controllerNames)}}};
        }}
    }}");
            return sourceBuilder.ToString();
        }
}