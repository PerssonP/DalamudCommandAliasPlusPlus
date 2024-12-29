using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace CommandAliasPlusPlus;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

    public Dictionary<string, string> AliasedCommands { get; set; } = [];

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        CommandAliasPlusPlus.PluginInterface.SavePluginConfig(this);
    }
}
