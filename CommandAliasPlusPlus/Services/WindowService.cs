using CommandAliasPlusPlus.Windows;
using Dalamud.Interface.Windowing;
using System;

namespace CommandAliasPlusPlus.Services;

/// <summary>
/// Service to handle windows and WindowSystem.
/// </summary>
internal class WindowService : IDisposable
{
    private readonly WindowSystem _windowSystem = new("CommandAlias++");
    private readonly ConfigWindow _configWindow;

    public WindowService(ConfigWindow configWindow)
    {
        _configWindow = configWindow;
        _windowSystem.AddWindow(_configWindow);
    }

    public void Dispose()
    {
        _windowSystem.RemoveAllWindows();
    }

    public void DrawUI() => _windowSystem.Draw();
    public void ToggleConfigUI() => _configWindow.Toggle();
}
