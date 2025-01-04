using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

namespace CommandAliasPlusPlus.Services;

internal unsafe class CommandService(WindowService windowService, IPluginLog logger)
{
    public const string AliasCommandName = "/alias";
    public const string ConfigCommandName = "/aliasconfig";

    public CommandInfo AliasCommandInfo =>
        new(HandleAliasCommand)
        {
            HelpMessage = @"Alternate method of calling an alias.
Call this command with the name of the alias you want to execute.
Aliases created with CommandAlias++ cannot be used in macros however the /alias command can."
        };

    public CommandInfo ConfigCommandInfo =>
        new(HandleConfigCommand)
        {
            HelpMessage = "Toggle the configuration window for CommandAlias++"
        };

    private void HandleAliasCommand(string command, string alias)
    {
        // Extract alias and execute using ExecuteCommandInner to trigger detour
        logger.Debug("Alias triggered: {alias}", alias);
        Utf8String utf8String = new($"/{alias}");
        RaptureShellModule.Instance()->ExecuteCommandInner(&utf8String, UIModule.Instance());
    }

    private void HandleConfigCommand(string command, string alias)
        => windowService.ToggleConfigUI();
    
}
