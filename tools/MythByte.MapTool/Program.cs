using Microsoft.Extensions.DependencyInjection;
using MythByte.MapTool;

var services = new ServiceCollection()
    .RegisterMapTool()
    .BuildServiceProvider();

return services.GetRequiredService<SyncNamesCommand>().Run(args, Console.Out);
