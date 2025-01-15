using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

//using (var scope = builder.Services.BuildServiceProvider().CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

//    try
//    {
//        var countries = context.Countries.ToList();
//        Console.WriteLine(" Teste de Conexão: " + countries.Count + " países encontrados.");
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine("Erro ao conectar ao banco: " + ex.Message);
//    }
//}
builder.Services.AddScoped<IpInfoRepository>();
builder.Services.AddScoped<IpInfoService>();
builder.Services.AddHttpClient<IpInfoService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
