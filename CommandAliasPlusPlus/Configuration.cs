using Dalamud.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CommandAliasPlusPlus;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;
    public List<AliasCommand> AliasedCommands { get; set; } = [];
    public void AliasCheckValid()
        => AliasedCommands.ForEach(alias => alias.CheckValid());
}

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
