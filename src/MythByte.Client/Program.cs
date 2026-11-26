using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MythByte.Client;
using MythByte.Networking;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services
    .RegisterNetworking()
    .RegisterClient(builder.Configuration);

using var host = builder.Build();
using var game = host.Services.GetRequiredService<MythByteGame>();

game.Run();
