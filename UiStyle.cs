using System.Numerics;
using AethertekUI;
using Dalamud.Bindings.ImGui;

#if DHOGNAV_PRIVATE_UI
namespace DhogNav.PrivateUi;
#else
namespace DhogNav.PublicShell;
#endif

internal enum UiFontRole { Body, BodyStrong, Title, Caption, Small, Heading, Action, CompactTitle }

internal static class UiStyle
{
    internal static readonly float[] FontSizes = [11, 12, 24, 10, 9, 13, 11, 21];
    internal static readonly string[] FontFiles = ["segoeui.ttf", "seguisb.ttf", "segoeuib.ttf", "segoeui.ttf", "segoeui.ttf", "seguisb.ttf", "seguisb.ttf", "segoeuib.ttf"];
    internal static bool Compact { get; set; }
    internal static float Padding => Compact ? 10 : 14;
    internal static float Gap => Compact ? 10 : 14;
    internal static Vector4 Ready => Rgb(0x24E6CB);
    internal static Vector4 Warning => Rgb(0xFFBF68);
    internal static Vector4 Error => Rgb(0xFF586B);
    internal static Vector4 Rgb(uint rgb) => new(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, 1);
    internal static uint Pack(Vector3 rgb) => ((uint)Math.Clamp((int)MathF.Round(rgb.X * 255), 0, 255) << 16)
        | ((uint)Math.Clamp((int)MathF.Round(rgb.Y * 255), 0, 255) << 8) | (uint)Math.Clamp((int)MathF.Round(rgb.Z * 255), 0, 255);
    internal static MaterialTheme Theme(uint accent, bool publicHost)
    {
        var referenceRgb = publicHost ? 0xA475FFu : 0x1CC9E6u;
        accent &= 0xFFFFFF;
        var reference = Rgb(referenceRgb);
        var original = MaterialColor.LabToLch(MaterialColor.SrgbToOklab(new(reference.X, reference.Y, reference.Z)));
        var selected = Rgb(accent);
        var seed = MaterialColor.LabToLch(MaterialColor.SrgbToOklab(new(selected.X, selected.Y, selected.Z)));
        Vector4 Relative(uint rgb)
        {
            var color = Rgb(rgb);
            if (accent == referenceRgb) return color;
            var lch = MaterialColor.LabToLch(MaterialColor.SrgbToOklab(new(color.X, color.Y, color.Z)));
            return new(MaterialColor.GamutMap(lch.X, seed.Y < .001f ? 0 : lch.Y * seed.Y / original.Y,
                lch.Z + (seed.Y < .001f ? 0 : seed.Z - original.Z)), 1);
        }
        var palette = new OklchPaletteGenerator().Generate(new(selected.X, selected.Y, selected.Z));
        // Surface samples from the approved DhogNav review mockups.
        var background = Relative(publicHost ? 0x111322u : 0x0F1A21u);
        var foreground = Relative(0xF2F4FB);
        var primary = Relative(referenceRgb);
        var colors = new MaterialColorScheme(palette)
        {
            Background = background, OnBackground = foreground,
            Surface = Relative(publicHost ? 0x171A2Bu : 0x0D202Bu), OnSurface = foreground,
            SurfaceContainerLowest = Relative(publicHost ? 0x111322u : 0x0C1D26u),
            SurfaceContainerLow = Relative(publicHost ? 0x171A2Bu : 0x0D202Bu),
            SurfaceContainer = Relative(publicHost ? 0x1D1C37u : 0x152A37u),
            SurfaceContainerHigh = Relative(publicHost ? 0x1D1F3Cu : 0x1D3443u),
            SurfaceContainerHighest = Relative(publicHost ? 0x292448u : 0x243F4Eu),
            SurfaceVariant = Relative(publicHost ? 0x383157u : 0x2C4D5Cu),
            OnSurfaceVariant = Relative(publicHost ? 0xBDC8DDu : 0xC6D9E5u),
            Outline = Relative(publicHost ? 0x465269u : 0x496677u),
            OutlineVariant = Relative(publicHost ? 0x323E50u : 0x294854u),
            Primary = primary, OnPrimary = MaterialColor.Contrast(primary, background) >= MaterialColor.Contrast(primary, foreground) ? background : foreground,
            PrimaryContainer = Relative(publicHost ? 0x5040D5u : 0x005D68u), OnPrimaryContainer = foreground,
            Secondary = Relative(publicHost ? 0xB9A8ECu : 0xA5DCE0u), OnSecondary = background,
            SecondaryContainer = Relative(publicHost ? 0x242C3Au : 0x1D3443u), OnSecondaryContainer = foreground,
            Tertiary = Relative(publicHost ? 0xCDBCEEu : 0xB4D7E7u), OnTertiary = background,
            TertiaryContainer = Relative(publicHost ? 0x2C3544u : 0x243F4Eu), OnTertiaryContainer = foreground,
            InverseSurface = foreground, InverseOnSurface = background, InversePrimary = Relative(publicHost ? 0x6542A9u : 0x167E87u),
        };
        return new(colors) { SurfaceOpacity = 1 };
    }
    internal static MaterialStyleScope Geometry(float scale)
    {
        var scope = new MaterialStyleScope();
        scope.Style(ImGuiStyleVar.WindowPadding, new Vector2(Padding) * scale);
        scope.Style(ImGuiStyleVar.FramePadding, new Vector2(Compact ? 6 : 8, Compact ? 3 : 5) * scale);
        scope.Style(ImGuiStyleVar.ItemSpacing, new Vector2(Compact ? 8 : 12, Compact ? 5 : 7) * scale);
        scope.Style(ImGuiStyleVar.CellPadding, new Vector2(Compact ? 6 : 10, Compact ? 4 : 6) * scale);
        scope.Style(ImGuiStyleVar.FrameRounding, 4 * scale);
        scope.Style(ImGuiStyleVar.ChildRounding, 4 * scale);
        scope.Style(ImGuiStyleVar.FrameBorderSize, scale);
        return scope;
    }
    internal static void Panel(string id, uint root, Vector2 size, Action draw, bool raised = false)
    {
        var min = ImGui.GetCursorScreenPos();
        var extent = new Vector2(size.X <= 0 ? ImGui.GetContentRegionAvail().X : size.X, size.Y);
        var colors = MaterialTheme.Current.Colors;
        MaterialCanvas.Surface(min, min + extent, raised ? colors.SurfaceContainerHigh : colors.Surface, colors.Background, 4 * MaterialTheme.Metrics.Scale);
        ImGui.GetWindowDrawList().AddRect(min, min + extent, MaterialCanvas.Color(colors.OutlineVariant), 4 * MaterialTheme.Metrics.Scale);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(Padding * MaterialTheme.Metrics.Scale));
        ImGui.PushStyleColor(ImGuiCol.ChildBg, Vector4.Zero);
        try
        {
            if (ImGui.BeginChild(id, size, false, ImGuiWindowFlags.AlwaysUseWindowPadding | ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGuiP.PushOverrideID(root);
                try { draw(); } finally { ImGui.PopID(); }
            }
            ImGui.EndChild();
        }
        finally { ImGui.PopStyleColor(); ImGui.PopStyleVar(); }
    }
    internal static void Panel(string id, uint root, Action draw)
    {
        var colors = MaterialTheme.Current.Colors;
        var min = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var padding = Padding * ImGui.GetIO().FontGlobalScale;
        using var style = new MaterialStyleScope();
        style.Style(ImGuiStyleVar.CellPadding, new Vector2(padding));
        if (!ImGui.BeginTable(id, 1, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchSame)) return;
        ImGui.TableNextRow(); ImGui.TableSetColumnIndex(0);
        ImGuiP.PushOverrideID(root);
        try
        {
            draw();
            var max = new Vector2(min.X + width, ImGuiP.GetCurrentWindow().DC.CursorMaxPos.Y + padding);
            ImGuiP.TablePushBackgroundChannel();
            try
            {
                MaterialCanvas.Surface(min, max, colors.SurfaceContainerHigh, colors.Surface, 4 * MaterialTheme.Metrics.Scale);
                ImGui.GetWindowDrawList().AddRect(min, max, MaterialCanvas.Color(colors.OutlineVariant), 4 * MaterialTheme.Metrics.Scale);
            }
            finally { ImGuiP.TablePopBackgroundChannel(); }
        }
        finally { ImGui.PopID(); ImGui.EndTable(); }
    }
    internal static bool NativeButton(string native, string visible, Vector2 size, bool primary = false, MaterialIcon icon = MaterialIcon.None)
    {
        using var height = MaterialText.PushLineHeight(visible);
        var colors = MaterialTheme.Current.Colors;
        var danger = icon == MaterialIcon.Delete;
        if (primary || danger)
        {
            var fill = danger ? Vector4.Lerp(colors.Surface, Error, .2f) : colors.PrimaryContainer;
            var highlight = danger ? Error : colors.Primary;
            ImGui.PushStyleColor(ImGuiCol.Button, fill);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Lerp(fill, highlight, .25f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Lerp(fill, highlight, .4f));
        }
        // Keep the raw label's native ID, including buttons predating keyed localization.
        size.X = MaterialLayout.FitNextItemWidth(size.X, MathF.Ceiling(ButtonWidth(visible, icon)));
        ImGui.PushStyleColor(ImGuiCol.Text, Vector4.Zero);
        var pressed = ImGui.Button(native, size);
        ImGui.PopStyleColor();
        if (primary || danger) ImGui.PopStyleColor(3);
        var min = ImGui.GetItemRectMin(); var max = ImGui.GetItemRectMax();
        var iconWidth = icon == MaterialIcon.None ? 0 : ImGui.GetFontSize() * 1.7f;
        var textSize = MaterialText.Measure(visible);
        var position = min + new Vector2((max.X - min.X - textSize.X - iconWidth) * .5f, (max.Y - min.Y - textSize.Y) * .5f);
        var ink = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
        if (danger) ink = Error;
        if (ImGui.GetStyle().Alpha < 1) ink.W *= ImGui.GetStyle().Alpha;
        if (icon != MaterialIcon.None) MaterialIcons.Draw(icon, position, ImGui.GetFontSize() * 1.3f, ink);
        MaterialText.AddText(ImGui.GetWindowDrawList(), ImGui.GetFont(), ImGui.GetFontSize(), position + new Vector2(iconWidth, 0), ImGui.ColorConvertFloat4ToU32(ink), visible);
        return pressed;
    }
    internal static float ButtonWidth(string visible, MaterialIcon icon = MaterialIcon.None) => MathF.Ceiling(MaterialText.Measure(visible).X + ImGui.GetStyle().FramePadding.X * 2
        + (icon == MaterialIcon.None ? 0 : ImGui.GetFontSize() * 1.7f));
}
