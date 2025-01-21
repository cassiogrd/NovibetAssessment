using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Serilog;
using Quartz;
using Novibet.Interfaces;
using Novibet.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog para logs no console
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

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
builder.Services.AddScoped<ReportRepository>();
builder.Services.AddSingleton(new AdoConnectionService(connectionString));
builder.Services.AddScoped<IIpInfoRepository, IpInfoRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

//builder.Services.AddScoped<IIpInfoService, IpInfoService>();
//builder.Services.AddScoped<IReportService, ReportService>();




// Configura��o do Quartz
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("IpUpdateJob");
    q.AddJob<IpUpdateJob>(opts => opts.WithIdentity(jobKey).DisallowConcurrentExecution());
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("IpUpdateJob-Trigger")
        .WithCronSchedule("0 0 * * * ?")); // Executa a cada 1 hora
});


builder.Services.AddScoped<IpUpdateJob>(); // Altere aqui para Scoped
builder.Services.AddQuartzHostedService();

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
