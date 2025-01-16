using Microsoft.AspNetCore.Mvc;
using Novibet.Models; // Importa IpInfoResponse
using StackExchange.Redis;
using Newtonsoft.Json;

[ApiController]
[Route("api/ipinfo")]
public class IpInfoController : ControllerBase
{
    private readonly IpInfoService _ipInfoService;

    public IpInfoController(IpInfoService ipInfoService)
    {
        _ipInfoService = ipInfoService;
    }

    [HttpGet("{ip}")]
    public async Task<IActionResult> GetIpInfo(string ip)
    {
        try
        {
            var ipInfo = await _ipInfoService.GetIpInfoAsync(ip);
            if (ipInfo != null)
            {
                return Ok(ipInfo);
            }

            return NotFound("IP information not found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro: {ex.Message}");
        }
    }
}
