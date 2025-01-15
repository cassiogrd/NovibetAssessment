using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/ipinfo")]
public class IpInfoController : ControllerBase
{
    private readonly IpInfoService _service;

    public IpInfoController(IpInfoService service)
    {
        _service = service;
    }

    //[HttpGet("{ip}")]

    [HttpGet("{ip}")] // inica qu a rota espera um parametro na url chamado ip
    // Por exemplo, o endpoint: https://localhost:7134/api/ipinfo/231 (onde 231 seria o ip)
    public async Task<IActionResult> GetIpInfo(string ip)
    {
        try
        {
            var result = await _service.GetIpInfoAsync(ip);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }
}
