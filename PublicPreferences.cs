using System.Text.Json.Nodes;
using Dalamud.Plugin;

namespace DhogNav.PublicShell;

// Preserve the existing Version/Payload envelope and all private fields.
internal sealed class PublicPreferences
{
    private readonly string path;
    private DateTime observedWrite = DateTime.MinValue;
    internal uint Accent { get; set; } = 0xA475FF;
    internal bool Compact { get; set; }
    internal string Language { get; set; } = "en";
    internal bool UiCompactVisibleOnMainWindow { get; set; } = true;
    internal bool UiLanguageVisibleOnMainWindow { get; set; } = true;
    internal bool UiTransparencyEnabled { get; set; } = true;
    internal int UiWindowOpacityPercent { get; set; } = 100;
    internal bool UiAutoFade { get; set; } = true;
    internal int UiFadedOpacityPercent { get; set; } = 50;
    internal float UiUnfocusedDelaySeconds { get; set; } = 10;
    internal PublicPreferences(IDalamudPluginInterface pi) { path = pi.ConfigFile.FullName; Reload(); }
    private JsonObject Read() => File.Exists(path)
        ? JsonNode.Parse(File.ReadAllText(path), documentOptions: new() { AllowTrailingCommas = true, CommentHandling = System.Text.Json.JsonCommentHandling.Skip }) as JsonObject
            ?? throw new InvalidDataException("DhogNav configuration must be an object.")
        : new JsonObject { ["Version"] = 1, ["Payload"] = new JsonObject() };
    internal void Reload()
    {
        var written = File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;
        if (written == observedWrite) return;
        var root = Read();
        var payload = root["Payload"] as JsonObject;
        Accent = payload?["UiAccentRgb"]?.GetValue<uint>() ?? 0xA475FF;
        Compact = payload?["UiCompact"]?.GetValue<bool>() ?? false;
        Language = payload?["UiLanguage"]?.GetValue<string>() ?? "en";
        UiCompactVisibleOnMainWindow = payload?["UiCompactVisibleOnMainWindow"]?.GetValue<bool>() ?? true;
        UiLanguageVisibleOnMainWindow = payload?["UiLanguageVisibleOnMainWindow"]?.GetValue<bool>() ?? true;
        UiTransparencyEnabled = payload?["UiTransparencyEnabled"]?.GetValue<bool>() ?? true;
        UiWindowOpacityPercent = payload?["UiWindowOpacityPercent"]?.GetValue<int>() ?? 100;
        UiAutoFade = payload?["UiAutoFade"]?.GetValue<bool>() ?? true;
        UiFadedOpacityPercent = payload?["UiFadedOpacityPercent"]?.GetValue<int>() ?? 50;
        UiUnfocusedDelaySeconds = payload?["UiUnfocusedDelaySeconds"]?.GetValue<float>() ?? 10;
        observedWrite = written;
    }
    internal void Save()
    {
        var root = Read();
        var payload = root["Payload"] as JsonObject;
        if (payload == null) { payload = new JsonObject(); root["Payload"] = payload; }
        payload["UiAccentRgb"] = Accent; payload["UiCompact"] = Compact; payload["UiLanguage"] = Language;
        payload["UiCompactVisibleOnMainWindow"] = UiCompactVisibleOnMainWindow;
        payload["UiLanguageVisibleOnMainWindow"] = UiLanguageVisibleOnMainWindow;
        payload["UiTransparencyEnabled"] = UiTransparencyEnabled;
        payload["UiWindowOpacityPercent"] = UiWindowOpacityPercent;
        payload["UiAutoFade"] = UiAutoFade;
        payload["UiFadedOpacityPercent"] = UiFadedOpacityPercent;
        payload["UiUnfocusedDelaySeconds"] = UiUnfocusedDelaySeconds;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, root.ToJsonString(new() { WriteIndented = true }), new System.Text.UTF8Encoding(false));
        observedWrite = File.GetLastWriteTimeUtc(path);
    }
}
