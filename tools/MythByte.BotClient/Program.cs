using Microsoft.Extensions.Hosting;
using MythByte.BotClient;
using MythByte.Networking;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .RegisterNetworking()
    .RegisterBotClient();

var host = builder.Build();

await host.RunAsync();
