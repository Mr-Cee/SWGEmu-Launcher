using System.Diagnostics;
using System.Text.Json;

namespace PrivateSwgLauncher;
internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--apply-launcher-update")
        {
            try { LauncherUpdater.Apply(args[1]); }
            catch (Exception e) { MessageBox.Show("Launcher update could not finish: " + e.Message + "\nYour previous launcher is available in its original folder.", "Launcher update", MessageBoxButtons.OK, MessageBoxIcon.Warning); Environment.ExitCode = 1; }
            return;
        }
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
        ApplicationConfiguration.Initialize(); Application.Run(new LauncherForm(args.Length == 2 && args[0] == "--launcher-updated" ? args[1] : null));
    }
}
public sealed class LauncherForm : Form
{
    readonly TextBox folder = new() { Width = 480 };
    readonly TextBox output = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Height = 180, Width = 590 };
    readonly Button check = new() { Text = "Check updates", AutoSize = true };
    readonly Button update = new() { Text = "Update client", AutoSize = true };
    readonly Button launcherUpdate = new() { Text = "Update launcher", AutoSize = true };
    readonly Button play = new() { Text = "Update && Play", AutoSize = true };
    readonly string settings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrivateSWGEmuLauncher");
    readonly CheckBox remember = new() { Text = "Remember account name (password is never saved)", AutoSize = true };
    readonly TextBox account = new() { Width = 300, MaxLength = 64 };
    string AccountFile => Path.Combine(settings, "account-" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(Path.GetFullPath(folder.Text.Trim()).ToUpperInvariant()))) + ".txt");
    void LoadAccount() { account.Text = ""; remember.Checked = false; try { if (File.Exists(AccountFile)) { account.Text = File.ReadAllText(AccountFile); remember.Checked = true; } } catch { } }

    public LauncherForm(string? updateJob = null)
    {
        Text = "SWGEmu Private Launcher"; ClientSize = new(650, 562); FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false; StartPosition = FormStartPosition.CenterScreen; Font = new Font("Segoe UI", 10);
        var layout = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), FlowDirection = FlowDirection.TopDown, WrapContents = false };
        layout.Controls.Add(new Label { Text = "SWGEmu — client updates", AutoSize = true, Font = new Font(Font.FontFamily, 16, FontStyle.Bold) });
        layout.Controls.Add(new Label { Text = "Choose the folder where your game is installed.", AutoSize = true });
        var row = new FlowLayoutPanel { Width = 600, Height = 42 }; row.Controls.Add(folder); var browse = new Button { Text = "Browse…", AutoSize = true }; row.Controls.Add(browse); layout.Controls.Add(row);
        layout.Controls.Add(remember);
        var accountRow = new FlowLayoutPanel { Width = 600, Height = 38 };
        accountRow.Controls.Add(new Label { Text = "Account name", AutoSize = true, Padding = new Padding(0, 6, 8, 0) }); accountRow.Controls.Add(account); layout.Controls.Add(accountRow);
        var buttons = new FlowLayoutPanel { Width = 600, Height = 42 }; buttons.Controls.AddRange([check, update, play]); layout.Controls.Add(buttons);
        var launcherRow = new FlowLayoutPanel { Width = 600, Height = 42 }; launcherRow.Controls.Add(launcherUpdate); launcherRow.Controls.Add(new Label { Text = "Launcher v" + LauncherUpdater.CurrentVersion.ToString(3), AutoSize = true, Padding = new Padding(6, 8, 0, 0) }); layout.Controls.Add(launcherRow); layout.Controls.Add(output);
        layout.Controls.Add(new Label { Text = "Launcher updates install in place and reopen this window.", AutoSize = true }); Controls.Add(layout);
        Directory.CreateDirectory(settings); string config = Path.Combine(settings, "client-path.txt");
        if (File.Exists(config)) folder.Text = File.ReadAllText(config).Trim();
        else if (File.Exists(Path.Combine(AppContext.BaseDirectory, "SWGEmu.exe"))) folder.Text = AppContext.BaseDirectory;
        else if (Directory.Exists(@"C:\Games\SWGEmu")) folder.Text = @"C:\Games\SWGEmu";
        LoadAccount(); folder.TextChanged += (_, _) => LoadAccount();
        browse.Click += (_, _) => { using var dialog = new FolderBrowserDialog { Description = "Choose the folder containing SWGEmu.exe", InitialDirectory = folder.Text }; if (dialog.ShowDialog() == DialogResult.OK) folder.Text = dialog.SelectedPath; };
        launcherUpdate.Click += async (_, _) => await UpdateLauncher();
        if (updateJob is not null) Shown += async (_, _) => { Log("Launcher updated successfully to " + LauncherUpdater.CurrentVersion.ToString(3) + "."); await LauncherUpdater.CleanupAsync(updateJob); };
        check.Click += async (_, _) => await Run(false, false); update.Click += async (_, _) => await Run(true, false); play.Click += async (_, _) => await Run(true, true);
        Log("Ready. Update & Play opens a new game window. Client updates require all game windows closed.");
    }
    void Log(string text) { if (InvokeRequired) { BeginInvoke(() => Log(text)); return; } output.AppendText(text + Environment.NewLine); }
    async Task UpdateLauncher()
    {
        check.Enabled = launcherUpdate.Enabled = update.Enabled = play.Enabled = false;
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            string? job = await LauncherUpdater.PrepareAsync(http, Environment.ProcessPath!, Log);
            if (job is null || IsDisposed) return;
            LauncherUpdater.StartInstall(job);
            Close();
        }
        catch (Exception e) { if (!IsDisposed) { Log("Launcher update failed: " + e.Message); MessageBox.Show(this, e.Message, "Launcher update", MessageBoxButtons.OK, MessageBoxIcon.Warning); } }
        finally { if (!IsDisposed) check.Enabled = launcherUpdate.Enabled = update.Enabled = play.Enabled = true; }
    }
    async Task Run(bool apply, bool launch)
    {
        check.Enabled = launcherUpdate.Enabled = update.Enabled = play.Enabled = false; folder.Enabled = remember.Enabled = account.Enabled = false;
        try
        {
            if (!apply) {
                using var releaseHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                var latest = await LauncherUpdater.LatestAsync(releaseHttp);
                var version = LauncherUpdater.ReleaseVersion(latest);
                Log(version > LauncherUpdater.CurrentVersion ? "Launcher " + version.ToString(3) + " is available. Select Update launcher to install it here." : "Your launcher is up to date (" + LauncherUpdater.CurrentVersion.ToString(3) + ").");
            }
            Log("Checking public patch manifest…"); string json;
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            json = await http.GetStringAsync(Patcher.ManifestUrl);
            var manifest = Patcher.Parse(json); Log($"Patch version: {manifest.Version}\r\n{manifest.Notes}");
            if (!apply) return;
            string client = folder.Text.Trim(); if (string.IsNullOrWhiteSpace(client)) throw new IOException("Choose the game folder first.");
            string name = remember.Checked ? account.Text.Trim() : "";
            if (remember.Checked && string.IsNullOrEmpty(name)) throw new IOException("Enter an account name, or uncheck Remember account name.");
            await Task.Run(() => Patcher.Apply(client, manifest, Log, name, verifyOnly: launch && Patcher.IsGameRunning(client)));
            if (remember.Checked) File.WriteAllText(AccountFile, name); else if (File.Exists(AccountFile)) File.Delete(AccountFile); File.WriteAllText(Path.Combine(settings, "client-path.txt"), client);
            if (launch) { Process.Start(new ProcessStartInfo(Path.Combine(client, "SWGEmu.exe")) { WorkingDirectory = client, UseShellExecute = true }); Log("New game window started."); }
        }
        catch (Exception e) { Log("Could not complete: " + e.Message); MessageBox.Show(this, e.Message, "Launcher", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        finally { check.Enabled = launcherUpdate.Enabled = update.Enabled = play.Enabled = true; folder.Enabled = remember.Enabled = account.Enabled = true; }
    }
}
