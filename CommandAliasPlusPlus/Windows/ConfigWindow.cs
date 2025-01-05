using CommandAliasPlusPlus.Services;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ImGuiNET;
using System;
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
        // can't ref a property, so use a local copy
        var configValue = _configService.Config.SomePropertyToBeSavedAndWithADefault;
        if (ImGui.Checkbox("Random Config Bool", ref configValue))
        {
            _configService.Config.SomePropertyToBeSavedAndWithADefault = configValue;
            _configService.Save();
        }

        var movable = _configService.Config.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            _configService.Config.IsConfigWindowMovable = movable;
            _configService.Save();
        }
    }

    /*
    public void MainWindowDrawUnused()
    {
        // Do not use .Text() or any other formatted function like TextWrapped(), or SetTooltip().
        // These expect formatting parameter if any part of the text contains a "%", which we can't
        // provide through our bindings, leading to a Crash to Desktop.
        // Replacements can be found in the ImGuiHelpers Class
        ImGui.TextUnformatted($"The random config bool is {_config.SomePropertyToBeSavedAndWithADefault}");

        if (ImGui.Button("Show Settings"))
        {
            //Plugin.ToggleConfigUI();
        }

        ImGui.Spacing();

        // Normally a BeginChild() would have to be followed by an unconditional EndChild(),
        // ImRaii takes care of this after the scope ends.
        // This works for all ImGui functions that require specific handling, examples are BeginTable() or Indent().
        using (var child = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, true))
        {
            // Check if this child is drawing
            if (child.Success)
            {
                ImGui.TextUnformatted("Have a goat:");
                var goatImage = PluginServices.TextureProvider.GetFromFile("path").GetWrapOrDefault();
                if (goatImage != null)
                {
                    using (ImRaii.PushIndent(55f))
                    {
                        ImGui.Image(goatImage.ImGuiHandle, new Vector2(goatImage.Width, goatImage.Height));
                    }
                }
                else
                {
                    ImGui.TextUnformatted("Image not found.");
                }

                ImGuiHelpers.ScaledDummy(20.0f);

                // Example for other services that Dalamud provides.
                // ClientState provides a wrapper filled with information about the local player object and client.

                var localPlayer = PluginServices.ClientState.LocalPlayer;
                if (localPlayer == null)
                {
                    ImGui.TextUnformatted("Our local player is currently not loaded.");
                    return;
                }

                if (!localPlayer.ClassJob.IsValid)
                {
                    ImGui.TextUnformatted("Our current job is currently not valid.");
                    return;
                }

                // ExtractText() should be the preferred method to read Lumina SeStrings,
                // as ToString does not provide the actual text values, instead gives an encoded macro string.
                ImGui.TextUnformatted($"Our current job is ({localPlayer.ClassJob.RowId}) \"{localPlayer.ClassJob.Value.Abbreviation.ExtractText()}\"");

                // Example for quarrying Lumina directly, getting the name of our current area.
                var territoryId = PluginServices.ClientState.TerritoryType;
                if (PluginServices.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
                {
                    ImGui.TextUnformatted($"We are currently in ({territoryId}) \"{territoryRow.PlaceName.Value.Name.ExtractText()}\"");
                }
                else
                {
                    ImGui.TextUnformatted("Invalid territory.");
                }
            }
        }
    }*/
}
