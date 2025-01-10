using Dalamud.Configuration;
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
    [JsonIgnore]
    private static readonly string[] AliasBlacklist = ["alias", "aliasconfig"];
    /// <summary>
    /// Check if AliasCommand is valid. Sets AliasCommand.Valid to true or false.
    /// </summary>
    public void CheckValid()
    {
        // TODO: Clean this up..
        // Check Alias
        if (Alias == string.Empty)
        {
            Error = "Alias command cannot be empty";
            return;
        }

        if (char.IsWhiteSpace(Alias[0]))
        {
            Error = "Alias command cannot begin with whitespace";
            return;
        }

        if (AliasBlacklist.Contains(Alias, StringComparer.OrdinalIgnoreCase))
        {
            Error = "Do not use this command as an alias";
            return;
        }

        // Check Canonical
        if (Canonical == string.Empty)
        {
            Error = "Canonical command cannot be empty";
            return;
        }

        if (char.IsWhiteSpace(Canonical[0]))
        {
            Error = "Canonical command cannot begin with whitespace";
            return;
        }

        if (Alias[0] == '/' || Canonical[0] == '/')
        {
            Error = "Don't start your command with /";
            return;
        }

        Error = null;
    }
}
