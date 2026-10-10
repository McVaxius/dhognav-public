using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Resources;
using AethertekUI;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace DhogNav.PublicShell;

internal sealed class PublicUi : IDisposable
{
    internal static readonly (string Code, string Name)[] Languages = [("en", "English"), ("de", "Deutsch"), ("fr", "Français"),
        ("es", "Español"), ("it", "Italiano"), ("ru", "Русский"), ("ja", "日本語"), ("ko", "한국어"), ("zh-Hans", "简体中文"),
        ("vi", "Tiếng Việt"), ("pt-BR", "Português (Brasil)"), ("id", "Bahasa Indonesia"), ("pl", "Polski"), ("tr", "Türkçe"), ("hi", "हिन्दी")];
    private readonly PublicPreferences preferences;
    private readonly ManagedUiFonts fonts;
    private readonly AethertekUI.Dalamud.MaterialTextHost shapedText;
    private readonly MaterialWindowOpacity fontStatusOpacity = new();
    private readonly Dictionary<string, ResourceSet> sets = [];
    private ResourceSet current = null!;
    private MaterialTheme? theme;
    private uint accent;
    private uint draftAccent = uint.MaxValue;
    private Vector3 accentDraft;
    private bool frameCompact;
    private bool loggedFontIssue;
    internal PublicUi(IDalamudPluginInterface pi, ITextureProvider textures)
    {
        preferences = new(pi); shapedText = new(textures);
        fonts = new(pi.UiBuilder, "DhogNav public interface") { ShapedText = shapedText.Renderer };
        foreach (var language in Languages)
        {
            var stream = typeof(PublicUi).Assembly.GetManifestResourceStream("DhogNav.PublicShell.Strings." + language.Code + ".resources")
                ?? throw new MissingManifestResourceException(language.Code);
            sets.Add(language.Code, new ResourceSet(stream));
        }
        var keys = sets["en"].Cast<DictionaryEntry>().Select(e => (string)e.Key).ToArray();
        foreach (var (code, set) in sets)
            if (set.Cast<DictionaryEntry>().Count() != keys.Length || keys.Any(k => string.IsNullOrWhiteSpace(set.GetString(k))))
                throw new MissingManifestResourceException("Incomplete DhogNav public translations: " + code);
    }
    internal string T(string key) => current.GetString(key) ?? throw new MissingManifestResourceException(key);
    internal IDisposable Font(UiFontRole role) => fonts.Push(role);
    internal bool Compact => frameCompact;
    internal void Draw(Action draw, Action<Exception> report)
    {
        using var shaping = shapedText.Push();
        preferences.Reload();
        var language = sets.ContainsKey(preferences.Language) ? preferences.Language : "en";
        current = sets[language];
        fonts.Prepare(language, current.Cast<DictionaryEntry>().Select(e => (string)e.Value!)
            .Concat(sets["en"].Cast<DictionaryEntry>().Select(e => (string)e.Value!)).Concat(Languages.Where(l => l.Code != "hi").Select(l => l.Name)));
        UiStyle.Compact = frameCompact = preferences.Compact;
        var selected = preferences.Accent & 0xFFFFFF;
        if (theme == null || accent != selected) { accent = selected; theme = UiStyle.Theme(selected, true); }
        theme.Density = frameCompact ? MaterialDensity.Compact : MaterialDensity.Standard;
        using var colors = MaterialTheme.Push(theme, ImGui.GetIO().FontGlobalScale, MaterialStyleMode.ColorsOnly);
        using var chrome = MaterialWindowChrome.Push();
        if (!fonts.Ready())
        {
            if (!loggedFontIssue && fonts.Error is { } error) { report(error); loggedFontIssue = true; }
            var hindiFailed = language == "hi" && fonts.Error is not null;
            ManagedUiFonts.DrawStatusWithRecovery(fonts.Error == null, language == "hi"
                ? hindiFailed ? "Hindi is unavailable. Use English to recover; your saved language is unchanged." : "Preparing DhogNav interface fonts..."
                : T(fonts.Error == null ? "Preparing DhogNav interface fonts..." : "DhogNav interface fonts are unavailable. See the Dalamud log."),
                hindiFailed ? () => { preferences.Language = "en"; preferences.Save(); } : null);
            ApplyWindowOpacity(fontStatusOpacity, "DhogNav##FontStatus"); return;
        }
        using var geometry = UiStyle.Geometry(ImGui.GetIO().FontGlobalScale);
        using var body = fonts.Push(UiFontRole.Body);
        draw();
    }
    internal float AppearanceWidth()
    {
        using var action = Font(UiFontRole.Action);
        var scale = MaterialTheme.Metrics.Scale;
        var metrics = MaterialControls.Metrics;
        var language = Languages.FirstOrDefault(row => row.Code == preferences.Language).Name ?? "Select...";
        var languageWidth = Math.Max(130 * scale, MathF.Ceiling(MaterialText.Measure(language).X
            + metrics.Height + 3 * metrics.Gap + Math.Min(metrics.IconSize, metrics.Height)));
        var compactWidth = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemInnerSpacing.X + MaterialText.Measure("C").X;
        return (preferences.UiLanguageVisibleOnMainWindow ? languageWidth + ImGui.GetStyle().ItemSpacing.X : 0)
            + (preferences.UiCompactVisibleOnMainWindow ? compactWidth + ImGui.GetStyle().ItemSpacing.X : 0)
            + (preferences.UiTransparencyVisibleOnMainWindow ? ImGui.GetFrameHeight() + ImGui.GetStyle().ItemInnerSpacing.X + MaterialText.Measure(T("Transparency")).X : 0);
    }
    internal void Appearance(bool header = false)
    {
        if (draftAccent != preferences.Accent)
        { draftAccent = preferences.Accent; var rgb = UiStyle.Rgb(draftAccent); accentDraft = new(rgb.X, rgb.Y, rgb.Z); }
        var language = preferences.Language;
        using var action = Font(UiFontRole.Action);
        var options = new MaterialOptions<string>(Languages.Select(l => new MaterialOption<string>(l.Code, l.Code,
            l.Code == "hi" && !fonts.HindiAvailable ? "Hindi (unavailable)" : l.Name, l.Code == "hi" && !fonts.HindiAvailable)).ToArray());
        var accentChanged = false;
        var languageChanged = false;
        if (!header)
        {
            var changed = MaterialAppearanceSelector.Draw("DhogNav-public-appearance", ref accentDraft, ref language, options,
                new(T("Color"), T("Language"), T("Teal"), T("Blue"), T("Pink"), T("Custom RGB")), 130);
            accentChanged = changed.AccentChanged; languageChanged = changed.LanguageChanged;
        }
        else if (preferences.UiLanguageVisibleOnMainWindow)
            languageChanged = MaterialAppearanceSelector.DrawLanguage("DhogNav-public-appearance", ref language, options, 130);
        if (accentChanged) preferences.Accent = UiStyle.Pack(accentDraft);
        if (languageChanged) preferences.Language = language;
        if (!header || preferences.UiCompactVisibleOnMainWindow)
        {
            var edge = ImGuiP.GetCurrentWindow().InnerClipRect.Max.X;
            var width = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemInnerSpacing.X + MaterialText.Measure("C").X;
            if ((!header || preferences.UiLanguageVisibleOnMainWindow) && ImGui.GetItemRectMax().X + ImGui.GetStyle().ItemSpacing.X + width <= edge) ImGui.SameLine();
            var compact = preferences.Compact;
            if (ImGui.Checkbox("C##DhogNav-public-compact", ref compact)) { preferences.Compact = compact; preferences.Save(); }
            if (ImGui.IsItemHovered()) MaterialText.SetTooltip(T("Compact mode"));
        }
        if (header && preferences.UiTransparencyVisibleOnMainWindow)
        {
            var edge = ImGuiP.GetCurrentWindow().InnerClipRect.Max.X;
            var width = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemInnerSpacing.X + MaterialText.Measure(T("Transparency")).X;
            if ((preferences.UiCompactVisibleOnMainWindow || preferences.UiLanguageVisibleOnMainWindow) && ImGui.GetItemRectMax().X + ImGui.GetStyle().ItemSpacing.X + width <= edge) ImGui.SameLine();
            var enabled = preferences.UiTransparencyEnabled;
            if (Checkbox(T("Transparency") + "###UiTransparencyHeader", ref enabled)) { preferences.UiTransparencyEnabled = enabled; preferences.Save(); }
        }
        if (accentChanged || languageChanged) preferences.Save();
    }
    internal void Heading(string key)
    {
        using var heading = Font(UiFontRole.Heading); MaterialText.Text(T(key));
    }
    public void Dispose() { fonts.Dispose(); shapedText.Dispose(); foreach (var set in sets.Values) set.Dispose(); }
    internal void WindowAppearance()
    {
        MaterialText.Text(T("Window appearance"));
        Appearance();
        var changed = false;
        var compactVisibleOnMainWindow = preferences.UiCompactVisibleOnMainWindow;
        if (Checkbox(T("Compact visible on main window") + "###UiCompactVisibleOnMainWindowSettings", ref compactVisibleOnMainWindow))
        { preferences.UiCompactVisibleOnMainWindow = compactVisibleOnMainWindow; changed = true; }
        var transparencyVisibleOnMainWindow = preferences.UiTransparencyVisibleOnMainWindow;
        if (Checkbox(T("Transparency visible on main window") + "###UiTransparencyVisibleOnMainWindowSettings", ref transparencyVisibleOnMainWindow))
        { preferences.UiTransparencyVisibleOnMainWindow = transparencyVisibleOnMainWindow; changed = true; }
        var languageVisibleOnMainWindow = preferences.UiLanguageVisibleOnMainWindow;
        if (Checkbox(T("Language visible on main window") + "###UiLanguageVisibleOnMainWindowSettings", ref languageVisibleOnMainWindow))
        { preferences.UiLanguageVisibleOnMainWindow = languageVisibleOnMainWindow; changed = true; }
        var transparencyEnabled = preferences.UiTransparencyEnabled;
        if (Checkbox(T("Transparency") + "###UiTransparencyEnabledSettings", ref transparencyEnabled))
        { preferences.UiTransparencyEnabled = transparencyEnabled; changed = true; }
        var autoFade = preferences.UiAutoFade;
        if (Checkbox(T("Auto-fade when unfocused") + "###UiAutoFadeSettings", ref autoFade))
        { preferences.UiAutoFade = autoFade; changed = true; }
        ImGui.BeginDisabled(!transparencyEnabled);
        var windowOpacityPercent = Math.Clamp(preferences.UiWindowOpacityPercent, 10, 100);
        ImGui.SetNextItemWidth(180 * MaterialTheme.Metrics.Scale);
        if (SliderInt(T("Opacity (%)") + "###UiWindowOpacityPercentSettings", ref windowOpacityPercent, 10, 100, "%d%%", ImGuiSliderFlags.AlwaysClamp))
        { preferences.UiWindowOpacityPercent = windowOpacityPercent; changed = true; }
        var fadedOpacityPercent = Math.Clamp(preferences.UiFadedOpacityPercent, 10, 100);
        ImGui.SetNextItemWidth(180 * MaterialTheme.Metrics.Scale);
        if (SliderInt(T("Unfocused opacity (%)") + "###UiFadedOpacityPercentSettings", ref fadedOpacityPercent, 10, 100, "%d%%", ImGuiSliderFlags.AlwaysClamp))
        { preferences.UiFadedOpacityPercent = fadedOpacityPercent; changed = true; }
        ImGui.BeginDisabled(!autoFade);
        var delay = float.IsFinite(preferences.UiUnfocusedDelaySeconds) ? Math.Max(0, preferences.UiUnfocusedDelaySeconds) : 10;
        ImGui.SetNextItemWidth(180 * MaterialTheme.Metrics.Scale);
        if (InputFloat(T("Unfocused delay (seconds)") + "###UiUnfocusedDelaySecondsSettings", ref delay))
        { preferences.UiUnfocusedDelaySeconds = float.IsFinite(delay) ? Math.Max(0, delay) : 10; changed = true; }
        ImGui.EndDisabled();
        ImGui.EndDisabled();
        if (changed) preferences.Save();
    }
    private static bool Checkbox(string native, ref bool value)
    {
        var text = native.Split("##",2)[0];
        if (!MaterialText.RequiresShaping(text)) return ImGui.Checkbox(native,ref value);
        using var height = MaterialText.PushLineHeight(text);
        var gap = ImGui.GetStyle().ItemInnerSpacing;
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, new Vector2(gap.X+MaterialText.Measure(text).X-ImGui.CalcTextSize(text).X,gap.Y));
        ImGui.PushStyleColor(ImGuiCol.Text,Vector4.Zero);
        bool changed;
        try { changed=ImGui.Checkbox(native,ref value); }
        finally { ImGui.PopStyleColor(); ImGui.PopStyleVar(); }
        var min=ImGui.GetItemRectMin();var max=ImGui.GetItemRectMax();
        MaterialText.AddText(ImGui.GetWindowDrawList(),min+new Vector2(ImGui.GetFrameHeight()+gap.X,(max.Y-min.Y-MaterialText.Measure(text).Y)*.5f),ImGui.GetColorU32(ImGuiCol.Text),text);
        return changed;
    }
    private static bool SliderInt(string native, ref int value, int minimum, int maximum, string format, ImGuiSliderFlags flags)
    {
        using var caption = new FieldCaption(native);
        return ImGui.SliderInt(native,ref value,minimum,maximum,format,flags);
    }
    private static bool InputFloat(string native, ref float value)
    {
        using var caption = new FieldCaption(native);
        return ImGui.InputFloat(native,ref value);
    }
    private readonly ref struct FieldCaption
    {
        private readonly MaterialStyleScope height;
        private readonly string text;
        private readonly Vector2 min;
        private readonly float width;
        private readonly ImDrawListPtr list;
        private readonly Vector2 previousMax;
        private readonly bool shaped;
        internal FieldCaption(string native)
        {
            text=native.Split("##",2)[0];height=MaterialText.PushLineHeight(text);
            min=ImGui.GetCursorScreenPos();width=ImGui.CalcItemWidth();list=ImGui.GetWindowDrawList();
            previousMax=ImGuiP.GetCurrentWindow().DC.CursorMaxPos;shaped=MaterialText.RequiresShaping(text);
            if(shaped) list.PushClipRect(new Vector2(min.X,ImGui.GetWindowPos().Y),new Vector2(min.X+width,ImGui.GetWindowPos().Y+ImGui.GetWindowSize().Y),true);
        }
        public void Dispose()
        {
            if(shaped) list.PopClipRect();
            try
            {
                if(!shaped)return;
                var p=min+new Vector2(width+ImGui.GetStyle().ItemInnerSpacing.X,(ImGui.GetFrameHeight()-MaterialText.Measure(text).Y)*.5f);
                MaterialText.AddText(list,p,ImGui.GetColorU32(ImGuiCol.Text),text);
                var window=ImGuiP.GetCurrentWindow();var right=p.X+MaterialText.Measure(text).X;
                window.DC.CursorMaxPos=new Vector2(Math.Max(previousMax.X,right),window.DC.CursorMaxPos.Y);
                window.DC.CursorPosPrevLine=new Vector2(right,window.DC.CursorPosPrevLine.Y);
            }
            finally { height.Dispose(); }
        }
    }
    internal void ApplyWindowOpacity(MaterialWindowOpacity opacity, string name)
    {
        opacity.Apply(name, Math.Clamp(preferences.UiWindowOpacityPercent, 10, 100) / 100f,
            preferences.UiTransparencyEnabled, preferences.UiAutoFade, Math.Clamp(preferences.UiFadedOpacityPercent, 10, 100) / 100f,
            float.IsFinite(preferences.UiUnfocusedDelaySeconds) ? Math.Max(0, preferences.UiUnfocusedDelaySeconds) : 10);
    }

}
