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
    private readonly TokenInfoWindow _tokenInfoWindow;

    public ConfigWindow(
        ConfigurationService configService,
        TokenInfoWindow tokenInfoWindow)
        : base("CommandAlias++ Configuration")
    {
        _configService = configService;
        _tokenInfoWindow = tokenInfoWindow;

        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(500, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        
        if (ImGui.Button("?"))
            _tokenInfoWindow.Toggle();
        ImGui.Separator();

        ImGui.Columns(4);
        ImGui.TextUnformatted("Alias command");
        ImGui.NextColumn();
        ImGui.TextUnformatted("Canonical command");
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.NextColumn();
        ImGui.Separator();

        for (int i = 0; i < _configService.Config.AliasCommands.Count; i++)
        {
            bool changed = false;

            var command = _configService.Config.AliasCommands[i];
            string alias = command.Alias;
            string canon = command.Canonical;

            ImGui.TextUnformatted("/");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-5);
            changed |= ImGui.InputText($"###alias{command.Id}", ref alias, 500);
            ImGui.NextColumn();
            ImGui.TextUnformatted("/");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-5);
            changed |= ImGui.InputText($"###canon{command.Id}", ref canon, 500);
            ImGui.NextColumn();
            if (ImGui.Button($"-###delete{command.Id}"))
            {
                _configService.Config.AliasCommands.RemoveAt(i);
                _configService.Save();
                return;
            }
            ImGui.NextColumn();
            if (command.Error != null)
                ImGui.TextUnformatted(command.Error);
            ImGui.NextColumn();
            ImGui.Separator();

            if (changed)
            {
                command.Alias = alias;
                command.Canonical = canon;
                command.CheckValid();
                _configService.Save();
            }
        }

        ImGui.Columns(1);
        if (ImGui.Button("Add new row"))
        {
            AliasCommand newCommand = new();
            newCommand.CheckValid();
            _configService.Config.AliasCommands.Add(newCommand);
            _configService.Save();
        }
    }
}
