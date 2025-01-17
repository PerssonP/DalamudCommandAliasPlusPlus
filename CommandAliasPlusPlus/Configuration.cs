using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandAliasPlusPlus;

/// <summary>
/// Configuration class for CommandAliasPlusPlus.
/// </summary>
internal class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool FirstTime { get; set; } = true;
    public List<AliasCommand> AliasCommands { get; set; } = [];
}

/// <summary>
/// A class containing an alias command. Contains the alias and the canonical command.<br />
/// Each AliasCommand has a unique ID which is used to keep track of the object (e.g. in ConfigWindow).<br />
/// Each AliasCommand also has a flag to indicate if the command is valid. This flag is not written to disk. Use <see cref="CheckValid"/> to calculate the flag.
/// </summary>
internal record class AliasCommand
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Alias { get; set; } = "";
    public string Canonical { get; set; } = "";
    
    [JsonIgnore]
    public string? Error { get; private set; } = null;

    /// <summary>
    /// Check if AliasCommand is valid.<br />
    /// Sets AliasCommand.Valid to null if validation is passed, otherwise to an error message.
    /// </summary>
    public void CheckValid()
    {
        // Validate Alias
        string? aliasError =
            CommandValidator.Empty(Alias) ??
            CommandValidator.LeadingWhitespace(Alias) ??
            CommandValidator.LeadingSlash(Alias) ??
            CommandValidator.InBlacklist(Alias); // TODO: Check alias is unique

        if (aliasError != null)
        {
            Error = $"Alias: {aliasError}";
            return;
        }

        // Validate Canonical
        string? canonicalError =
            CommandValidator.Empty(Canonical) ??
            CommandValidator.LeadingWhitespace(Canonical) ??
            CommandValidator.LeadingSlash(Canonical);

        if (canonicalError != null)
        {
            Error = $"Canonical: {canonicalError}";
            return;
        }

        // Remove any Error message is all validations pass
        Error = null;
    }
}

/// <summary>
/// Class of validator functions to be run on AliasCommand.Alias and AliasCommand.Canonical.<br />
/// Each function takes a string input (the command) and returns an error message is the validation failed or null if it succeeded.
/// </summary>
internal static class CommandValidator
{
    private static readonly string[] CommandBlacklist = ["alias", "aliasconfig"];

    public static string? InBlacklist(string command)
        => CommandBlacklist.Contains(command, StringComparer.OrdinalIgnoreCase) ? "Do not use this as your command" : null;

    public static string? Empty(string command)
        => command == string.Empty ? "Command cannot be empty" : null;

    public static string? LeadingWhitespace(string command)
        => char.IsWhiteSpace(command[0]) ? "Command cannot start with an empty character" : null;

    public static string? LeadingSlash(string command)
        => command[0] == '/' ? "Starting your command with a forward slash is not necessary" : null;
}
