using CommandAliasPlusPlus.Windows;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;

namespace CommandAliasPlusPlus.Services;

/// <summary>
/// Service to handle setup and disposal of windows in WindowSystem.
/// </summary>
internal class WindowService(
    IDalamudPluginInterface pluginInterface,
    WindowSystem windowSystem,
    UIService uiService,
    ConfigWindow configWindow,
    IntroductionWindow introWindow,
    TokenInfoWindow tokenInfoWindow)
    : IDisposable
{
    public void InitWindows()
    {
        windowSystem.AddWindow(configWindow);
        windowSystem.AddWindow(introWindow);
        windowSystem.AddWindow(tokenInfoWindow);

        pluginInterface.UiBuilder.Draw += windowSystem.Draw;
        pluginInterface.UiBuilder.OpenConfigUi += uiService.ToggleWindow<ConfigWindow>;
    }

    public void Dispose()
    {
        windowSystem.RemoveAllWindows();
    }
}
