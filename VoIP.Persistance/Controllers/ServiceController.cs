using Microsoft.AspNetCore.Mvc;

public class ServiceController : Controller
{
    private readonly UdpSenderService _backgroundService;

    public ServiceController(UdpSenderService backgroundService)
    {
        _backgroundService = backgroundService;
    }

    [HttpPost]
    public IActionResult StartService()
    {
        _backgroundService.StartService();
        return Ok("Service started");
    }

    [HttpPost]
    public IActionResult StopService()
    {
        _backgroundService.StopService();
        return Ok("Service stopped");
    }

    [HttpGet]
    public IActionResult GetStatus()
    {
        var status = _backgroundService.IsRunning ? "running" : "stopped";
        return Ok($"Service is currently {status}");
    }
}
