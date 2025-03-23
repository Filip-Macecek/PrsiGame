using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using PrsiWeb.Services;

namespace PrsiWeb;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

        builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(Program).Assembly));
        builder.Services.AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));
        builder.Services.AddSingleton<IGameSessionRepository, InMemoryGameSessionRepository>();
        builder.Services.AddSingleton<WebSocketClientService>();

        var app = builder.Build();

// Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseWebSockets();
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.UseDefaultFiles();
        app.UseStaticFiles(new StaticFileOptions()
        {
            ServeUnknownFileTypes = true
        });

        app.Run();
    }
}
