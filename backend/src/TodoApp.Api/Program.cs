using System.Text.Json.Serialization;
using TodoApp.Application;
using TodoApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Spa",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Spa");
app.UseAuthorization();
app.MapControllers();
app.Run();
