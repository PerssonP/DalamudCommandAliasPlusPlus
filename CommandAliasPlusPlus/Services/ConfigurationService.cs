using Dalamud.Plugin;
using System;
using System.Linq;

namespace CommandAliasPlusPlus.Services;

/// <summary>
/// Service to handle the plugin configuration.
/// </summary>
internal class ConfigurationService(IDalamudPluginInterface pluginInterface)
{
    public Configuration Config { get; init; } = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

    /// <summary>
    /// Save configuration to disk.
    /// </summary>
    public void Save()
        => pluginInterface.SavePluginConfig(Config);

    /// <summary>
    /// Run validity checks for all aliases in Config.
    /// </summary>
    public void RunAliasCommandValidityChecks()
        => Config.AliasCommands.ForEach(alias => alias.CheckValid());

    /// <summary>
    /// Get canonical command for given alias. Returns null if alias is not registered.
    /// </summary>
    /// <param name="alias">String to look for in Config.AliasCommands</param>
    /// <returns>Canonical command as a string or null if alias is not registered</returns>
    public string? GetCanonicalCommandForAlias(string alias)
        => Config.AliasCommands.FirstOrDefault(command =>
            command.Alias.Equals(alias, StringComparison.OrdinalIgnoreCase))?.Canonical;
}
