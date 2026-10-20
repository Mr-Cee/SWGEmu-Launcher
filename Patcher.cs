using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PrivateSwgLauncher;

public sealed record PatchManifest(int SchemaVersion, string Version, string MinimumLauncherVersion, string[] Patches, string Notes);
public static class Patcher
{
    public const string ManifestUrl = "https://raw.githubusercontent.com/Mr-Cee/SWGEmu-Launcher/main/patches/manifest.json";
    public const string ReleasesUrl = "https://github.com/Mr-Cee/SWGEmu-Launcher/releases/latest";
    public static readonly string[] Supported = ["center-of-being-icon", "free-crafting-skills", "waypoint-monitor-default"];
    public static PatchManifest Parse(string text)
    {
        if (text.Length > 65536) throw new InvalidDataException("Patch manifest is too large.");
        var m = JsonSerializer.Deserialize<PatchManifest>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("Empty patch manifest.");
        if (m.SchemaVersion != 1 || string.IsNullOrWhiteSpace(m.Version) || m.Patches is null || m.Patches.Any(p => !Supported.Contains(p)))
            throw new InvalidDataException("These patches need a newer launcher. Download the latest release.");
        if (!System.Version.TryParse(m.MinimumLauncherVersion, out var minimum) || minimum > new System.Version(1, 0, 1))
            throw new InvalidDataException("Download the latest launcher before applying these patches.");
        return m;
    }
    public static string BundledManifest()
    {
        using var stream = typeof(Patcher).Assembly.GetManifestResourceStream("SWGEmuLauncher.patches.manifest.json")
            ?? throw new InvalidDataException("Bundled manifest is missing.");
        using var reader = new StreamReader(stream); return reader.ReadToEnd();
    }
    static int LE(byte[] b, int p) => BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(p, 4));
    static int BE(byte[] b, int p) => BinaryPrimitives.ReadInt32BigEndian(b.AsSpan(p, 4));
    static string CString(byte[] b, ref int pos)
    {
        int end = Array.IndexOf(b, (byte)0, pos);
        if (end < pos) throw new InvalidDataException("Invalid string in game data.");
        string s = Encoding.UTF8.GetString(b, pos, end - pos); pos = end + 1; return s;
    }
    static byte[] Block(BinaryReader reader, int compression, int length, int expected)
    {
        if (length < 0 || length > 32 * 1024 * 1024 || expected < 0 || expected > 32 * 1024 * 1024)
            throw new InvalidDataException("Archive block exceeds supported size.");
        byte[] input = reader.ReadBytes(length);
        if (input.Length != length) throw new InvalidDataException("Truncated game archive.");
        if (compression == 0) { if (input.Length != expected) throw new InvalidDataException("Wrong block length."); return input; }
        if (compression != 2) throw new InvalidDataException("Unsupported archive compression.");
        using var source = new MemoryStream(input); using var z = new ZLibStream(source, CompressionMode.Decompress);
        byte[] output = new byte[expected]; z.ReadExactly(output);
        if (z.ReadByte() != -1) throw new InvalidDataException("Unexpected decompressed length.");
        return output;
    }
    static byte[]? FromArchive(string filename, string resource)
    {
        using var file = File.OpenRead(filename); using var r = new BinaryReader(file);
        if (Encoding.ASCII.GetString(r.ReadBytes(8)) != "EERT5000") throw new InvalidDataException("Unsupported archive " + Path.GetFileName(filename));
        int count = r.ReadInt32(), offset = r.ReadInt32(), ct = r.ReadInt32(), cs = r.ReadInt32(), nt = r.ReadInt32(), ns = r.ReadInt32(), nu = r.ReadInt32();
        if (count < 0 || count > 1_000_000 || offset < 36 || offset > file.Length) throw new InvalidDataException("Invalid archive index.");
        file.Position = offset; var records = Block(r, ct, cs, checked(count * 24)); var names = Block(r, nt, ns, nu);
        for (int i = 0; i < count; i++)
        {
            int p = i * 24; int namePos = LE(records, p + 20);
            if (CString(names, ref namePos) != resource) continue;
            int size = LE(records, p + 4), location = LE(records, p + 8), compression = LE(records, p + 12), packed = LE(records, p + 16);
            if (location < 36 || location > file.Length) throw new InvalidDataException("Invalid archive offset.");
            file.Position = location; return Block(r, compression, compression == 0 ? size : packed, size);
        }
        return null;
    }
    public static List<string> ConfigLines(string client)
    {
        List<string> result = []; HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        void Load(string file)
        {
            if (!seen.Add(file) || !File.Exists(file)) return;
            foreach (var line in File.ReadAllLines(file))
            {
                var m = Regex.Match(line, "^\\s*\\.include\\s+\"([^\"]+)\"");
                if (m.Success) Load(SafePath(client, m.Groups[1].Value)); else result.Add(line);
            }
        }
        Load(Path.Combine(client, "swgemu.cfg")); return result;
    }
    static string SafePath(string client, string relative)
    {
        var root = Path.GetFullPath(client).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("A configuration path leaves the game folder.");
        return path;
    }
    public static byte[] Resource(string client, string resource)
    {
        string loose = SafePath(client, resource); if (File.Exists(loose)) return File.ReadAllBytes(loose);
        var archives = ConfigLines(client).Select(line => Regex.Match(line, @"^\s*searchTree_\d+_(\d+)\s*=\s*(\S+)"))
            .Where(m => m.Success).OrderByDescending(m => int.Parse(m.Groups[1].Value));
        foreach (var m in archives)
        {
            var file = SafePath(client, m.Groups[2].Value); if (!File.Exists(file)) continue;
            var data = FromArchive(file, resource); if (data is not null) return data;
        }
        throw new FileNotFoundException("Could not locate " + resource + ". Select a complete SWGEmu game installation.");
    }
    record Chunk(string Tag, int Offset, int Size, int[] Parents);
    static List<Chunk> Chunks(byte[] data)
    {
        List<Chunk> all = [];
        void Walk(int start, int end, int[] parents)
        {
            int p = start;
            while (p < end)
            {
                if (p + 8 > end) throw new InvalidDataException("Truncated IFF header.");
                string tag = Encoding.ASCII.GetString(data, p, 4); int size = BE(data, p + 4);
                if (size < 0 || (long)p + 8 + size > end) throw new InvalidDataException("Invalid IFF length.");
                all.Add(new(tag, p, size, parents));
                if (tag == "FORM") Walk(p + 12, p + 8 + size, [.. parents, p]);
                p += 8 + size;
            }
            if (p != end) throw new InvalidDataException("Invalid IFF boundary.");
        }
        Walk(0, data.Length, []); return all;
    }
    static (List<string> Columns, List<string> Types, Chunk Rows) Table(byte[] data)
    {
        var chunks = Chunks(data); var c = chunks.Single(c => c.Tag == "COLS"); int count = LE(data, c.Offset + 8), p = c.Offset + 12;
        List<string> columns = []; for (int i = 0; i < count; i++) columns.Add(CString(data, ref p));
        var t = chunks.Single(c => c.Tag == "TYPE"); p = t.Offset + 8; List<string> types = [];
        for (int i = 0; i < count; i++) types.Add(CString(data, ref p));
        return (columns, types, chunks.Single(c => c.Tag == "ROWS"));
    }
    public static byte[] SkillCosts(byte[] data)
    {
        var (columns, types, rows) = Table(data);
        if (columns[0] != "NAME" || columns[8] != "POINTS_REQUIRED" || types[8] != "i") throw new InvalidDataException("Unexpected skill table schema.");
        var result = (byte[])data.Clone(); int count = LE(data, rows.Offset + 8), p = rows.Offset + 12, matches = 0; bool artisan = false;
        for (int i = 0; i < count; i++)
        {
            string name = ""; int cost = -1;
            for (int column = 0; column < types.Count; column++)
            {
                if (types[column].StartsWith('s')) { var v = CString(data, ref p); if (column == 0) name = v; }
                else { if (column == 8) cost = p; p += 4; }
            }
            if (name.StartsWith("crafting_", StringComparison.Ordinal) || name.StartsWith("outdoors_bio_engineer_", StringComparison.Ordinal))
            {
                BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(cost, 4), 0); matches++; artisan |= name == "crafting_artisan_novice";
            }
        }
        if (p != rows.Offset + 8 + rows.Size || !artisan || matches < 180) throw new InvalidDataException("Skill table validation failed.");
        return result;
    }
    public static byte[] BuffIcon(byte[] data)
    {
        var (columns, types, rows) = Table(data);
        if (!columns.SequenceEqual(new[] { "ModifierName", "DisplayName", "DisplayDescription", "IconImageStyle" }) || types.Any(t => t != "s"))
            throw new InvalidDataException("Unexpected modifier table schema.");
        string[] entry = ["centerofbeing", "@cmd_n:centerofbeing", "@cmd_d:centerofbeing", "/Styles.Icon.command.centerOfBeing"];
        int count = LE(data, rows.Offset + 8), p = rows.Offset + 12, matched = 0; List<string[]> entries = [];
        for (int i = 0; i < count; i++)
        {
            string[] row = new string[4]; for (int j = 0; j < 4; j++) row[j] = CString(data, ref p);
            if (row[0] == "centerofbeing") { row = entry; matched++; } entries.Add(row);
        }
        if (matched > 1 || p != rows.Offset + 8 + rows.Size) throw new InvalidDataException("Invalid buff table.");
        if (matched == 0) entries.Add(entry);
        using var mem = new MemoryStream(); using var writer = new BinaryWriter(mem, Encoding.UTF8, true); writer.Write(entries.Count);
        foreach (var row in entries) foreach (var value in row) { writer.Write(Encoding.UTF8.GetBytes(value)); writer.Write((byte)0); }
        byte[] payload = mem.ToArray(); int delta = payload.Length - rows.Size;
        byte[] output = new byte[data.Length + delta]; Array.Copy(data, output, rows.Offset + 4);
        BinaryPrimitives.WriteInt32BigEndian(output.AsSpan(rows.Offset + 4, 4), payload.Length);
        payload.CopyTo(output, rows.Offset + 8); Array.Copy(data, rows.Offset + 8 + rows.Size, output, rows.Offset + 8 + payload.Length, data.Length - rows.Offset - 8 - rows.Size);
        foreach (int parent in rows.Parents) BinaryPrimitives.WriteInt32BigEndian(output.AsSpan(parent + 4, 4), BE(data, parent + 4) + delta);
        _ = Table(output); return output;
    }
    public static void Apply(string client, PatchManifest manifest, Action<string> log, string? accountName = null)
    {
        if (accountName is not null && !Regex.IsMatch(accountName, @"^[A-Za-z0-9_-]{0,64}$")) throw new InvalidDataException("Account names may contain letters, numbers, underscores and hyphens only.");
        client = Path.GetFullPath(client);
        if (!File.Exists(Path.Combine(client, "SWGEmu.exe")) || !File.Exists(Path.Combine(client, "swgemu.cfg"))) throw new InvalidDataException("Select the folder containing SWGEmu.exe and swgemu.cfg.");
        using var updateLock = new FileStream(Path.Combine(client, "private-launcher.update.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        foreach (var process in System.Diagnostics.Process.GetProcessesByName("SWGEmu"))
        {
            try { if (string.Equals(process.MainModule?.FileName, Path.Combine(client, "SWGEmu.exe"), StringComparison.OrdinalIgnoreCase)) throw new IOException("Close the game before updating."); }
            finally { process.Dispose(); }
        }
        var pending = new Dictionary<string, byte[]>();
        if (manifest.Patches.Contains("free-crafting-skills")) { log("Preparing free crafting skills…"); pending["datatables/skill/skills.iff"] = SkillCosts(Resource(client, "datatables/skill/skills.iff")); }
        if (manifest.Patches.Contains("center-of-being-icon")) { log("Preparing Center of Being icon…"); pending["datatables/modifiers/modifiers.iff"] = BuffIcon(Resource(client, "datatables/modifiers/modifiers.iff")); }
        var lines = ConfigLines(client); int priority = 26;
        foreach (var line in lines)
        {
            var m = Regex.Match(line, @"^\s*search(?:Tree|Path)_\d+_(\d+)\s*="); if (m.Success) priority = Math.Max(priority, int.Parse(m.Groups[1].Value) + 1);
        }
        string existing = Path.Combine(client, "private_server_buff_icons.cfg");
        if (File.Exists(existing)) { var m = Regex.Match(File.ReadAllText(existing), @"searchPath_00_(\d+)=\."); if (m.Success) priority = int.Parse(m.Groups[1].Value); }
        pending["private_server_buff_icons.cfg"] = Encoding.ASCII.GetBytes($"[SharedFile]\nmaxSearchPriority={priority + 1}\nsearchPath_00_{priority}=.\n");
        const string defaults = ".include \"private_server_defaults.cfg\"", icons = ".include \"private_server_buff_icons.cfg\"";
        var configLines = File.ReadAllLines(Path.Combine(client, "swgemu.cfg")).Where(l => l.Trim() != defaults && l.Trim() != icons).ToList();
        if (manifest.Patches.Contains("waypoint-monitor-default"))
        {
            pending["private_server_defaults.cfg"] = Encoding.ASCII.GetBytes("[ClientUserInterface]\nuseWaypointMonitor = true\n"); configLines.Insert(0, defaults);
        }
        if (accountName is not null)
        {
            const string login = ".include \"private_launcher_account.cfg\"";
            configLines.RemoveAll(l => l.Trim() == login);
            configLines.Add(login);
            pending["private_launcher_account.cfg"] = Encoding.ASCII.GetBytes($"[ClientGame]\nloginClientID={accountName}\nloginClientPassword=\nautoConnectToLoginServer=false\n");
        }
        configLines.Add(icons); pending["swgemu.cfg"] = Encoding.ASCII.GetBytes(string.Join("\r\n", configLines) + "\r\n");
        // Prepare every result before touching the client; back up the complete change set.
        var changed = pending.Where(kv => !File.Exists(SafePath(client, kv.Key)) || !File.ReadAllBytes(SafePath(client, kv.Key)).SequenceEqual(kv.Value)).ToList();
        string backup = Path.Combine(client, "private-launcher-backups", DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
        foreach (var item in changed)
        {
            string file = SafePath(client, item.Key), saved = Path.Combine(backup, item.Key.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(file)) { Directory.CreateDirectory(Path.GetDirectoryName(saved)!); File.Copy(file, saved); }
        }
        List<string> written = [];
        try
        {
            foreach (var item in changed)
            {
                string file = SafePath(client, item.Key); Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                written.Add(item.Key); File.WriteAllBytes(file + ".launcher-tmp", item.Value); File.Move(file + ".launcher-tmp", file, true);
            }
        }
        catch
        {
            foreach (var relative in written.AsEnumerable().Reverse())
            {
                string file = SafePath(client, relative), saved = Path.Combine(backup, relative.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(saved)) File.Copy(saved, file, true); else if (File.Exists(file)) File.Delete(file);
                if (File.Exists(file + ".launcher-tmp")) File.Delete(file + ".launcher-tmp");
            }
            throw;
        }
        log($"Patch {manifest.Version} ready. {changed.Count} file(s) updated. Server address preserved.");
    }
}
