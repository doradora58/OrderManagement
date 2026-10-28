using OrderManagement.Repositories;
using OrderManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry
const string serviceName = "OrderManagement";

// Add services to the container
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<IOrderService, OrderService>();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapGet("/api/version", () =>
{
    var version = Environment.GetEnvironmentVariable("APP_VERSION") ?? "dev";
    return Results.Ok(new { version });
});

app.MapControllers();

app.Run();
