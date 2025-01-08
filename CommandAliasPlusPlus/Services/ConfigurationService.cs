using Dalamud.Plugin;

namespace CommandAliasPlusPlus.Services;

/// <summary>
/// Service to handle the plugin configuration.
/// </summary>
internal class ConfigurationService(IDalamudPluginInterface pluginInterface)
{
    public Configuration Config { get; init; } = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
    public void Save()
        => pluginInterface.SavePluginConfig(Config);
}
