using Dalamud.Plugin;

namespace CommandAliasPlusPlus.Services;

internal class ConfigurationService(IDalamudPluginInterface pluginInterface)
{
    public Configuration Config { get; init; } = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
    public void Save()
        => pluginInterface.SavePluginConfig(Config);
}
