using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Plugin;

namespace CommandAliasPlusPlus;

internal class PluginServices
{
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService]
    internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService]
    internal static IClientState ClientState { get; private set; } = null!;
    [PluginService]
    internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService]
    internal static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService]
    internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
}


