var builder = WebApplication.CreateBuilder(args);

// Configure Ocelot and API Gateway services

var app = builder.Build();

app.Run();
