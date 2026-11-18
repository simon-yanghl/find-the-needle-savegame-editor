using FtnEditor;
using System.Reflection;
using System.Security.Cryptography;

string dir = Path.GetFullPath(args.Length == 0 ? ".work/core-tests" : args[0]);
Directory.CreateDirectory(dir);
int failures = 0;
void Test(string name, Action run) { try { run(); Console.WriteLine("PASS " + name); } catch (Exception ex) { failures++; Console.WriteLine("FAIL " + name + ": " + ex.Message); } }
void Assert(bool ok, string why) { if (!ok) throw new Exception(why); }
typeof(SaveGame).GetField("saveDir", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, Path.Combine(dir, "saves"));
Directory.CreateDirectory(SaveGame.SaveDir);
foreach (bool? flag in new bool?[] { true, false, null })
    Test("save forces false from " + (flag?.ToString() ?? "missing"), () =>
    {
        var root = GdVar.NewDict(); var state = GdVar.NewDict(); root.Dict.Set("state", state);
        state.Dict.Set("money", GdVar.NewFloat(123.75));
        if (flag.HasValue) state.Dict.Set("debug_used", GdVar.NewBool(flag.Value));
        var unknown = GdVar.NewDict(); unknown.Dict.Set("huge", GdVar.NewInt(9007199254740993));
        unknown.Dict.Set("float32", new GdVar { Header = GdVar.FLOAT, Value = (double)1.25f });
        unknown.Dict.Set("int32", new GdVar { Header = GdVar.INT, Value = (long)123 });
        root.Dict.Set("future_unknown", unknown);
        string path = Path.Combine(SaveGame.SaveDir, "slot_" + (flag?.ToString() ?? "missing") + ".dat");
        byte[] original = GdCrypt.Encrypt(root.ToFile()); File.WriteAllBytes(path, original);
        var s = SaveGame.Load(path); s.Dirty = true; string backup = s.Save();
        Assert(File.ReadAllBytes(backup).AsSpan().SequenceEqual(original), "backup must contain original encrypted bytes");
        var re = SaveGame.Load(path);
        Assert(re.State.Get("debug_used") is { Type: GdVar.BOOL, Bool: false }, "expected bool false after Save()");
        Assert(!s.Dirty, "save should clear dirty flag");
        re.State.Remove("debug_used"); state.Dict.Remove("debug_used");
        Assert(re.Root.ToFile().AsSpan().SequenceEqual(root.ToFile()), "unknown fields, number bits and types changed");
        File.Copy(backup, path, true);
        Assert(File.ReadAllBytes(path).AsSpan().SequenceEqual(original), "backup restore was not exact");
    });
Test("encrypted checksum rejects tampering", () =>
{
    byte[] b = GdCrypt.Encrypt(GdVar.NewDict().ToFile()); b[4] ^= 1;
    try { GdCrypt.Decrypt(b); throw new Exception("corruption accepted"); } catch (InvalidDataException) { }
});
Test("three languages, persisted setting and old settings", () =>
{
    string settings = Path.Combine(dir, "settings.ini"); Environment.SetEnvironmentVariable("FTN_SETTINGS", settings);
    if (File.Exists(settings)) File.Delete(settings);
    Loc.Load(); Assert(Loc.L("Speichern") == "保存", "first launch should be Chinese");
    var set = typeof(Loc).GetMethod("Set", new[] { typeof(string), typeof(bool) });
    Assert(set != null, "explicit language code API missing");
    set.Invoke(null, new object[] { "en", true }); Loc.Load(); Assert(Loc.L("Speichern") == "Save", "English setting not retained");
    File.WriteAllText(settings, "lang=de\n"); Loc.Load(); Assert(Loc.L("Speichern") == "Speichern", "legacy German setting broken");
    set.Invoke(null, new object[] { "zh-CN", true }); Loc.Load(); Assert(Loc.L("Speichern") == "保存", "Chinese setting not retained");
    set.Invoke(null, new object[] { "en", false }); Loc.Load(); Assert(Loc.L("Speichern") == "保存", "CLI override persisted unexpectedly");
});
Test("Chinese dictionary covers English keys and preserves format arguments", () =>
{
    string Slots(string text) => string.Join(",", System.Text.RegularExpressions.Regex.Matches(text, @"\{(\d+)(?:[^}]*)\}").Select(m => m.Groups[1].Value).Order());
    foreach (var (key, en) in LocEn.Map)
    {
        Assert(LocZh.Map.TryGetValue(key, out var zh), "untranslated key: " + key);
        Assert(Slots(key) == Slots(zh), "format arguments differ: " + key);
        _ = System.Text.CompositeFormat.Parse(zh);
    }
});
Test("depot history trims latest matching records only", () =>
{
    var helper = typeof(SaveGame).Assembly.GetType("FtnEditor.DepotHistory");
    Assert(helper != null, "DepotHistory feature missing");
    var count = helper.GetMethod("Count"); var trim = helper.GetMethod("Trim");
    GdVar Entry(string kind, string id, int index)
    {
        var entry = GdVar.NewArray();
        entry.List.AddRange(new[] { GdVar.NewFloat(index + .125), GdVar.NewString(kind), new GdVar { Header = GdVar.STRING_NAME, Value = id }, GdVar.NewInt(0), GdVar.NewFloat(750 + index), GdVar.NewInt(9007199254740993) });
        return entry;
    }
    var root = GdVar.NewDict(); var state = GdVar.NewDict(); var log = GdVar.NewArray();
    log.List.AddRange(new[] { Entry("tool", "depot", 0), Entry("build", "depot", 1), Entry("build", "depot_drone", 2), Entry("build", "depot", 3), Entry("card", "depot", 4), Entry("build", "depot", 5), GdVar.NewString("unknown future record") });
    state.Dict.Set("purchase_log", log); state.Dict.Set("money", GdVar.NewFloat(123.123456789)); root.Dict.Set("state", state);
    var buildings = GdVar.NewArray(); var depot = GdVar.NewDict(); depot.Dict.Set("type", GdVar.NewString("depot")); buildings.List.Add(depot); root.Dict.Set("buildings", buildings);
    var save = new SaveGame { Root = root }; var expected = GdVar.FromFile(root.ToFile());
    Assert((int)count.Invoke(null, new object[] { save }) == 3, "count must ignore unrelated/malformed records");
    Assert((int)trim.Invoke(null, new object[] { save, 1 }) == 2 && save.Dirty, "trim should remove two and mark dirty");
    expected["state"]["purchase_log"].List.RemoveAt(5); expected["state"]["purchase_log"].List.RemoveAt(3);
    Assert(save.Root.ToFile().AsSpan().SequenceEqual(expected.ToFile()), "trim changed unrelated data, number types, entities or order");
    byte[] after = save.Root.ToFile(); save.Dirty = false;
    Assert((int)trim.Invoke(null, new object[] { save, 1 }) == 0 && !save.Dirty, "unchanged count must be a no-op");
    foreach (int invalid in new[] { -1, 2 })
    {
        try { trim.Invoke(null, new object[] { save, invalid }); throw new Exception("invalid count accepted"); }
        catch (TargetInvocationException ex) when (ex.InnerException is ArgumentOutOfRangeException) { }
        Assert(save.Root.ToFile().AsSpan().SequenceEqual(after) && !save.Dirty, "invalid request mutated save");
    }
    Assert((int)trim.Invoke(null, new object[] { save, 0 }) == 1 && (int)count.Invoke(null, new object[] { save }) == 0, "zero should remove all remaining depot records");
    save.State.Remove("purchase_log"); save.Dirty = false; byte[] absent = save.Root.ToFile();
    Assert((int)count.Invoke(null, new object[] { save }) == 0 && (int)trim.Invoke(null, new object[] { save, 0 }) == 0, "missing log should be a no-op");
    Assert(save.Root.ToFile().AsSpan().SequenceEqual(absent) && !save.Dirty, "missing log was created or dirtied");
});
Environment.ExitCode = failures == 0 ? 0 : 1;
