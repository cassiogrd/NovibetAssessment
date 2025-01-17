using Microsoft.AspNetCore.Mvc;
using Novibet.Models; // Importa IpInfoResponse
using StackExchange.Redis;
using Newtonsoft.Json;
using Serilog;

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
        Log.Information("Requisição recebida para consultar IP: {Ip}", ip);

        if (!IsValidIPv4(ip))
        {
            Log.Warning("IP inválido fornecido: {Ip}", ip);

            return BadRequest(new
            {
                Status = 400,
                Message = "O valor fornecido não é um IPv4 válido.",
                Timestamp = DateTime.UtcNow
            });
        }

        try
        {
            Log.Information("Consultando informações para o IP: {Ip}", ip);
            var ipInfo = await _ipInfoService.GetIpInfoAsync(ip);
            Log.Information("Resposta enviada para o IP {Ip}: {@IpInfo}", ip, ipInfo);
            return Ok(ipInfo);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao consultar informações para o IP: {Ip}", ip);
            return StatusCode(500, $"Erro: {ex.Message}");
        }
    }


    // Validação manual de IPv4
    private bool IsValidIPv4(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return false;

        var parts = ip.Split('.');
        if (parts.Length != 4)
            return false;

        foreach (var part in parts)
        {
            if (!int.TryParse(part, out var num) || num < 0 || num > 255)
                return false;
        }

        return true;
    }


}
