using Dalamud.Plugin;
using System;
using System.Linq;

namespace CommandAliasPlusPlus.Services;

/// <summary>
/// Service to handle the plugin configuration.
/// </summary>
internal class ConfigurationService(IDalamudPluginInterface pluginInterface)
{
    private static readonly string[] CommandBlacklist = ["alias", "aliasconfig"];
    public Configuration Config { get; init; } = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

    /// <summary>
    /// Save configuration to disk.
    /// </summary>
    public void Save()
        => pluginInterface.SavePluginConfig(Config);

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

    /// <summary>
    /// Validate Configuration.<br />
    /// </summary>
    public void ValidateConfiguration()
        => Config.AliasCommands.ForEach(ValidateAliasCommand);

    /// <summary>
    /// Check if AliasCommand is valid.<br />
    /// Sets AliasCommand.Valid to null if validation is passed, otherwise to an error message.
    /// </summary>
    public void ValidateAliasCommand(AliasCommand command)
    {
        // Validate Alias
        string? aliasError =
            ValidateEmpty(command.Alias) ??
            ValidateLeadingWhitespace(command.Alias) ??
            ValidateLeadingForwardSlash(command.Alias) ??
            ValidateInBlacklist(command.Alias); // todo: check unique

        if (aliasError != null)
        {
            command.Error = $"Alias: {aliasError}";
            return;
        }

        // Validate Canonical
        string? canonicalError =
            ValidateEmpty(command.Canonical) ??
            ValidateLeadingWhitespace(command.Canonical) ??
            ValidateLeadingForwardSlash(command.Canonical);

        if (canonicalError != null)
        {
            command.Error = $"Canonical: {canonicalError}";
            return;
        }

        // Remove any Error message is all validations pass
        command.Error = null;
    }

    private static string? ValidateInBlacklist(string command)
        => CommandBlacklist.Contains(command, StringComparer.OrdinalIgnoreCase) ? "Do not use this as your command" : null;

    private static string? ValidateEmpty(string command)
        => command == string.Empty ? "Command cannot be empty" : null;

    private static string? ValidateLeadingWhitespace(string command)
        => char.IsWhiteSpace(command[0]) ? "Command cannot start with an empty character" : null;

    private static string? ValidateLeadingForwardSlash(string command)
        => command[0] == '/' ? "Starting your command with a forward slash is not necessary" : null;
}
