using System.Diagnostics;
using System.Text.Json;

namespace PrivateSwgLauncher;
internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--preview")
        {
            ApplicationConfiguration.Initialize(); using var form = new LauncherForm();
            form.Show(); Application.DoEvents(); using var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height)); bitmap.Save(args[1]); return;
        }
        if (args.Length == 2 && (args[0] == "--patch-offline" || args[0] == "--patch-online"))
        {
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
                var text = args[0] == "--patch-online" ? http.GetStringAsync(Patcher.ManifestUrl).GetAwaiter().GetResult() : Patcher.BundledManifest();
                Patcher.Apply(args[1], Patcher.Parse(text), message => File.AppendAllText(Path.Combine(args[1], "launcher-test.log"), message + "\n")); Environment.ExitCode = 0;
            }
            catch (Exception e) { File.WriteAllText(Path.Combine(args[1], "launcher-test-error.log"), e.ToString()); Environment.ExitCode = 1; }
            return;
        }
        ApplicationConfiguration.Initialize(); Application.Run(new LauncherForm());
    }
}
public sealed class LauncherForm : Form
{
    readonly TextBox folder = new() { Width = 480 };
    readonly TextBox output = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Height = 180, Width = 590 };
    readonly Button check = new() { Text = "Check updates", AutoSize = true };
    readonly Button update = new() { Text = "Update client", AutoSize = true };
    readonly Button play = new() { Text = "Update && Play", AutoSize = true };
    readonly string settings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrivateSWGEmuLauncher");
    string? cached;
    public LauncherForm()
    {
        Text = "SWGEmu Private Launcher"; ClientSize = new(650, 440); FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false; StartPosition = FormStartPosition.CenterScreen; Font = new Font("Segoe UI", 10);
        var layout = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), FlowDirection = FlowDirection.TopDown, WrapContents = false };
        layout.Controls.Add(new Label { Text = "SWGEmu — client updates", AutoSize = true, Font = new Font(Font.FontFamily, 16, FontStyle.Bold) });
        layout.Controls.Add(new Label { Text = "Choose the folder where your game is installed.", AutoSize = true });
        var row = new FlowLayoutPanel { Width = 600, Height = 42 }; row.Controls.Add(folder); var browse = new Button { Text = "Browse…", AutoSize = true }; row.Controls.Add(browse); layout.Controls.Add(row);
        var buttons = new FlowLayoutPanel { Width = 600, Height = 42 }; buttons.Controls.AddRange([check, update, play]); layout.Controls.Add(buttons); layout.Controls.Add(output);
        var link = new LinkLabel { Text = "Download a newer launcher", AutoSize = true }; link.LinkClicked += (_, _) => Process.Start(new ProcessStartInfo(Patcher.ReleasesUrl) { UseShellExecute = true }); layout.Controls.Add(link); Controls.Add(layout);
        Directory.CreateDirectory(settings); string config = Path.Combine(settings, "client-path.txt");
        if (File.Exists(config)) folder.Text = File.ReadAllText(config).Trim();
        else if (File.Exists(Path.Combine(AppContext.BaseDirectory, "SWGEmu.exe"))) folder.Text = AppContext.BaseDirectory;
        else if (Directory.Exists(@"C:\Games\SWGEmu")) folder.Text = @"C:\Games\SWGEmu";
        browse.Click += (_, _) => { using var dialog = new FolderBrowserDialog { Description = "Choose the folder containing SWGEmu.exe", InitialDirectory = folder.Text }; if (dialog.ShowDialog() == DialogResult.OK) folder.Text = dialog.SelectedPath; };
        check.Click += async (_, _) => await Run(false, false); update.Click += async (_, _) => await Run(true, false); play.Click += async (_, _) => await Run(true, true);
        Log("Ready. Choose your game folder, then select Update & Play.");
    }
    void Log(string text) { if (InvokeRequired) { BeginInvoke(() => Log(text)); return; } output.AppendText(text + Environment.NewLine); }
    async Task Run(bool apply, bool launch)
    {
        check.Enabled = update.Enabled = play.Enabled = false; folder.Enabled = false;
        try
        {
            Log("Checking public patch manifest…"); string json;
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            try { json = await http.GetStringAsync(Patcher.ManifestUrl); }
            catch (HttpRequestException) { json = cached ?? (File.Exists(Path.Combine(settings, "manifest.json")) ? File.ReadAllText(Path.Combine(settings, "manifest.json")) : Patcher.BundledManifest()); Log("Download unavailable. Using cached/bundled patch instructions."); }
            catch (TaskCanceledException) { json = cached ?? Patcher.BundledManifest(); Log("Download timed out. Using bundled patch instructions."); }
            var manifest = Patcher.Parse(json); cached = json; File.WriteAllText(Path.Combine(settings, "manifest.json"), json); Log($"Patch version: {manifest.Version}\r\n{manifest.Notes}");
            if (!apply) return;
            string client = folder.Text.Trim(); if (string.IsNullOrWhiteSpace(client)) throw new IOException("Choose the game folder first.");
            await Task.Run(() => Patcher.Apply(client, manifest, Log)); File.WriteAllText(Path.Combine(settings, "client-path.txt"), client);
            if (launch) { Process.Start(new ProcessStartInfo(Path.Combine(client, "SWGEmu.exe")) { WorkingDirectory = client, UseShellExecute = true }); Log("Game started."); }
        }
        catch (Exception e) { Log("Could not complete: " + e.Message); MessageBox.Show(this, e.Message, "Launcher", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        finally { check.Enabled = update.Enabled = play.Enabled = true; folder.Enabled = true; }
    }
}
