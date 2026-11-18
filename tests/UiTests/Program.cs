using System.IO;
using System.Reflection;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using FtnEditor;

internal static class Program
{
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static void Assert(bool condition, string why) { if (!condition) throw new Exception(why); }
    static IEnumerable<T> Desc<T>(DependencyObject root) where T : DependencyObject
    {
        if (root is T match) yield return match;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            foreach (var child in Desc<T>(VisualTreeHelper.GetChild(root, i))) yield return child;
    }
    static void Pump() { var frame = new DispatcherFrame(); Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false)); Dispatcher.PushFrame(frame); }
    static void Shot(MainWindow w, string path)
    {
        w.UpdateLayout(); Pump();
        var bitmap = new RenderTargetBitmap((int)w.ActualWidth, (int)w.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(w); var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var file = File.Create(path); encoder.Save(file);
    }
    [STAThread]
    static int Main(string[] args)
    {
        string input = Path.GetFullPath(args[0]), dir = Path.GetFullPath(args[1]); Directory.CreateDirectory(dir);
        string saveDir = Path.Combine(dir, "saves"); Directory.CreateDirectory(saveDir);
        string copy = Path.Combine(saveDir, "slot_0.dat"); File.Copy(input, copy, true);
        typeof(SaveGame).GetField("saveDir", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, saveDir);
        Environment.SetEnvironmentVariable("FTN_OPEN", copy);
        Environment.SetEnvironmentVariable("FTN_SETTINGS", Path.Combine(dir, "settings.ini"));
        // Load the same production resources into a plain Application. Instantiating
        // App would queue its production OnStartup and create an unrelated second window.
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        var xaml = System.Xml.Linq.XDocument.Load(Path.Combine(AppContext.BaseDirectory, "AppResources.xaml"));
        System.Xml.Linq.XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        var resources = new System.Xml.Linq.XElement(ns + "ResourceDictionary",
            new System.Xml.Linq.XAttribute(System.Xml.Linq.XNamespace.Xmlns + "x", "http://schemas.microsoft.com/winfx/2006/xaml"),
            xaml.Root.Element(ns + "Application.Resources").Elements());
        app.Resources = (ResourceDictionary)System.Windows.Markup.XamlReader.Parse(resources.ToString());
        MainWindow w = null;
        try
        {
            Loc.Set("zh-CN", false); w = new MainWindow(); app.MainWindow = w; w.Show(); Pump();
            var originalSave = w.Save; Assert(originalSave != null, "test copy not loaded");
            var name = Desc<TextBox>(w).Single(t => t.Width == 220);
            w.Activate(); name.Focus(); Keyboard.Focus(name); Pump();
            Assert(name.IsKeyboardFocused, "name edit focus was not established");
            name.Text = "中文测试：未保存的名称";
            typeof(MainWindow).GetMethod("SwitchLang", Private).Invoke(w, new object[] { "en" }); Pump();
            w = (MainWindow)app.MainWindow;
            Assert(ReferenceEquals(w.Save, originalSave), "language change replaced save object");
            Assert(w.Save.Meta.Str("name") == "中文测试：未保存的名称" && w.Save.Dirty, "pending text edit lost: name=" + w.Save.Meta.Str("name") + ", dirty=" + w.Save.Dirty);
            var money = Desc<TextBox>(w).Single(t => t.Width == 280);
            w.Activate(); money.Focus(); Keyboard.Focus(money); Pump(); Assert(money.IsKeyboardFocused, "number edit focus failed");
            money.Text = "123456.75";
            typeof(MainWindow).GetMethod("SwitchLang", Private).Invoke(w, new object[] { "de" }); Pump();
            w = (MainWindow)app.MainWindow;
            Assert(w.Save.State.Num("money") == 123456.75 && w.Save.Dirty, "pending numeric edit lost");
            foreach (string lang in new[] { "zh-CN", "en", "de", "zh-CN" })
            {
                typeof(MainWindow).GetMethod("SwitchLang", Private).Invoke(w, new object[] { lang }); Pump();
                w = (MainWindow)app.MainWindow;
                Assert(ReferenceEquals(w.Save, originalSave) && w.Save.Dirty && w.Save.State.Num("money") == 123456.75, "unsaved edits lost in " + lang);
            }
            Console.WriteLine("PASS EN/DE/中文: focused text and number edits, dirty state and Save instance retained");
            w.Save.State.SetNum("money", 123.123456789); w.Refresh(); Pump();
            money = Desc<TextBox>(w).Single(t => t.Width == 280);
            w.Activate(); money.Focus(); Keyboard.Focus(money); Pump(); Assert(money.IsKeyboardFocused, "unchanged number focus failed");
            typeof(MainWindow).GetMethod("SwitchLang", Private).Invoke(w, new object[] { "en" }); Pump(); w = (MainWindow)app.MainWindow;
            Assert(w.Save.State.Num("money") == 123.123456789, "unchanged focused numeric field rounded the stored value");
            Console.WriteLine("PASS language change preserves undisplayed numeric precision when input is unchanged");
            typeof(MainWindow).GetMethod("SwitchLang", Private).Invoke(w, new object[] { "zh-CN" }); Pump(); w = (MainWindow)app.MainWindow;
            var pages = ((IEnumerable)typeof(MainWindow).GetField("pages", Private).GetValue(w)).Cast<object>().ToArray();
            var navigate = typeof(MainWindow).GetMethod("Navigate", Private);
            foreach (var page in pages)
            {
                navigate.Invoke(w, new[] { page }); Pump();
                string key = (string)page.GetType().GetProperty("Key").GetValue(page);
                Assert(!Desc<TextBlock>(w).Any(t => t.Text == Loc.L("Dieser Bereich konnte nicht aufgebaut werden")), "page failed: " + key);
            }
            Console.WriteLine("PASS all pages built after repeated language changes");
            var techPage = pages.Single(p => (string)p.GetType().GetProperty("Key").GetValue(p) == "tech");
            navigate.Invoke(w, new[] { techPage }); Pump();
            var search = Desc<TextBox>(w).First(t => t.Width != 220 && t.Text == ""); search.Text = "传送带"; Pump();
            Assert(Desc<TextBlock>(w).Any(t => t.IsVisible && t.Text == "传送带"), "Chinese tech search failed");
            Console.WriteLine("PASS Chinese technology search");
            search.Text = "";
            w.Width = 1060; w.Height = 640; Pump();
            var maxAction = Desc<Button>(w).Single(b => b.Content as string == Loc.L("Alles auf MAX") || Desc<TextBlock>(b).Any(t => t.Text == Loc.L("Alles auf MAX")));
            var bounds = maxAction.TransformToAncestor(w).TransformBounds(new Rect(0, 0, maxAction.ActualWidth, maxAction.ActualHeight));
            Assert(bounds.Right <= w.ActualWidth, "technology MAX action clipped outside minimum-width window");
            Shot(w, Path.Combine(dir, "tech-1060x640.png"));
            w.WindowState = WindowState.Maximized; Pump(); Shot(w, Path.Combine(dir, "tech-maximized.png"));
            w.WindowState = WindowState.Normal; w.Width = 1320; w.Height = 840;
            // Keep current page and edited values across a final language change.
            typeof(MainWindow).GetMethod("SwitchLang", Private).Invoke(w, new object[] { "en" }); Pump(); w = (MainWindow)app.MainWindow;
            object current = typeof(MainWindow).GetField("current", Private).GetValue(w);
            Assert((string)current.GetType().GetProperty("Key").GetValue(current) == "tech", "current page lost");
            Console.WriteLine("PASS current page retained and minimum/maximized windows rendered");
            var critical = new Dictionary<string, string> { ["tech"] = "Alles auf MAX", ["depot"] = "Depot leeren", ["raw"] = ".bin importieren", ["backups"] = "Backup jetzt", ["buildings"] = "Alle Generatoren auftanken" };
            foreach (string lang in new[] { "zh-CN", "en", "de" })
            {
                typeof(MainWindow).GetMethod("SwitchLang", Private).Invoke(w, new object[] { lang }); Pump(); w = (MainWindow)app.MainWindow;
                w.Width = 1060; w.Height = 640;
                var currentPages = ((IEnumerable)typeof(MainWindow).GetField("pages", Private).GetValue(w)).Cast<object>();
                foreach (var page in currentPages)
                {
                    string key = (string)page.GetType().GetProperty("Key").GetValue(page);
                    if (!critical.TryGetValue(key, out string label)) continue;
                    navigate.Invoke(w, new[] { page }); Pump();
                    var action = Desc<Button>(w).Single(b => b.Content as string == Loc.L(label) || Desc<TextBlock>(b).Any(t => t.Text == Loc.L(label)));
                    var rect = action.TransformToAncestor(w).TransformBounds(new Rect(0, 0, action.ActualWidth, action.ActualHeight));
                    Assert(rect.Left >= 0 && rect.Right <= w.ActualWidth, lang + " minimum-width toolbar clipped: " + key);
                    if (lang == "zh-CN") Shot(w, Path.Combine(dir, key + "-minimum.png"));
                }
            }
            Console.WriteLine("PASS minimum-width critical toolbar controls in all three languages");
            typeof(MainWindow).GetMethod("SwitchLang", Private).Invoke(w, new object[] { "zh-CN" }); Pump(); w = (MainWindow)app.MainWindow;
            var depotPage = ((IEnumerable)typeof(MainWindow).GetField("pages", Private).GetValue(w)).Cast<object>().Single(p => (string)p.GetType().GetProperty("Key").GetValue(p) == "depot");
            navigate.Invoke(w, new[] { depotPage }); Pump();
            int initialHistory = DepotHistory.Count(w.Save);
            Assert(initialHistory > 0, "depot history fixture needs existing records");
            var expectedHistoryRoot = GdVar.FromFile(w.Save.Root.ToFile());
            var expectedHistoryLog = expectedHistoryRoot["state"]["purchase_log"].List;
            int lastDepot = expectedHistoryLog.FindLastIndex(e => e.List is { Count: >= 5 } l && l[1].Str == "build" && l[2].Str == "depot"); expectedHistoryLog.RemoveAt(lastDepot);
            var historyInput = Desc<TextBox>(w).Single(t => System.Windows.Automation.AutomationProperties.GetAutomationId(t) == "DepotHistoryKeepCount");
            var historyApply = Desc<Button>(w).Single(t => System.Windows.Automation.AutomationProperties.GetAutomationId(t) == "DepotHistoryApply");
            w.Activate(); historyInput.Focus(); Keyboard.Focus(historyInput); Pump(); Assert(historyInput.IsKeyboardFocused, "history edit focus failed");
            historyInput.Text = "NaN";
            bool invalidDialog = false;
            var invalidTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            invalidTimer.Tick += (_, _) => { foreach (var dialog in app.Windows.OfType<Window>().Where(x => x != w).ToArray()) { invalidDialog = true; dialog.Close(); } invalidTimer.Stop(); };
            invalidTimer.Start(); historyApply.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); Pump(); invalidTimer.Stop();
            Assert(!invalidDialog && DepotHistory.Count(w.Save) == initialHistory, "NaN history input should reset without confirmation or mutation");
            Console.WriteLine("PASS non-finite history input rejected without confirmation or mutation");
            w.Activate(); historyInput.Focus(); Keyboard.Focus(historyInput); Pump();
            historyInput.Text = (initialHistory - 1).ToString();
            var confirmTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            confirmTimer.Tick += (_, _) =>
            {
                var confirm = app.Windows.OfType<Window>().Where(dialog => dialog != w).SelectMany(dialog => Desc<Button>(dialog)).FirstOrDefault(b => b.Content as string == Loc.L("Anwenden"));
                if (confirm != null) { confirmTimer.Stop(); confirm.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); }
            };
            confirmTimer.Start(); historyApply.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); Pump(); confirmTimer.Stop();
            Assert(DepotHistory.Count(w.Save) == initialHistory - 1 && w.Save.Dirty, "history apply did not commit pending input");
            Assert(w.Save.Root.ToFile().AsSpan().SequenceEqual(expectedHistoryRoot.ToFile()), "history UI changed buildings, money or unrelated fields");
            typeof(MainWindow).GetMethod("SwitchLang", Private).Invoke(w, new object[] { "en" }); Pump(); w = (MainWindow)app.MainWindow;
            Assert(DepotHistory.Count(w.Save) == initialHistory - 1 && w.Save.Dirty, "history edit lost on language switch");
            byte[] beforeHistorySave = File.ReadAllBytes(copy); string historyBackup = w.Save.Save();
            Assert(File.ReadAllBytes(historyBackup).AsSpan().SequenceEqual(beforeHistorySave), "history save backup changed original bytes");
            Assert(SaveGame.Load(copy).Root.ToFile().AsSpan().SequenceEqual(w.Save.Root.ToFile()), "history save/reread not exact");
            Assert(SaveGame.Load(copy).State.Get("debug_used") is { Type: GdVar.BOOL, Bool: false }, "history save lost false policy");
            Console.WriteLine("PASS depot history UI trims latest record only, preserves buildings/money, survives language switch and saves with exact backup/reread");
            // Real WPF dialog layout, auto-close with dispatcher; no disk action occurs.
            Loc.Set("zh-CN", false);
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
            timer.Tick += (_, _) => { foreach (var dialog in app.Windows.OfType<Window>().Where(x => x != w).ToArray()) { dialog.UpdateLayout(); var bitmap = new RenderTargetBitmap((int)dialog.ActualWidth, (int)dialog.ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(dialog); var e = new PngBitmapEncoder(); e.Frames.Add(BitmapFrame.Create(bitmap)); using (var f = File.Create(Path.Combine(dir, "discard-dialog.png"))) e.Save(f); dialog.Close(); } timer.Stop(); };
            timer.Start(); Dlg.Confirm("Ungespeicherte Änderungen", "Im aktuellen Spielstand gibt es Änderungen, die noch nicht gespeichert sind. Verwerfen?", "Verwerfen", true);
            Console.WriteLine("PASS Chinese discard dialog rendered");
            return 0;
        }
        catch (Exception ex) { Console.WriteLine("FAIL " + ex); return 1; }
        finally { foreach (var open in app.Windows.OfType<MainWindow>().ToArray()) { if (open.Save != null) open.Save.Dirty = false; open.Close(); } app.Shutdown(); }
    }
}
