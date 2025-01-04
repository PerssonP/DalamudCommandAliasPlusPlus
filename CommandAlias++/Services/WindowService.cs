using CommandAliasPlusPlus.Windows;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandAliasPlusPlus.Services;

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
