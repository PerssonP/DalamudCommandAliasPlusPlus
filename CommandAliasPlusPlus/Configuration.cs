using CommandAliasPlusPlus.Services;
using Dalamud.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
/// Each AliasCommand also has a flag to indicate if the command is valid. This flag is not written to disk. Use <see cref="ConfigurationService.ValidateConfiguration"/> to calculate the flag.
/// </summary>
internal record class AliasCommand
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Alias { get; set; } = "";
    public string Canonical { get; set; } = "";
    
    [JsonIgnore]
    public string? Error { get; set; } = null;
}
