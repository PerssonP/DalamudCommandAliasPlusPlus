using CommandAliasPlusPlus.Services;
using CommandAliasPlusPlus.Windows;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommandAliasPlusPlus;

/// <summary>
/// Bootstrap class for CommandAliasPlusPlus.
/// </summary>
internal class Plugin : IDalamudPlugin
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
        var builder = Host.CreateEmptyApplicationBuilder(new()
        {
            ContentRootPath = pluginInterface.ConfigDirectory.FullName
        });

        builder.Services
            .AddHostedService<CommandAliasPlusPlus>()
            // Dalamud services
            .AddSingleton(pluginInterface)
            .AddSingleton(pluginLog)
            .AddSingleton(commandManager).AddSingleton(clientState)
            .AddSingleton(dataManager)
            .AddSingleton(gameInteropProvider)
            // Plugin
            .AddSingleton<ConfigurationService>()
            .AddSingleton(new WindowSystem("CommandAlias++"))
            .AddSingleton<WindowService>()
            .AddSingleton<UIService>()
            .AddSingleton<CommandService>()
            .AddSingleton<ConfigWindow>()
            .AddSingleton<IntroductionWindow>()
            .AddSingleton<TokenInfoWindow>();

        _host = builder.Build();
        _host.StartAsync();
    }

    public void Dispose()
    {
        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();
    }
}
