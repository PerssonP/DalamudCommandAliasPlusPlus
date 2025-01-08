using CommandAliasPlusPlus.Services;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;

namespace CommandAliasPlusPlus.Windows;

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

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (_configService.Config.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        ImGui.Columns(2);
        ImGui.TextUnformatted("Alias command");
        ImGui.NextColumn();
        ImGui.TextUnformatted("Canonical command");
        ImGui.NextColumn();
        ImGui.Separator();

        List<AliasCommand> aliases2 = _configService.Config.AliasedCommands;
        bool changed = false;
        foreach (AliasCommand command in _configService.Config.AliasedCommands)
        {
            string alias = command.Alias;
            string canon = command.Canonical;
            if (ImGui.InputText($"###alias{command.Id}", ref alias, 500))
            {
                command.Alias = alias;
                changed = true;
            }
            ImGui.NextColumn();
            if (ImGui.InputText($"###canon{command.Id}", ref canon, 500))
            {
                command.Canonical = canon;
                changed = true;
            }
            ImGui.NextColumn();
            ImGui.Separator();
        }

        if (changed)
            _configService.Save();
    }
}
