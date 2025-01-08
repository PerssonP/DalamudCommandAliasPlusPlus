using CommandAliasPlusPlus.Services;
using Dalamud.Hooking;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.Shell;
using ImGuiNET;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CommandAliasPlusPlus;

internal sealed unsafe class CommandAliasPlusPlus : IHostedService
{
    private readonly IPluginLog _logger;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly ICommandManager _commandManager;

    private readonly Configuration _config;
    private readonly CommandService _commandService;
    private readonly WindowService _windowService;

    private readonly Hook<ShellCommandModule.Delegates.ExecuteCommandInner> _executeCommandInnerHook;
    private readonly Dictionary<string, int> _listsAndLastGrabbedIndex = [];

    public CommandAliasPlusPlus(
        IPluginLog logger,
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IGameInteropProvider gameInteropProvider,
        ConfigurationService configService,
        CommandService commandService,
        WindowService windowService)
    {
        _logger = logger;
        _pluginInterface = pluginInterface;
        _commandManager = commandManager;

        _config = configService.Config;
        _commandService = commandService;
        _windowService = windowService;

        _pluginInterface.UiBuilder.Draw += windowService.DrawUI;
        _pluginInterface.UiBuilder.OpenConfigUi += windowService.ToggleConfigUI;

        _executeCommandInnerHook = gameInteropProvider.HookFromAddress<ShellCommandModule.Delegates.ExecuteCommandInner>(
            ShellCommandModule.MemberFunctionPointers.ExecuteCommandInner,
            DetourExecuteCommandInner
        );
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _commandManager.AddHandler(CommandService.AliasCommandName, _commandService.AliasCommandInfo);
        _commandManager.AddHandler(CommandService.ConfigCommandName, _commandService.ConfigCommandInfo);
        _executeCommandInnerHook.Enable();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _commandManager.RemoveHandler(CommandService.AliasCommandName);
        _commandManager.RemoveHandler(CommandService.ConfigCommandName);
        _executeCommandInnerHook?.Dispose();

        _windowService.Dispose();

        return Task.CompletedTask;
    }

    private void DetourExecuteCommandInner(ShellCommandModule* self, Utf8String* message, UIModule* uiModule)
    {
        _logger.Debug("Detour: Detour hit for ExecuteCommandInner");
        bool commandConsumed = false;
        try
        {
            if (message->GetCharAt(0) != '/')
            {
                _logger.Debug("Detour: Message was not a command. Ending.");
                return;
            }

            string originalCommand = message->ToString();
            _logger.Debug("Detour: Original command was {command}", originalCommand);

            if (originalCommand.StartsWith("/alias ", StringComparison.OrdinalIgnoreCase))
            {
                // Command is alias-command. Set command to be args of alias
                _logger.Debug("Detour: Alias-command found. Extracting args and continuing parse.");
                originalCommand = "/" + originalCommand[(originalCommand.IndexOf(' ') + 1)..];
            }

            string? canonicalCommand = _config.AliasedCommands.FirstOrDefault(command => command.Alias.Equals(originalCommand, StringComparison.OrdinalIgnoreCase))?.Canonical;
            if (canonicalCommand == null)
            {
                _logger.Debug("Detour: Command was not a registered alias. Ending.");
                return;
            }

            string translatedCommand = Regexes.Token()
                .Replace(canonicalCommand, (match) => TranslateToken(match.Groups[1].Value));
            _logger.Information("Detour: Alias was successfully matched and canonical command has been translated into {command}", translatedCommand);

            Utf8String translatedCommandUtf8String = new(translatedCommand);
            _logger.Debug("Detour: Executing translated command.");

            _executeCommandInnerHook!.Original(self, &translatedCommandUtf8String, uiModule);
            commandConsumed = true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occured when detouring ExecuteCommandInner.");
        }
        finally
        {
            // If the command was not consumed in prior code, return it here unaltered
            if (!commandConsumed)
            {
                _logger.Debug("Detour: Message was not consumed by detour. Returning message to originl function.");
                _executeCommandInnerHook!.Original(self, message, uiModule);
            }
        }
    }

    /// <summary>
    /// Translate a token.
    /// </summary>
    /// <param name="input">The token to be translated.</param>
    /// <returns>The translated value.</returns>
    private string TranslateToken(string input)
    {
        _logger.Debug("Detour: Token {token} found.", input);
        string translated = input switch
        {
            "cb" => ImGui.GetClipboardText(),
            ['[', .. var list, ']'] => GetCurrentItemFromList(list),
            _ => input
        };
        _logger.Debug("Detour: Token has been translated to {translated}", translated);
        return translated;
    }

    /// <summary>
    /// Check the registered lists for the current item from a given list-key
    /// If this is the first time the list-key is encountered, the list will be registered and the first item will be returned.
    /// If the list-key has been encountered before, the index will increment and the current item will be returned.
    /// If the index exceeds the length of the list the index will be reset and the first value will be returned.
    /// </summary>
    /// <param name="list">The list-key to check.</param>
    /// <returns>The current item for a given list.</returns>
    private string GetCurrentItemFromList(string list)
    {
        _logger.Debug("List token: Using key {list}", list);
        if (_listsAndLastGrabbedIndex.TryGetValue(list, out int lastGrabbedIndex))
        {
            string[] values = list.Split(',');
            int newIndex = lastGrabbedIndex + 1;
            _logger.Debug("List token: List found. New index is {newIndex}.", newIndex);

            if (newIndex >= values.Length)
            {
                _logger.Debug("List token: List depleted. Resetting index.");
                _listsAndLastGrabbedIndex[list] = 0;
                return values[0];
            }
            else
            {
                _listsAndLastGrabbedIndex[list] = newIndex;
                return values[newIndex];
            }
        }

        _logger.Debug("New list-key encountered. Registering and returning first value.");
        _listsAndLastGrabbedIndex.Add(list, 0);
        return list.Split(',', 2)[0];
    }
}

public static partial class Regexes
{
    [GeneratedRegex("{(.*)}", RegexOptions.Singleline)]
    public static partial Regex Token();

}
