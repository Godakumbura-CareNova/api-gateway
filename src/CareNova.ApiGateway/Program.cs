using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "api-gateway" }));

await app.UseOcelot();

app.Run();