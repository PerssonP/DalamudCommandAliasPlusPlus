using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace CommandAliasPlusPlus.Windows;
internal class TokenInfoWindow : Window
{
    public TokenInfoWindow() : base("CommandAlias++ Token Info")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        ImGui.TextUnformatted("Token info");
    }
}
