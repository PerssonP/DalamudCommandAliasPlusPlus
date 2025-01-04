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
    public Dictionary<string, string> AliasedCommands { get; set; }
        = new() { { "/clipboardgather", "/gather {cb}" }, { "/testinglists", "/echo {[one,two,three]}" }, { "/unsupported", "/echo {foo}" } };
}
