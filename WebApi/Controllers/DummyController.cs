using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

public class DummyController : Controller
{
    // GET
    public IActionResult Index()
    {
        return null;
    }

    public int ThisIsATestFunction()
    {
        return 1;
    }
}