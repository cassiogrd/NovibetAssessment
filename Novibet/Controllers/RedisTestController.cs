using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

[ApiController]
[Route("api/[controller]")]
public class RedisTestController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;

    public RedisTestController(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    [HttpGet("test")]
    public async Task<IActionResult> TestRedis()
    {
        try
        {
            var db = _redis.GetDatabase();

            Console.WriteLine("Tentando salvar valor no Redis...");
            bool setResult = await db.StringSetAsync("test-key", "Hello from RedisTestController!");
            Console.WriteLine($"Resultado do SET: {setResult}");

            if (!setResult)
            {
                return StatusCode(500, "Falha ao salvar valor no Redis.");
            }

            Console.WriteLine("Tentando recuperar valor do Redis...");
            var value = await db.StringGetAsync("test-key");

            if (value.HasValue)
            {
                Console.WriteLine($"Valor obtido do Redis: {value}");
                return Ok($"Redis retornou: {value}");
            }

            Console.WriteLine("Chave não encontrada no Redis.");
            return NotFound("Falha ao conectar ao Redis ou chave não encontrada.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao conectar ao Redis: {ex.Message}");
            return StatusCode(500, $"Erro ao conectar ao Redis: {ex.Message}");
        }
    }
}
