using MythByte.API;
using MythByte.Application;
using MythByte.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .RegisterApplication()
    .RegisterInfrastructure()
    .RegisterAPI();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
