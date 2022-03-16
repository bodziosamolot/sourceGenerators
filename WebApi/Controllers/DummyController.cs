using Microsoft.AspNetCore.Mvc;

public class DummyController : Controller
{
    // GET
    public IActionResult Index()
    {
        return null;
    }

    public int ThisIsATestFunctionReally()
    {
        return 32;
    }
}