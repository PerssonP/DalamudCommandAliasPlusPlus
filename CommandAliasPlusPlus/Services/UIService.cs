using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using System.Linq;

namespace CommandAliasPlusPlus.Services;

/// <summary>
/// Service for opening/closing windows
/// </summary>
internal class UIService(IPluginLog logger, WindowSystem windowSystem)
{
    public void ToggleWindow<T>() where T : Window
    {
        var window = windowSystem.Windows.FirstOrDefault(window => window is T);
        if (window == null)
        {
            logger.Error("UIService: Tried to toggle a window that wasn't registered in WindowSystem");
            return;
        }
        window.Toggle();
    }

    public void OpenOrFocusWindow<T>() where T : Window
    {
        var window = windowSystem.Windows.FirstOrDefault(window => window is T);
        if (window == null)
        {
            logger.Error("UIService: Tried to open a window that wasn't registered in WindowSystem");
            return;
        }

        if (!window.IsOpen)
            window.IsOpen = true;
        else
            window.BringToFront();
    }
}
