using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace CommandAliasPlusPlus.Windows;
internal class IntroductionWindow : Window
{
    public IntroductionWindow() : base("CommandAlias++ Introduction")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        ImGui.TextUnformatted("Introduction");
    }
}
