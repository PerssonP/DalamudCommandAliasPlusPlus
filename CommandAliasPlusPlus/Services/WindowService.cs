using CommandAliasPlusPlus.Windows;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;

namespace CommandAliasPlusPlus.Services;

/// <summary>
/// Service to handle windows and WindowSystem.
/// </summary>
internal class WindowService(
    IDalamudPluginInterface pluginInterface,
    ConfigWindow configWindow)
    : IDisposable
{
    private readonly WindowSystem _windowSystem = new("CommandAlias++");

    public void InitWindows()
    {
        _windowSystem.AddWindow(configWindow);

        pluginInterface.UiBuilder.Draw += DrawUI;
        pluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
    }

    public void Dispose()
    {
        _windowSystem.RemoveAllWindows();
    }

    public void DrawUI() => _windowSystem.Draw();
    public void ToggleConfigUI() => configWindow.Toggle();
}
