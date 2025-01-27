using CommandAliasPlusPlus.Windows;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

namespace CommandAliasPlusPlus.Services;

/// <summary>
/// Service to handle the custom plugin commands.
/// </summary>
internal unsafe class CommandService(UIService uiService, IPluginLog logger)
{
    /// <summary>
    /// Command to run an alias.
    /// </summary>
    public const string AliasCommandName = "/alias";

    /// <summary>
    /// Command to open the config window.
    /// </summary>
    public const string ConfigCommandName = "/aliasconfig";

    /// <summary>
    /// CommandInfo for AliasCommand
    /// </summary>
    public CommandInfo AliasCommandInfo =>
        new(HandleAliasCommand)
        {
            HelpMessage = @"Alternate method of calling an alias.
Call this command with the name of the alias you want to execute.
Aliases created with CommandAlias++ cannot be used in macros however the /alias command can."
        };

    /// <summary>
    /// CommandInfo for ConfigCommand
    /// </summary>
    public CommandInfo ConfigCommandInfo =>
        new(HandleConfigCommand)
        {
            HelpMessage = "Toggle the configuration window for CommandAlias++"
        };

    /// <summary>
    /// Function that is called when <see cref="AliasCommandName"/> is run.<br />
    /// Grabs the parameters of the command (the requested alias) and sends them to ExecuteCommandInner as a command.
    /// This will trigger DetourExecuteCommandInner.
    /// </summary>
    /// <param name="alias">The requested alias</param>
    private void HandleAliasCommand(string _, string alias)
    {
        // Extract alias and execute using ExecuteCommandInner to trigger detour
        logger.Debug("Alias triggered: {alias}", alias);
        RaptureShellModule.Instance()->ExecuteCommandInner(Utf8String.FromString($"/{alias}"), UIModule.Instance());
    }

    /// <summary>
    /// Function that is called when <see cref="ConfigCommandName"/> is run.<br />
    /// Toggles the config window.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="alias"></param>
    private void HandleConfigCommand(string command, string alias)
        => uiService.ToggleWindow<ConfigWindow>();
    
}
