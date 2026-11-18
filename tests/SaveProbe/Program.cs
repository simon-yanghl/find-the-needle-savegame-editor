using System.Security.Cryptography;
using System.Text.Json;
using FtnEditor;

if (args.Length != 2) throw new ArgumentException("Usage: SaveProbe <read-only-source> <test-directory>");
string source = Path.GetFullPath(args[0]), testDir = Path.GetFullPath(args[1]);
if (source.StartsWith(testDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
    throw new ArgumentException("Source must be outside test directory.");
Directory.CreateDirectory(testDir);
byte[] original = File.ReadAllBytes(source);
string hash = Convert.ToHexString(SHA256.HashData(original));
byte[] plain = GdCrypt.Decrypt(original); // Includes MD5 verification.
var root = GdVar.FromFile(plain);
bool exactRoundTrip = plain.AsSpan().SequenceEqual(root.ToFile());
if (!exactRoundTrip) throw new Exception("Unedited Variant round-trip is not byte-exact.");
var report = new Dictionary<string, object>
{
    ["sourceSha256"] = hash,
    ["sourceBytes"] = original.Length,
    ["md5Verified"] = true,
    ["byteExactVariantRoundTrip"] = exactRoundTrip,
    ["sourceDebugUsed"] = root["state"]?["debug_used"]?.Bool,
    ["sourceDebugUsedType"] = root["state"]?["debug_used"]?.TypeName,
    ["sourceRunTimed"] = root["state"]?["run_timed"]?.ToString(),
};

// Normal fork save must force false. Never pass the live source path to Save().
string normalPath = Path.Combine(testDir, "normal-save.dat");
File.WriteAllBytes(normalPath, original);
var normal = SaveGame.Load(normalPath);
normal.Save(backup: false); // Avoid the application's global live backup directory.
if (SaveGame.Load(normalPath).State.Get("debug_used")?.Bool != false)
    throw new Exception("Normal Save() did not force debug_used=false.");
report["normalSaveForcesFalse"] = true;
if (!SaveGame.Load(normalPath).Root.ToFile().AsSpan().SequenceEqual(normal.Root.ToFile()))
    throw new Exception("Normal save reread differs from saved state.");
// Expected changes are SyncMeta() and debug_used=false only.
var expectedSave = new SaveGame { Root = GdVar.FromFile(plain) }; expectedSave.SyncMeta();
expectedSave.State.Set("debug_used", GdVar.NewBool(false));
if (!SaveGame.Load(normalPath).Root.ToFile().AsSpan().SequenceEqual(expectedSave.Root.ToFile()))
    throw new Exception("Unexpected change in normal save.");
report["normalSaveOnlyExpectedChanges"] = true;
normal.State.SetNum("money", normal.State.Num("money") + 1.25); normal.Dirty = true;
normal.Save(backup: false);
if (!SaveGame.Load(normalPath).Root.ToFile().AsSpan().SequenceEqual(normal.Root.ToFile()))
    throw new Exception("Edited save does not round-trip.");
report["editedMoneyRereadExact"] = true;

// Format-only experiment: use the unchanged codec, avoiding Save()'s deliberate policy.
// This does not establish how the game or leaderboard treats the resulting file.
string falsePath = Path.Combine(testDir, "debug-false-format-test.dat");
var experiment = GdVar.FromFile(plain);
experiment["state"].Dict.Set("debug_used", GdVar.NewBool(false));
byte[] expected = experiment.ToFile();
File.WriteAllBytes(falsePath, GdCrypt.Encrypt(expected));
var reread = SaveGame.Load(falsePath);
if (reread.State.Get("debug_used")?.Bool != false || !reread.Root.ToFile().AsSpan().SequenceEqual(expected))
    throw new Exception("debug_used=false format experiment failed.");
// Compare every serialized field, value and type after restoring the single changed flag.
if (root["state"].Dict.Get("debug_used") is { } prior) reread.State.Set("debug_used", prior);
else reread.State.Remove("debug_used");
if (!reread.Root.ToFile().AsSpan().SequenceEqual(plain))
    throw new Exception("Fields other than debug_used changed in format experiment.");
report["falseFlagReencryptedAndMd5Verified"] = true;
report["allOtherFieldsAndTypesByteExact"] = true;

// A subsequent save through the ordinary fork editor keeps false.
reread.Save(backup: false);
if (SaveGame.Load(falsePath).State.Get("debug_used")?.Bool != false)
    throw new Exception("Normal save did not preserve debug_used=false.");
report["subsequentNormalSaveKeepsFalse"] = true;
report["gameLoadTested"] = false;
report["leaderboardTested"] = false;
if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(source))) != hash)
    throw new Exception("Source changed during testing.");
report["sourceUnchanged"] = true;
string json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(Path.Combine(testDir, "report.json"), json);
Console.WriteLine(json);
File.WriteAllText(Path.Combine(testDir, "game-ids.json"), JsonSerializer.Serialize(new
{
    tech = root["tech"]?["ranks"]?.Dict?.Keys.ToArray(),
    buildings = root["buildings"]?.List?.Select(b => b["type"]?.Str).Distinct().ToArray()
}, new JsonSerializerOptions { WriteIndented = true }));
