using Dalamud.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CommandAliasPlusPlus;

/// <summary>
/// Configuration class for CommandAliasPlusPlus.
/// </summary>
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public List<AliasCommand> AliasCommands { get; set; } = [];
    /// <summary>
    /// Run validity checks for all aliases.
    /// </summary>
    public void AliasCheckValid()
        => AliasCommands.ForEach(alias => alias.CheckValid());
}

/// <summary>
/// A class containing an alias command. Contains the alias and the canonical command.<br />
/// Each AliasCommand has a unique ID which is used to keep track of the object (e.g. in ConfigWindow).<br />
/// Each AliasCommand also has a flag to indicate if the command is valid. This flag is not written to disk. Use <see cref="CheckValid"/> to calculate the flag.
/// </summary>
public record class AliasCommand
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Alias { get; set; } = "";
    public string Canonical { get; set; } = "";

    [JsonIgnore]
    public bool Valid { get; set; } = false;
    /// <summary>
    /// Check if AliasCommand is valid. Sets AliasCommand.Valid to true or false.
    /// </summary>
    public void CheckValid()
    {
        // Check Alias
        if (Alias == string.Empty || char.IsWhiteSpace(Alias[0]))
        {
            Valid = false;
            return;
        }

        // Check Alias
        if (Canonical == string.Empty || char.IsWhiteSpace(Canonical[0]))
        {
            Valid = false;
            return;
        }

        Valid = true;
    }
}
