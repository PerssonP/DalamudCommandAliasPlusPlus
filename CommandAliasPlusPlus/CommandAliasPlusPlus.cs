using CommandAliasPlusPlus.Services;
using CommandAliasPlusPlus.Windows;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.Shell;
using ImGuiNET;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CommandAliasPlusPlus;

/// <summary>
/// Main plugin class. Starts the plugin, keeps track of resources and handles disposal.
/// </summary>
internal sealed unsafe class CommandAliasPlusPlus : IHostedService
{
    private readonly IPluginLog _logger;
    private readonly ICommandManager _commandManager;

    private readonly ConfigurationService _configService;
    private readonly CommandService _commandService;
    private readonly WindowService _windowService;
    private readonly UIService _uiService;

    private readonly Hook<ShellCommandModule.Delegates.ExecuteCommandInner> _executeCommandInnerHook;
    private readonly Dictionary<string, int> _listsAndLastGrabbedIndex = [];

    public CommandAliasPlusPlus(
        IPluginLog logger,
        ICommandManager commandManager,
        IGameInteropProvider gameInteropProvider,
        ConfigurationService configService,
        CommandService commandService,
        WindowService windowService,
        UIService uiService)
    {
        _logger = logger;
        _commandManager = commandManager;

        _configService = configService;
        _commandService = commandService;
        _windowService = windowService;
        _uiService = uiService;

        _executeCommandInnerHook = gameInteropProvider.HookFromAddress<ShellCommandModule.Delegates.ExecuteCommandInner>(
            ShellCommandModule.MemberFunctionPointers.ExecuteCommandInner,
            DetourExecuteCommandInner
        );
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _windowService.InitWindows();

        _commandManager.AddHandler(CommandService.AliasCommandName, _commandService.AliasCommandInfo);
        _commandManager.AddHandler(CommandService.ConfigCommandName, _commandService.ConfigCommandInfo);
        _executeCommandInnerHook.Enable();

        _configService.ValidateConfiguration();

        // Show IntroductionWindow on first time plugin loads
        if (_configService.Config.FirstTime)
        {
            _uiService.ToggleWindow<IntroductionWindow>();
            _configService.Config.FirstTime = false;
            _configService.Save();
        }

#if DEBUG
        foreach (var aliasCommand in _configService.Config.AliasCommands)
            _logger.Debug(aliasCommand.ToString());
#endif

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

    /// <summary>
    /// Detour for ExecuteCommandInner. If message is not consumed by the detour it will be returned to the original function.
    /// </summary>
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

            string messageString = message->ToString();
            _logger.Debug("Detour: Original command was {command}", messageString);

            if (messageString.StartsWith("/alias ", StringComparison.OrdinalIgnoreCase))
            {
                // Command is alias-command. Set command to be args of alias
                _logger.Debug("Detour: Alias-command found. Extracting args and continuing parse.");
                messageString = messageString[(messageString.IndexOf(' ') + 1)..];
            }
            else
            {
                // Remove leading slash
                messageString = messageString[1..];
            }

            string[] messageSplit = messageString.Split(' ', 2);
            string originalCommand = messageSplit[0];

            string? canonicalCommand = _configService.GetCanonicalCommandForAlias(originalCommand)?.Canonical;
            if (canonicalCommand == null)
            {
                _logger.Debug("Detour: Command was not a registered alias. Ending.");
                return;
            }

            string? originalArgs = messageSplit.Length > 1 ? messageSplit[1] : null;
            string translatedCommand = Regexes.Token()
                .Replace(canonicalCommand, (match) => TranslateToken(match.Groups[1].Value));
            _logger.Information("Detour: Alias was successfully matched and canonical command has been translated into {command}", translatedCommand);

            Utf8String translatedCommandUtf8String = new($"/{translatedCommand}{(originalArgs == null ? "" : " " + originalArgs)}");
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
    /// Check the registered lists for the current item from a given list-key.<br />
    /// If this is the first time the list-key is encountered, the list will be registered and the first item will be returned.<br />
    /// If the list-key has been encountered before, the index will increment and the current item will be returned.<br />
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
