using Dalamud.Configuration;
using System;
using System.Collections.Generic;

namespace CommandAliasPlusPlus;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;
    public List<AliasCommand> AliasedCommands { get; set; } = [];
}

public class AliasCommand
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Alias { get; set; } = "";
    public string Canonical { get; set; } = "";
}
