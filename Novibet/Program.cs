using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog para logs no console
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Define o n�vel m�nimo de log
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// Substituir o Logger padr�o
builder.Host.UseSerilog();

// Adicionar servi�os ao container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura��o do banco de dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configurando o Redis no cont�iner DI
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configurationOptions = new ConfigurationOptions
    {
        EndPoints = { { "redis-18245.c251.east-us-mz.azure.redns.redis-cloud.com", 18245 } },
        User = "default",
        Password = "5Tyy9aqnxCtNcIrJJTecpmMISByhvVpG",
        IncludeDetailInExceptions = true
    };

    return ConnectionMultiplexer.Connect(configurationOptions);
});

builder.Services.AddScoped<IpInfoRepository>();
builder.Services.AddScoped<IpInfoService>();
builder.Services.AddHttpClient<IpInfoService>();

// Configura��o do pipeline HTTP
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Configura��o de logs ao iniciar e ao finalizar a aplica��o
try
{
    Log.Information("Aplica��o iniciando...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplica��o encontrou um erro fatal e ser� encerrada.");
}
finally
{
    Log.Information("Aplica��o finalizada.");
    Log.CloseAndFlush();
}
