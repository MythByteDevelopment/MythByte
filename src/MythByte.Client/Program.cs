using Microsoft.Extensions.Hosting;
using MythByte.Client;
using MythByte.Networking;
using MythByte.Shared.Versioning;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .RegisterNetworking()
    .RegisterClient();

var host = builder.Build();

Console.WriteLine($"MythByte v{AppVersion.Of(typeof(Program).Assembly)}");

await host.RunAsync();
