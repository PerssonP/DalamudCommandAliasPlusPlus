
using CommandAliasPlusPlus.Services;
using CommandAliasPlusPlus.Windows;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommandAliasPlusPlus;

/// <summary>
/// Bootstrap class for CommandAlias++.
/// </summary>
public class Plugin : IDalamudPlugin
{
    private readonly IHost _host;

    public Plugin(
        IDalamudPluginInterface pluginInterface,
        IPluginLog pluginLog,
        ICommandManager commandManager,
        IClientState clientState,
        IDataManager dataManager,
        IGameInteropProvider gameInteropProvider)
    {

        _host = new HostBuilder()
            .UseContentRoot(pluginInterface.ConfigDirectory.FullName)
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(pluginInterface.GetPluginConfig() as Configuration ?? new Configuration())
                    // Dalamud services
                    .AddSingleton(pluginInterface)
                    .AddSingleton(pluginLog)
                    .AddSingleton(commandManager)
                    .AddSingleton(clientState)
                    .AddSingleton(dataManager)
                    .AddSingleton(gameInteropProvider)
                    // Plugin
                    .AddSingleton<ConfigWindow>()
                    .AddSingleton<WindowService>()
                    .AddSingleton<CommandService>()
                    .AddHostedService<CommandAliasPlusPlus>();
            })
            .Build();

        _host.StartAsync();
    }

    public void Dispose()
    {
        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();
    }
}
