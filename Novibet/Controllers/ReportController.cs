using Microsoft.AspNetCore.Mvc;
using Serilog;
using Novibet.Interfaces;

[ApiController]
[Route("api/reports")]
public class ReportController : ControllerBase
{
    private readonly IReportRepository _reportRepository;

    public ReportController(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    [HttpGet("addresses-per-country")]
    public async Task<IActionResult> GetReport([FromQuery] string? countryCodes)
    {
        try
        {
            var report = await _reportRepository.GetCountryReportAsync(countryCodes);
            return Ok(report);
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Erro ao gerar o relatório de endereços por país.");
            return StatusCode(500, "Erro interno ao gerar o relatório.");
        }
    }
}
