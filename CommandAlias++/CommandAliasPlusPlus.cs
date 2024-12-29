using CommandAliasPlusPlus.Windows;
using Dalamud.Game.Command;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using FFXIVClientStructs.FFXIV.Component.Shell;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;

namespace CommandAliasPlusPlus;

public sealed unsafe class CommandAliasPlusPlus : IDalamudPlugin
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

    private const string CommandName = "/alias";
    private const string ConfigCommandName = "/aliasconfig";

    private readonly Dictionary<string, string> _commandAliases = new()
    {
        { "clipboardgather", "/gather $cb" },
        { "testinglists", "/echo $[one,two,three]" },
        { "unsupported", "/echo $foo" }
    };

    private readonly Dictionary<string, int> _listsAndLastGrabbedIndex = [];

    private readonly Hook<ShellCommandModule.Delegates.ExecuteCommandInner>? _executeCommandInnerHook;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private ConfigWindow ConfigWindow { get; init; }

    public CommandAliasPlusPlus()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        CommandManager.AddHandler(CommandName, new CommandInfo(OnAliasCommand)
        {
            HelpMessage = @"Alternate method of calling an alias.
Call this command with the name of the alias you want to execute.
Regular aliases created with CommandAlias++ will not work in macros however this /alias command will"
        });

        CommandManager.AddHandler(ConfigCommandName, new CommandInfo(OnConfigCommand)
        {
            HelpMessage = "Toggle the configuration window for CommandAlias++"
        });

        _executeCommandInnerHook = GameInteropProvider.HookFromAddress<ShellCommandModule.Delegates.ExecuteCommandInner>(
            ShellCommandModule.MemberFunctionPointers.ExecuteCommandInner,
            DetourExecuteCommandInner
        );
        _executeCommandInnerHook.Enable();
    }

    private void OnAliasCommand(string command, string alias)
    {
        // Will be executed if /alias is run in a macro
        // Execute command using ExecuteCommandInner to trigger detour
        PluginLog.Debug("Alias triggered: {alias}", alias);
        Utf8String utf8String = new($"{command} {alias}");
        RaptureShellModule.Instance()->ExecuteCommandInner(&utf8String, UIModule.Instance());
    }

    private void OnConfigCommand(string command, string _)
    {
        ToggleConfigUI();
    }

    private void DetourExecuteCommandInner(ShellCommandModule* self, Utf8String* command, UIModule* uiModule)
    {
        PluginLog.Debug("Detour hit for ExecuteCommandInner");
        bool commandConsumed = false;
        try
        {
            if (command->GetCharAt(0) != '/')
            {
                PluginLog.Debug("Ending: First char not equal /");
                return;
            }

            //var aliases = Configuration.CommandAliases;

            string originalCommand = command->ToString();
            PluginLog.Debug("Original command: " + originalCommand);

            string[] originalCommandParts = originalCommand.Split(' ', 2);
            if (originalCommandParts[0].Equals("/alias", StringComparison.OrdinalIgnoreCase))
            {
                // Command is alias-command. Set original command to be args of alias
                PluginLog.Debug("Alias hit in detour");
                originalCommand = originalCommandParts[^1];
            }
            else
            {
                // Remove leading forward slash
                originalCommand = originalCommand[1..];
            }

            string? canonicalCommand = _commandAliases.GetValueOrDefault(originalCommand);
            if (canonicalCommand == null)
            {
                PluginLog.Debug("Ending: No alias found");
                return;
            }


            string translatedCommand = TranslateCanonicalCommand(canonicalCommand);
            Utf8String translatedCommandUtf8 = new(translatedCommand);
            PluginLog.Debug("Executing translated command");

            _executeCommandInnerHook!.Original(self, &translatedCommandUtf8, uiModule);
            commandConsumed = true;
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "An error occured when handling a macro save event.");
        }
        finally
        {
            // If the command was not consumed in prior code, execute it here unaltered
            if (!commandConsumed)
                _executeCommandInnerHook!.Original(self, command, uiModule);
        }
    }

    /// <summary>
    /// Parse a canonical command for tokens and replace them with appropiate values.
    /// </summary>
    /// <param name="canonicalCommand">The canonical command to translate.</param>
    /// <returns>A new string containing the translated command, ready to be executed.</returns>
    private string TranslateCanonicalCommand(string canonicalCommand)
    {
        string[] canonicalCommandParts = canonicalCommand.Split(' ');
        for (int i = 1; i < canonicalCommandParts.Length; i++)
        {
            string part = canonicalCommandParts[i];

            PluginLog.Debug("Found part: " + part);
            if (part[0] == '$')
            {
                PluginLog.Debug("Found token: " + part);

                string translatedToken = TranslateToken(part);

                PluginLog.Debug("Translated token: " + translatedToken);
                canonicalCommandParts[i] = translatedToken;
            }
        }
        return string.Join(' ', canonicalCommandParts);
    }

    /// <summary>
    /// Translate a token.
    /// </summary>
    /// <param name="input">The token to be translated.</param>
    /// <returns>The translated value.</returns>
    private string TranslateToken(string input) => input switch
    {
        "$cb" => ImGui.GetClipboardText(),
        ['$', '[', .. var list, ']'] => GetCurrentItemFromList(list),
        _ => input
    };

    /// <summary>
    /// Check the registered lists for the current item from a given list-key
    /// If this is the first time the list-key is encountered, the list will be registered and the first item will be returned.
    /// If the list-key has been encountered before, the index will increment and the current item will be returned.
    /// If the index exceeds the length of the list the index will be reset and the first value will be returned.
    /// </summary>
    /// <param name="list">The list-key to check</param>
    /// <returns>The current item for a given list</returns>
    private string GetCurrentItemFromList(string list)
    {
        PluginLog.Debug("hit lists " + list);
        if (_listsAndLastGrabbedIndex.TryGetValue(list, out int lastGrabbedIndex))
        {
            string[] values = list.Split(',');
            int newIndex = lastGrabbedIndex + 1;

            PluginLog.Debug("new index: " + newIndex + " length: " + values.Length);
            if (newIndex >= values.Length)
            {
                _listsAndLastGrabbedIndex[list] = 0;
                return values[0];
            }
            else
            {
                _listsAndLastGrabbedIndex[list] = newIndex;
                return values[newIndex];
            }
        }

        _listsAndLastGrabbedIndex.Add(list, 0);
        return list.Split(',', 2)[0];
    }

    public void Dispose()
    {
        _executeCommandInnerHook?.Dispose();
        CommandManager.RemoveHandler(CommandName);
        CommandManager.RemoveHandler(ConfigCommandName);

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
    }

    private void DrawUI() => WindowSystem.Draw();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
}
