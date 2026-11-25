using Microsoft.Extensions.Hosting;
using MythByte.Application;
using MythByte.GameServer;
using MythByte.Infrastructure;
using MythByte.Networking;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .RegisterApplication()
    .RegisterInfrastructure()
    .RegisterNetworking()
    .RegisterGameServer(builder.Configuration);

var host = builder.Build();

await host.RunAsync();
