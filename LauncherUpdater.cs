using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace PrivateSwgLauncher;

public sealed record LauncherAsset([property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("browser_download_url")] string Url,
    [property: JsonPropertyName("size")] long Size);
public sealed record LauncherRelease([property: JsonPropertyName("tag_name")] string Tag,
    [property: JsonPropertyName("draft")] bool Draft,
    [property: JsonPropertyName("prerelease")] bool Prerelease,
    [property: JsonPropertyName("assets")] LauncherAsset[] Assets);
public sealed record LauncherUpdateJob(string Target, string OldHash, string NewHash, string Version,
    int ParentPid, long ParentStartTicks, int HelperPid = 0);

public static class LauncherUpdater
{
    public const string ReleaseApi = "https://api.github.com/repos/Mr-Cee/SWGEmu-Launcher/releases/latest";
    public const string PackageName = "SWGEmuLauncher-win-x64.zip";
    public static Version CurrentVersion => typeof(LauncherUpdater).Assembly.GetName().Version!;
    public static string UpdatesRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrivateSWGEmuLauncher", "updates");
    static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    public static Version ReleaseVersion(LauncherRelease release)
    {
        if (release.Draft || release.Prerelease || !Regex.IsMatch(release.Tag ?? "", @"^v\d+\.\d+\.\d+$") ||
            !Version.TryParse(release.Tag![1..], out var version)) throw new InvalidDataException("Unsupported launcher release.");
        return new Version(version.Major, version.Minor, version.Build, 0);
    }
    public static async Task<LauncherRelease> LatestAsync(HttpClient http)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ReleaseApi);
        request.Headers.UserAgent.ParseAdd("SWGEmuLauncher/" + CurrentVersion);
        using var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength > 1024 * 1024) throw new InvalidDataException("Release response is too large.");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        if (bytes.Length > 1024 * 1024) throw new InvalidDataException("Release response is too large.");
        var release = JsonSerializer.Deserialize<LauncherRelease>(bytes) ?? throw new InvalidDataException("Missing launcher release.");
        _ = ReleaseVersion(release); return release;
    }
    static LauncherAsset Asset(LauncherRelease release, string name)
    {
        var matches = release.Assets?.Where(a => a.Name == name).ToArray();
        if (matches is null || matches.Length != 1) throw new InvalidDataException("The launcher release is incomplete.");
        var asset = matches[0];
        string expected = $"https://github.com/Mr-Cee/SWGEmu-Launcher/releases/download/{release.Tag}/{name}";
        if (asset.Url != expected || asset.Size <= 0) throw new InvalidDataException("Invalid launcher download location.");
        return asset;
    }
    static async Task DownloadAsync(HttpClient http, LauncherAsset asset, string path, long limit)
    {
        if (asset.Size > limit) throw new InvalidDataException("Launcher download is too large.");
        using var response = await http.GetAsync(asset.Url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var uri = response.RequestMessage?.RequestUri;
        if (uri is null || uri.Scheme != "https" || !(uri.Host == "github.com" || uri.Host == "release-assets.githubusercontent.com" || uri.Host == "objects.githubusercontent.com"))
            throw new InvalidDataException("Unexpected launcher download redirect.");
        if (response.Content.Headers.ContentLength > limit) throw new InvalidDataException("Launcher download is too large.");
        await using var input = await response.Content.ReadAsStreamAsync();
        await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        var buffer = new byte[81920]; long total = 0; int count;
        while ((count = await input.ReadAsync(buffer)) > 0) {
            total += count; if (total > limit) throw new InvalidDataException("Launcher download exceeded its size limit.");
            await output.WriteAsync(buffer.AsMemory(0, count));
        }
        if (total != asset.Size) throw new InvalidDataException("Launcher download is incomplete.");
    }
    public static void VerifyChecksum(string package, string checksums)
    {
        var matches = File.ReadAllLines(checksums).Select(l => Regex.Match(l, @"^([a-fA-F0-9]{64})\s+\*?SWGEmuLauncher-win-x64\.zip\s*$")).Where(m => m.Success).ToArray();
        if (matches.Length != 1 || !Hash(package).Equals(matches[0].Groups[1].Value, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Launcher checksum verification failed. Your installed launcher has not changed.");
    }
    public static void ExtractLauncher(string package, string destination, Version expected)
    {
        using var zip = ZipFile.OpenRead(package);
        var entries = zip.Entries.Where(e => e.FullName.Equals("SWGEmuLauncher.exe", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (entries.Length != 1 || entries[0].Length < 1024 || entries[0].Length > 200L * 1024 * 1024)
            throw new InvalidDataException("The launcher package is invalid.");
        entries[0].ExtractToFile(destination, false);
        var version = FileVersionInfo.GetVersionInfo(destination);
        if (version.FileMajorPart != expected.Major || version.FileMinorPart != expected.Minor || version.FileBuildPart != expected.Build || version.FilePrivatePart != 0)
            throw new InvalidDataException("The downloaded launcher does not match the release version.");
    }
    public static async Task<string?> PrepareAsync(HttpClient http, string target, Action<string> log)
    {
        log("Checking for a launcher update…");
        var release = await LatestAsync(http); var version = ReleaseVersion(release);
        if (version <= CurrentVersion) { log("Your launcher is up to date (" + CurrentVersion.ToString(3) + ")."); return null; }
        target = Path.GetFullPath(target);
        if (!Path.GetFileName(target).Equals("SWGEmuLauncher.exe", StringComparison.OrdinalIgnoreCase) || !File.Exists(target) ||
            (File.GetAttributes(target) & FileAttributes.ReparsePoint) != 0) throw new IOException("Run the installed SWGEmuLauncher.exe to update it.");
        // Check write access before downloading; no elevation or game-file changes.
        string probe = Path.Combine(Path.GetDirectoryName(target)!, ".launcher-write-" + Guid.NewGuid().ToString("N"));
        using (new FileStream(probe, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { }
        File.Delete(probe);
        string dir = Path.Combine(UpdatesRoot, Guid.NewGuid().ToString("N")); Directory.CreateDirectory(dir);
        try {
            string package = Path.Combine(dir, PackageName), checksums = Path.Combine(dir, "SHA256SUMS.txt"), executable = Path.Combine(dir, "SWGEmuLauncher.exe");
            log("Downloading launcher " + version.ToString(3) + "…");
            await DownloadAsync(http, Asset(release, PackageName), package, 200L * 1024 * 1024);
            await DownloadAsync(http, Asset(release, "SHA256SUMS.txt"), checksums, 65536);
            VerifyChecksum(package, checksums); ExtractLauncher(package, executable, version);
            using var parent = Process.GetCurrentProcess();
            var job = new LauncherUpdateJob(target, Hash(target), Hash(executable), version.ToString(4), parent.Id, parent.StartTime.ToUniversalTime().Ticks);
            string jobPath = Path.Combine(dir, "update.json"); File.WriteAllText(jobPath, JsonSerializer.Serialize(job));
            log("Download verified. Restarting the launcher to install the update…"); return jobPath;
        } catch { DeleteJobDirectory(dir); throw; }
    }
    public static void StartInstall(string jobPath)
    {
        ValidateJobPath(jobPath);
        var start = new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(jobPath)!, "SWGEmuLauncher.exe")) { UseShellExecute = false, CreateNoWindow = true };
        start.ArgumentList.Add("--apply-launcher-update"); start.ArgumentList.Add(jobPath);
        _ = Process.Start(start) ?? throw new IOException("Could not start the launcher updater.");
    }
    static void ValidateJobPath(string jobPath)
    {
        string dir = Path.GetDirectoryName(Path.GetFullPath(jobPath))!;
        if (Path.GetFileName(jobPath) != "update.json" || Path.GetDirectoryName(dir) != Path.GetFullPath(UpdatesRoot) || !Guid.TryParseExact(Path.GetFileName(dir), "N", out _))
            throw new InvalidDataException("Invalid launcher update location.");
    }
    static void DeleteJobDirectory(string dir)
    {
        ValidateJobPath(Path.Combine(dir, "update.json"));
        try { Directory.Delete(dir, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }
    public static void ReplaceVerified(string source, LauncherUpdateJob job)
    {
        string target = Path.GetFullPath(job.Target);
        if (!Path.GetFileName(target).Equals("SWGEmuLauncher.exe", StringComparison.OrdinalIgnoreCase) ||
            (File.GetAttributes(target) & FileAttributes.ReparsePoint) != 0 ||
            !Hash(source).Equals(job.NewHash, StringComparison.OrdinalIgnoreCase) || !Hash(target).Equals(job.OldHash, StringComparison.OrdinalIgnoreCase))
            throw new IOException("Launcher files changed during the update. The installed launcher was preserved.");
        string pending = target + ".update-" + Guid.NewGuid().ToString("N");
        try {
            File.Copy(source, pending, false);
            File.Replace(pending, target, target + ".previous", true);
        } finally { if (File.Exists(pending)) File.Delete(pending); }
    }
    public static void Apply(string jobPath)
    {
        ValidateJobPath(jobPath);
        var job = JsonSerializer.Deserialize<LauncherUpdateJob>(File.ReadAllText(jobPath)) ?? throw new InvalidDataException("Invalid launcher update job.");
        string source = Path.Combine(Path.GetDirectoryName(jobPath)!, "SWGEmuLauncher.exe");
        if (!string.Equals(source, Environment.ProcessPath, StringComparison.OrdinalIgnoreCase) || job.Version != CurrentVersion.ToString(4))
            throw new InvalidDataException("Launcher update helper does not match the download.");
        try {
            using var parent = Process.GetProcessById(job.ParentPid);
            if (parent.StartTime.ToUniversalTime().Ticks != job.ParentStartTicks) throw new IOException("Launcher process changed during update.");
            if (!parent.WaitForExit(60000)) throw new IOException("Close the original launcher and try again.");
        } catch (ArgumentException) { /* Original launcher has already exited. */ }
        foreach (var process in Process.GetProcessesByName("SWGEmuLauncher")) {
            using (process) {
                if (process.Id == Environment.ProcessId) continue;
                try { if (string.Equals(process.MainModule?.FileName, job.Target, StringComparison.OrdinalIgnoreCase)) throw new IOException("Close other launcher windows before updating."); }
                catch (System.ComponentModel.Win32Exception) { throw new IOException("Could not check another launcher process. Close other launcher windows first."); }
            }
        }
        ReplaceVerified(source, job);
        try {
            job = job with { HelperPid = Environment.ProcessId }; File.WriteAllText(jobPath, JsonSerializer.Serialize(job));
            var start = new ProcessStartInfo(job.Target) { UseShellExecute = false, WorkingDirectory = Path.GetDirectoryName(job.Target)! };
            start.ArgumentList.Add("--launcher-updated"); start.ArgumentList.Add(jobPath);
            _ = Process.Start(start) ?? throw new IOException("Could not reopen the updated launcher.");
        } catch {
            File.Replace(job.Target + ".previous", job.Target, null, true); throw;
        }
    }
    public static async Task CleanupAsync(string jobPath)
    {
        try {
            ValidateJobPath(jobPath);
            var job = JsonSerializer.Deserialize<LauncherUpdateJob>(File.ReadAllText(jobPath));
            if (job is null || !string.Equals(job.Target, Environment.ProcessPath, StringComparison.OrdinalIgnoreCase) || job.HelperPid <= 0 || job.HelperPid == Environment.ProcessId) return;
            try { using var helper = Process.GetProcessById(job.HelperPid); await helper.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(60)); } catch (ArgumentException) { }
            DeleteJobDirectory(Path.GetDirectoryName(jobPath)!);
        } catch (IOException) { } catch (UnauthorizedAccessException) { } catch (TimeoutException) { }
    }
}
