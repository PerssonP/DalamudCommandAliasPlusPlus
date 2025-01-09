using CommandAliasPlusPlus.Services;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace CommandAliasPlusPlus.Windows;

/// <summary>
/// Window to allow the user to alter Configuration.AliasCommands.
/// </summary>
internal class ConfigWindow : Window
{
    private readonly ConfigurationService _configService;

    public ConfigWindow(ConfigurationService configService)
        : base("CommandAlias++ Configuration")
    {
        _configService = configService;

        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        bool changed = false;

        ImGui.Columns(3);
        ImGui.TextUnformatted("Alias command");
        ImGui.NextColumn();
        ImGui.TextUnformatted("Canonical command");
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.Separator();

        foreach (AliasCommand command in _configService.Config.AliasCommands)
        {
            string alias = command.Alias;
            string canon = command.Canonical;
            ImGui.SetNextItemWidth(-5);
            if (ImGui.InputText($"###alias{command.Id}", ref alias, 500))
            {
                command.Alias = alias;
                changed = true;
            }
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-5);
            if (ImGui.InputText($"###canon{command.Id}", ref canon, 500))
            {
                command.Canonical = canon;
                changed = true;
            }
            ImGui.NextColumn();

            if (ImGui.Button($"-###delete{command.Id}"))
            {
                command.Delete = true;
                changed = true;
            }
            ImGui.NextColumn();
            ImGui.Separator();
        }

        ImGui.Columns(1);
        if (ImGui.Button("Add new row"))
        {
            _configService.Config.AliasCommands.Add(new AliasCommand());
            changed = true;
        }

        if (changed)
        {
            int deletionIndex = _configService.Config.AliasCommands.FindIndex(c => c.Delete);
            if (deletionIndex != -1)
                _configService.Config.AliasCommands.RemoveAt(deletionIndex);

            _configService.Save();
        }
    }
}
