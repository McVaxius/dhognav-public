using System;
using System.IO;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;

namespace DhogNav.PublicShell;

internal sealed class IntroductionWindow : Window
{
    private readonly AethertekUI.Dalamud.MaterialWindowMotion motion = new();
    private const string DiscordUrl = "https://discord.gg/VsXqydsvpu";
    private const string SupportUrl = "https://ko-fi.com/mcvaxius";
    private readonly PublicUi ui;
    private readonly AethertekUI.MaterialWindowOpacity opacity = new();
    private bool openAppearanceSection;
    internal void OpenSettings() { openAppearanceSection = true; IsOpen = true; }
    private readonly ISharedImmediateTexture icon;
    private readonly ModuleLoader loader;
    private readonly Action refresh;

    public IntroductionWindow(IDalamudPluginInterface pluginInterface, ITextureProvider textures, ModuleLoader loader, Action refresh, PublicUi ui)
        : base($"DhogNav v{BuildInfo.Version}###DhogNav.PublicShell.Introduction")
    {
        this.loader = loader;
        this.refresh = refresh;
        this.ui = ui;
        Size = new Vector2(620, 700);
        SizeCondition = ImGuiCond.FirstUseEver;
        Flags |= ImGuiWindowFlags.HorizontalScrollbar;
        SizeConstraints = new WindowSizeConstraints { MinimumSize = new Vector2(360, 380), MaximumSize = new Vector2(float.MaxValue) };
        icon = textures.GetFromFile(Path.Combine(pluginInterface.AssemblyLocation.DirectoryName!, "icon.png"));
    }

    public override void PreDraw() => motion.Prepare(this, reducedMotion: false, roundedCorners: true);
    public override void PostDraw()
    {
        motion.Restore(this);
        ui.ApplyWindowOpacity(opacity, WindowName);
    }

    public override void Draw()
    {
        motion.DrawChrome();
        var root = ImGui.GetID("");
        if (icon.TryGetWrap(out var texture, out _))
        {
            ImGui.Image(texture.Handle, new Vector2(ui.Compact ? 36 : 48) * ImGuiHelpers.GlobalScale); ImGui.SameLine();
        }
        ImGui.BeginGroup(); PublicIntroduction.Header(ui, BuildInfo.Version); ImGui.EndGroup();
        PublicIntroduction.Card("DhogNav-PublicAbout", root, ui, AethertekUI.MaterialIcon.Globe, "PUBLIC ACCESS HOST", () =>
        {
            PublicIntroduction.Brand(ui);
            ui.Heading("Saved locations and navigation"); ImGui.Separator();
            AethertekUI.MaterialText.TextWrapped(ui.T("This public plugin provides the introduction and privately granted module access."));
        });
        PublicIntroduction.Card("DhogNav-PublicCommunity", root, ui, AethertekUI.MaterialIcon.Group, "JOIN THE COMMUNITY", () =>
        {
            AethertekUI.MaterialText.TextWrapped(ui.T("Visit The Dumpster Fire community for updates, discussion and access from McVaxius. Support does not automatically grant access."));
            var actions = PublicIntroduction.CommunityActions(ui, "Open Discord");
            if (actions.Discord) Util.OpenLink(DiscordUrl);
            if (actions.Support) Util.OpenLink(SupportUrl);
        });
        PublicIntroduction.Card("DhogNav-PublicAccess", root, ui, AethertekUI.MaterialIcon.Download, ui.Compact ? "INSTALLATION (APM)" : "ACCESS", () =>
        {
            AethertekUI.MaterialText.TextWrapped(ui.T("Copy your direct access ZIP link. Include and enable DhogNav in APM, trust the publisher, then choose Check clipboard for updates. Select a row only for duplicate eligible copies."));
            AethertekUI.MaterialText.TextWrapped(ui.T("Keep DhogNav and APM enabled until the update finishes. The public plugin stays installed; restart DhogNav and open /dnav."));
            if (loader.Failed) AethertekUI.MaterialText.TextColored(UiStyle.Warning, ui.T("Access could not initialize. Check /xllog for [Access] details."));
            if (UiStyle.NativeButton("Check installed access", ui.T("Check installed access"), Vector2.Zero)) refresh();
        });
        if (openAppearanceSection) { ImGui.SetNextItemOpen(true); openAppearanceSection = false; }
        if (AethertekUI.MaterialText.CollapsingHeader(ui.T("Window appearance") + "###WindowAppearanceSection")) ui.WindowAppearance();
    }
}
