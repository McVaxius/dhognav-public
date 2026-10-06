using Dalamud;
using AethertekUI;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;

#if DHOGNAV_PRIVATE_UI
namespace DhogNav.PrivateUi;
#else
namespace DhogNav.PublicShell;
#endif

internal sealed class ManagedUiFonts : IDisposable
{
    private readonly IFontAtlas atlas;
    internal MaterialTextRenderer? ShapedText { get; set; }
    private static readonly MaterialWindowFold statusMotion = new();
    private static readonly MaterialWindowDecorations statusDecorations = new();
    private IFontHandle[] handles = [];
    private string language = "";
    private string[] required = [];
    private int generation;
    private int checkedGeneration = -1;
    private Exception? glyphError;
    internal ManagedUiFonts(IUiBuilder builder, string label) => atlas = builder.CreateFontAtlas(FontAtlasAutoRebuildMode.Async, true, label);
    internal Exception? Error => glyphError ?? handles.FirstOrDefault(h => h.LoadException != null)?.LoadException;
    internal void Invalidate() => language = "";
    internal void Prepare(string selected, IEnumerable<string> strings)
    {
        if (selected == language) return;
        using var suppress = atlas.SuppressAutoRebuild();
        foreach (var handle in handles) { handle.ImFontChanged -= Changed; handle.Dispose(); }
        language = selected;
        required = strings.Concat(["English", "Deutsch", "Français", "Español", "Italiano", "Русский", "日本語", "한국어", "简体中文", "繁體中文", "Português (Brasil)", "Tiếng Việt", "Bahasa Indonesia", "Polski", "Türkçe", "हिन्दी", "♡", "—", "…"]).Distinct().ToArray();
        var ranges = required.SelectMany(text => MaterialText.NativeGlyphText(text).EnumerateRunes())
            .Where(rune => rune.Value <= ushort.MaxValue && !Rune.IsControl(rune)).Select(rune => (char)rune.Value)
            .Concat(Enumerable.Range(0x20, 0x250 - 0x20).Select(i => (char)i))
            .Concat(Enumerable.Range(0x400, 0x130).Select(i => (char)i)).ToGlyphRange();
        glyphError = null; checkedGeneration = -1;
        handles = UiStyle.FontSizes.Select((pointSize, index) => atlas.NewDelegateFontHandle(step => step.OnPreBuild(build =>
        {
            var size = pointSize * 4 / 3;
            build.Font = build.AddFontFromFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), UiStyle.FontFiles[index]),
                new SafeFontConfig { SizePx = size, GlyphRanges = ranges });
            build.AddFontFromFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "seguisym.ttf"),
                new SafeFontConfig { SizePx = size, MergeFont = build.Font, GlyphRanges = ranges });
            // Verified bundled TTC: JP=0, KR=1, SC=2, TC=3. Selected face owns shared ideographs.
            foreach (var locale in new[] { "ja", "ko", "zh-Hans", "zh-Hant" }.OrderBy(code => code == selected ? 0 : 1))
                build.AddDalamudAssetFont(DalamudAsset.NotoSansCjkRegular,
                    new SafeFontConfig { SizePx = size, MergeFont = build.Font, GlyphRanges = ranges,
                        FontNo = locale switch { "ko" => 1, "zh-Hans" => 2, "zh-Hant" => 3, _ => 0 } });
            build.AttachExtraGlyphsForDalamudLanguage(new SafeFontConfig { SizePx = size, MergeFont = build.Font });
            build.AddGameSymbol(new SafeFontConfig { SizePx = size, MergeFont = build.Font });
        }))).ToArray();
        foreach (var handle in handles) handle.ImFontChanged += Changed;
    }
    private void Changed(IFontHandle handle, ILockedImFont font) => System.Threading.Interlocked.Increment(ref generation);
    internal unsafe bool Ready()
    {
        if (handles.Length != UiStyle.FontSizes.Length || handles.Any(h => !h.Available || h.LoadException != null) || glyphError != null) return false;
        var current = System.Threading.Volatile.Read(ref generation);
        if (current == checkedGeneration) return true;
        try
        {
            for (var index = 0; index < handles.Length; index++)
            {
                ShapedText?.CheckGlyphs(required, UiStyle.FontSizes[index]*4/3*ImGui.GetIO().FontGlobalScale);
                using var font = handles[index].Lock();
                foreach (var rune in required.SelectMany(text => MaterialText.NativeGlyphText(text).EnumerateRunes()).Where(rune => !Rune.IsControl(rune)).Distinct())
                    if (rune.Value>ushort.MaxValue || ImGui.FindGlyphNoFallback(font.ImFont, (ushort)rune.Value).Handle == null)
                        throw new InvalidOperationException("Required DhogNav UI glyph missing: U+" + rune.Value.ToString("X4") + " in " + (UiFontRole)index);
            }
            checkedGeneration = current;
            return current == System.Threading.Volatile.Read(ref generation);
        }
        catch (Exception error) { glyphError = error; return false; }
    }
    internal IDisposable Push(UiFontRole role)
    {
        if (!Ready()) throw new InvalidOperationException("DhogNav UI fonts are not ready.", Error);
        return handles[(int)role].Push();
    }
    internal static unsafe void DrawStatus(bool loading, string translated)
    {
        // Before managed fonts are ready, paint only glyphs present in the host font.
        var covered = MaterialText.NativeGlyphText(translated).EnumerateRunes().Where(rune => !Rune.IsControl(rune)).All(rune => rune.Value<=ushort.MaxValue && ImGui.FindGlyphNoFallback(ImGui.GetFont(), (ushort)rune.Value).Handle != null);
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(450 * ImGui.GetIO().FontGlobalScale, 0));
        statusMotion.PreDraw("DhogNav##FontStatus", null, null, reducedMotion: false, prepareDecorations: statusDecorations.Prepare);
        if (ImGui.Begin("DhogNav##FontStatus", ImGuiWindowFlags.AlwaysAutoResize))
        {
            statusDecorations.Paint();
            MaterialText.TextWrapped(covered ? translated : loading ? "Preparing DhogNav interface fonts..." : "DhogNav interface fonts are unavailable. See the Dalamud log.");
        }
        ImGui.End();
        statusDecorations.Paint();
        statusMotion.PostDraw();
    }
    public void Dispose()
    {
        foreach (var handle in handles) { handle.ImFontChanged -= Changed; handle.Dispose(); }
        handles = []; atlas.Dispose();
    }
}
