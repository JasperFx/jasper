using Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten()

var app = builder.Build();



app.Run();
