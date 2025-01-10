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
    /// Get AliasCommand for given alias. Returns null if alias is not registered.<br />
    /// Only returns AliasCommands with no errors.
    /// </summary>
    /// <param name="alias">String to look for in Config.AliasCommands</param>
    /// <returns>AliasCommand or null if alias is not registered</returns>
    public AliasCommand? GetCanonicalCommandForAlias(string alias)
        => Config.AliasCommands.FirstOrDefault(command =>
            command.Error == null &&
            command.Alias.Equals(alias, StringComparison.OrdinalIgnoreCase));
}
