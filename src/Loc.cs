using System.IO;

namespace FtnEditor;

/// <summary>
/// UI language. German texts in the code are the keys; English and Simplified Chinese
/// come from <see cref="LocEn.Map"/> and <see cref="LocZh.Map"/>.
/// The UI helpers (T, Caps, Btn, Switch, Seg, Pill, Tip, Combo) translate on their own, so only
/// texts with numbers in them call L(format, args) explicitly.
/// </summary>
public static class Loc
{
    public static string Language { get; private set; } = "zh-CN";
    public static bool De => Language == "de";
    public static bool Zh => Language == "zh-CN";
    static string SettingsFile => Environment.GetEnvironmentVariable("FTN_SETTINGS") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FindTheNeedleEditor", "settings.ini");

    public static void Load()
    {
        Language = "zh-CN";
        try
        {
            if (File.Exists(SettingsFile))
                foreach (string line in File.ReadAllLines(SettingsFile))
                    if (line.Trim().StartsWith("lang=", StringComparison.OrdinalIgnoreCase))
                        Language = Normalize(line.Trim()[5..]);
        }
        catch { }
        ApplyCulture();
    }
    static void ApplyCulture()
    {
        var c = System.Globalization.CultureInfo.GetCultureInfo(De ? "de-DE" : Zh ? "zh-CN" : "en-US");
        System.Globalization.CultureInfo.CurrentCulture = c; System.Globalization.CultureInfo.DefaultThreadCurrentCulture = c;
        System.Globalization.CultureInfo.CurrentUICulture = c; System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = c;
    }
    static string Normalize(string code) => code?.Trim().ToLowerInvariant() switch { "de" or "de-de" => "de", "en" or "en-us" => "en", _ => "zh-CN" };
    public static void Set(string code, bool persist = true)
    {
        Language = Normalize(code); ApplyCulture();
        if (!persist) return;
        try { Directory.CreateDirectory(Path.GetDirectoryName(SettingsFile)); File.WriteAllText(SettingsFile, "lang=" + Language + Environment.NewLine); } catch { }
    }

    static readonly HashSet<string> missing = new();
    static readonly string missLog = Environment.GetEnvironmentVariable("FTN_I18N_MISSING");

    public static string L(string de)
    {
        if (De || string.IsNullOrEmpty(de)) return de;
        if (Zh && LocZh.Map.TryGetValue(de, out var zh)) return zh;
        if (LocEn.Map.TryGetValue(de, out var en))
        {
            if (Zh) LogMissing(de);
            return en;
        }
        LogMissing(de);
        return de;
    }
    static void LogMissing(string de)
    {
        if (missLog != null && de.Any(char.IsLetter) && missing.Add(de)) try { File.AppendAllText(missLog, de + Environment.NewLine); } catch { }
    }
    public static string L(string de, params object[] args) => string.Format(L(de), args);

    public static string Date(DateTime d) => d.ToString(De ? "dd.MM.yyyy HH:mm" : "yyyy-MM-dd HH:mm");
}
