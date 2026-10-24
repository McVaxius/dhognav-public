using System.Numerics;
using AethertekUI;
using Dalamud.Bindings.ImGui;

namespace DhogNav.PublicShell;

internal static class PublicIntroduction
{
    internal static void Card(string id, uint root, PublicUi ui, MaterialIcon symbol, string heading, Action draw)
    {
        // Native rows grow with translated paragraphs and complete button reflow.
        UiStyle.Panel(id, root, () =>
        {
            using (ui.Font(UiFontRole.BodyStrong))
            {
                if (MaterialTheme.Current.IsLcars) MaterialLcars.PanelHeading(ui.T(heading));
                else
                {
                MaterialIcons.Draw(symbol, ImGui.GetCursorScreenPos(), ImGui.GetFontSize(), MaterialTheme.Current.Colors.Primary);
                ImGui.Dummy(new Vector2(ImGui.GetFontSize())); ImGui.SameLine();
                AethertekUI.MaterialText.TextColored(MaterialTheme.Current.Colors.Primary, ui.T(heading));
                }
            }
            draw();
        });
        ImGui.Dummy(new Vector2(0, UiStyle.Gap * .5f * ImGui.GetIO().FontGlobalScale));
    }
    internal static void Header(PublicUi ui, string version)
    {
        ImGui.BeginGroup();
        using (ui.Font(ui.Compact ? UiFontRole.CompactTitle : UiFontRole.Title)) AethertekUI.MaterialText.Text("DhogNav");
        AethertekUI.MaterialText.TextDisabled("mcvaxius | v" + version);
        ImGui.EndGroup();
    }
    internal static void Brand(PublicUi ui)
    {
        using var title = ui.Font(UiFontRole.Title);
        var size = (ui.Compact ? 36 : 40) * ImGui.GetIO().FontGlobalScale;
        var factor = size / ImGui.GetFontSize();
        var origin = ImGui.GetCursorScreenPos();
        var extent = AethertekUI.MaterialText.Measure("DHOGNAV") * factor;
        var prefix = AethertekUI.MaterialText.Measure("DHOG").X * factor;
        var colors = MaterialTheme.Current.Colors;
        var list = ImGui.GetWindowDrawList();
        list.AddText(ImGui.GetFont(), size, origin, MaterialCanvas.Color(colors.OnSurface), "DHOG");
        list.AddText(ImGui.GetFont(), size, origin + new Vector2(prefix, 0), MaterialCanvas.Color(colors.Primary), "NAV");
        ImGui.Dummy(extent);
    }
    internal static (bool Discord, bool Support) CommunityActions(PublicUi ui, string discordLabel)
    {
        using var action = ui.Font(UiFontRole.Action);
        var discord = ui.T(discordLabel);
        var support = ui.T("Support on Ko-fi");
        var window = ImGuiP.GetCurrentWindow();
        var scale = ImGui.GetIO().FontGlobalScale;
        var available = Math.Max(1, Math.Min(ImGui.GetContentRegionAvail().X,
            window.InnerRect.Max.X - window.WindowPadding.X - ImGui.GetCursorScreenPos().X));
        var gap = ImGui.GetStyle().ItemSpacing.X;
        var half = (available - gap) * .5f;
        var paired = half >= Math.Max(UiStyle.ButtonWidth(discord, MaterialIcon.Discord), UiStyle.ButtonWidth(support, MaterialIcon.KoFi));
        var width = paired ? half : available;
        var height = (ui.Compact ? 30 : 34) * scale;
        var openDiscord = UiStyle.NativeButton(discordLabel, discord, new Vector2(width, height), true, MaterialIcon.Discord);
        if (paired) ImGui.SameLine();
        var openSupport = UiStyle.NativeButton("Support on Ko-fi", support, new Vector2(width, height), false, MaterialIcon.KoFi);
        return (openDiscord, openSupport);
    }
}
