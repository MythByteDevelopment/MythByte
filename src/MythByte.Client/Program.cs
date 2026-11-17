using Microsoft.Extensions.Hosting;
using MythByte.Client;
using MythByte.Networking;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .RegisterNetworking()
    .RegisterClient();

var host = builder.Build();

await host.RunAsync();
