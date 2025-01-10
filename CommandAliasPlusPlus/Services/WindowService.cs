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
    ConfigWindow configWindow,
    IntroductionWindow introWindow,
    TokenInfoWindow tokenInfoWindow)
    : IDisposable
{
    private readonly WindowSystem _windowSystem = new("CommandAlias++");

    public void InitWindows()
    {
        _windowSystem.AddWindow(configWindow);
        _windowSystem.AddWindow(introWindow);
        _windowSystem.AddWindow(tokenInfoWindow);

        pluginInterface.UiBuilder.Draw += DrawUI;
        pluginInterface.UiBuilder.OpenConfigUi += ToggleConfigWindow;
    }

    public void Dispose()
    {
        _windowSystem.RemoveAllWindows();
    }

    public void DrawUI() => _windowSystem.Draw();

    public void ToggleConfigWindow() => configWindow.Toggle();
    public void ToggleIntroWindow() => introWindow.Toggle();
    public void ToggleTokenInfoWindow() => tokenInfoWindow.Toggle();
}
