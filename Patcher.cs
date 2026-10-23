using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PrivateSwgLauncher;

public sealed record PatchManifest(int SchemaVersion, string Version, string MinimumLauncherVersion, string[] Patches, string Notes);
public static partial class Patcher
{
    public const string ManifestUrl = "https://raw.githubusercontent.com/Mr-Cee/SWGEmu-Launcher/main/patches/manifest.json";
    public const string ReleasesUrl = "https://github.com/Mr-Cee/SWGEmu-Launcher/releases/latest";
    public static readonly string[] Supported = ["center-of-being-icon", "free-crafting-skills", "waypoint-monitor-default", "galaxy-wide-travel", "map-double-click-travel", "mounted-conversations", "multiple-instances", "area-corpse-commands", "free-force-sensitive-skills", "instant-mission-accept", "companion-rider-animations", "saved-account-password-focus", "relaxed-structure-placement", "combat-master-sprint", "instant-item-destroy"];
    public static PatchManifest Parse(string text)
    {
        if (text.Length > 65536) throw new InvalidDataException("Patch manifest is too large.");
        var m = JsonSerializer.Deserialize<PatchManifest>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("Empty patch manifest.");
        if (m.SchemaVersion != 1 || string.IsNullOrWhiteSpace(m.Version) || m.Patches is null || m.Patches.Any(p => !Supported.Contains(p)))
            throw new InvalidDataException("These patches need a newer launcher. Select Update launcher to install the latest release.");
        if (!System.Version.TryParse(m.MinimumLauncherVersion, out var minimum) || minimum > new System.Version(1, 0, 17))
            throw new InvalidDataException("Select Update launcher before applying these patches.");
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
    public static byte[] CompanionRider(byte[] data)
    {
        byte[] key = Encoding.ASCII.GetBytes("arrangementDescriptorFilename\0");
        const string rider = "abstract/slot/arrangement/player.iff";
        var fields = Chunks(data).Where(c => c.Tag == "XXXX" && data.AsSpan(c.Offset + 8, c.Size).StartsWith(key)).ToArray();
        if (fields.Length != 1) throw new InvalidDataException("Unexpected companion appearance schema.");
        var field = fields[0]; int value = field.Offset + 8 + key.Length, end = field.Offset + 8 + field.Size;
        if (value >= end) throw new InvalidDataException("Missing companion attachment value.");
        if (data[value] == 1) {
            int pos = value + 1; string existing = CString(data, ref pos);
            if (pos != end || existing != rider) throw new InvalidDataException("A custom companion attachment needs review before updating.");
            return (byte[])data.Clone();
        }
        if (data[value] != 0 || value + 1 != end) throw new InvalidDataException("Unexpected companion attachment encoding.");
        byte[] replacement = [.. key, 1, .. Encoding.ASCII.GetBytes(rider), 0];
        int delta = replacement.Length - field.Size;
        byte[] result = new byte[checked(data.Length + delta)];
        data.AsSpan(0, field.Offset + 8).CopyTo(result);
        replacement.CopyTo(result, field.Offset + 8);
        data.AsSpan(end).CopyTo(result.AsSpan(field.Offset + 8 + replacement.Length));
        BinaryPrimitives.WriteInt32BigEndian(result.AsSpan(field.Offset + 4, 4), replacement.Length);
        foreach (int parent in field.Parents) BinaryPrimitives.WriteInt32BigEndian(result.AsSpan(parent + 4, 4), checked(BE(data, parent + 4) + delta));
        _ = Chunks(result); return result;
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
    public static byte[] ForceSensitiveCosts(byte[] data)
    {
        var (columns, types, rows) = Table(data);
        if (columns[0] != "NAME" || columns[8] != "POINTS_REQUIRED" || types[8] != "i") throw new InvalidDataException("Unexpected skill table schema.");
        string[] prefixes = ["force_sensitive_combat_prowess_", "force_sensitive_enhanced_reflexes_", "force_sensitive_crafting_mastery_", "force_sensitive_heightened_senses_"];
        var matches = new int[prefixes.Length];
        var result = (byte[])data.Clone(); int count = LE(data, rows.Offset + 8), p = rows.Offset + 12;
        for (int i = 0; i < count; i++)
        {
            string name = ""; int cost = -1;
            for (int column = 0; column < types.Count; column++) {
                if (types[column].StartsWith('s')) { var value = CString(data, ref p); if (column == 0) name = value; }
                else { if (column == 8) cost = p; p += 4; }
            }
            for (int tree = 0; tree < prefixes.Length; tree++) {
                if (!name.StartsWith(prefixes[tree], StringComparison.Ordinal)) continue;
                BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(cost, 4), 0); matches[tree]++; break;
            }
        }
        if (p != rows.Offset + 8 + rows.Size || matches.Any(n => n < 16)) throw new InvalidDataException("Force Sensitive skill table validation failed.");
        return result;
    }

    public static byte[] MountedConversations(byte[] data)
    {
        var (columns, types, rows) = Table(data);
        string[] names = ["npcconversationstart", "npcconversationselect", "npcconversationstop"];
        string[] permissions = ["L:drivingVehicle", "L:ridingCreature", "S:ridingMount"];
        var targets = permissions.Select(name => columns.FindIndex(c => c.Equals(name, StringComparison.OrdinalIgnoreCase))).ToArray();
        if (targets.Any(i => i < 0) || !types[0].StartsWith('s')) throw new InvalidDataException("Unexpected conversation command schema.");
        var result = (byte[])data.Clone(); int count = LE(data, rows.Offset + 8), p = rows.Offset + 12;
        var found = new HashSet<string>();
        for (int row = 0; row < count; row++)
        {
            string name = "";
            for (int column = 0; column < types.Count; column++)
            {
                if (types[column].StartsWith('s')) { var value = CString(data, ref p); if (column == 0) name = value.ToLowerInvariant(); }
                else {
                    if (names.Contains(name) && targets.Contains(column)) BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(p, 4), 1);
                    p += 4;
                }
            }
            if (names.Contains(name)) found.Add(name);
        }
        if (p != rows.Offset + 8 + rows.Size || found.Count != 3) throw new InvalidDataException("Conversation command validation failed.");
        return result;
    }
    public static byte[] AreaCommands(byte[] data)
    {
        var (columns, types, rows) = Table(data);
        if (columns[0] != "commandName" || !types[0].StartsWith('s')) throw new InvalidDataException("Unexpected command table schema.");
        int count = LE(data, rows.Offset + 8), p = rows.Offset + 12;
        var entries = new List<byte[][]>();
        var templates = new Dictionary<string, byte[][]>(StringComparer.OrdinalIgnoreCase);
        var custom = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < count; i++)
        {
            var row = new byte[types.Count][];
            for (int c = 0; c < types.Count; c++)
            {
                int start = p;
                if (types[c].StartsWith('s')) _ = CString(data, ref p); else p += 4;
                if (p > rows.Offset + 8 + rows.Size) throw new InvalidDataException("Truncated command row.");
                row[c] = data[start..p];
            }
            string name = Encoding.UTF8.GetString(row[0]).TrimEnd('\0');
            if (name.Equals("loot", StringComparison.OrdinalIgnoreCase) || name.Equals("harvestCorpse", StringComparison.OrdinalIgnoreCase))
            {
                if (!templates.TryAdd(name, row)) throw new InvalidDataException("Duplicate command template.");
            }
            if (name.Equals("lootArea", StringComparison.OrdinalIgnoreCase) || name.Equals("harvestArea", StringComparison.OrdinalIgnoreCase))
            {
                if (!custom.Add(name)) throw new InvalidDataException("Duplicate area command.");
            }
            else entries.Add(row);
        }
        if (p != rows.Offset + 8 + rows.Size || templates.Count != 2) throw new InvalidDataException("Command templates missing or invalid.");
        foreach (var pair in new[] { ("lootArea", "loot"), ("harvestArea", "harvestCorpse") })
        {
            var row = templates[pair.Item2].Select(v => (byte[])v.Clone()).ToArray();
            void Text(string col, string value) { int i = columns.IndexOf(col); if (i < 0 || !types[i].StartsWith('s')) throw new InvalidDataException("Missing string command column."); row[i] = Encoding.UTF8.GetBytes(value + "\0"); }
            void Number(string col, int value) { int i = columns.IndexOf(col); if (i < 0 || types[i].StartsWith('s')) throw new InvalidDataException("Missing numeric command column."); row[i] = BitConverter.GetBytes(value); }
            Text("commandName", pair.Item1);
            foreach (string col in new[] { "scriptHook", "failScriptHook", "cppHook", "failCppHook", "tempScript", "characterAbility", "stringId" }) Text(col, "");
            foreach (string col in new[] { "targetType", "maxRangeToTarget", "defaultTime", "defaultPriority", "disabled", "godLevel", "addToCombatQueue", "callOnTarget", "commandGroup" }) Number(col, 0);
            entries.Add(row);
        }
        using var mem = new MemoryStream(); using var writer = new BinaryWriter(mem, Encoding.UTF8, true);
        writer.Write(entries.Count); foreach (var row in entries) foreach (var cell in row) writer.Write(cell);
        byte[] payload = mem.ToArray(); int delta = payload.Length - rows.Size;
        byte[] output = new byte[data.Length + delta]; Array.Copy(data, output, rows.Offset + 4);
        BinaryPrimitives.WriteInt32BigEndian(output.AsSpan(rows.Offset + 4, 4), payload.Length);
        payload.CopyTo(output, rows.Offset + 8);
        Array.Copy(data, rows.Offset + 8 + rows.Size, output, rows.Offset + 8 + payload.Length, data.Length - rows.Offset - 8 - rows.Size);
        foreach (int parent in rows.Parents) BinaryPrimitives.WriteInt32BigEndian(output.AsSpan(parent + 4, 4), BE(data, parent + 4) + delta);
        _ = Table(output); return output;
    }

    public static byte[] TravelFares(byte[] data)
    {
        var (columns, types, rows) = Table(data);
        if (columns.Count != 11 || columns[0] != "Planet" || types[0] != "s" || types.Skip(1).Any(t => t != "I")) throw new InvalidDataException("Unexpected travel table schema.");
        int count = LE(data, rows.Offset + 8), p = rows.Offset + 12;
        if (count != 10) throw new InvalidDataException("Unexpected travel planet count.");
        var offsets = new int[count, count];
        for (int i = 0; i < count; i++)
        {
            if (CString(data, ref p) != columns[i + 1]) throw new InvalidDataException("Unexpected travel planet order.");
            for (int j = 0; j < count; j++) { offsets[i,j] = p; p += 4; }
        }
        if (p != rows.Offset + 8 + rows.Size) throw new InvalidDataException("Invalid travel rows.");
        var result = (byte[])data.Clone();
        for (int i = 0; i < count; i++) for (int j = i; j < count; j++)
        {
            int fare = LE(data, offsets[i,j]); if (fare <= 0) fare = 1000;
            BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(offsets[i,j],4),fare);
            BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(offsets[j,i],4),fare);
        }
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
    public static bool IsGameRunning(string client)
    {
        string path = Path.Combine(Path.GetFullPath(client), "SWGEmu.exe");
        foreach (var process in System.Diagnostics.Process.GetProcessesByName("SWGEmu"))
        {
            try { if (string.Equals(process.MainModule?.FileName, path, StringComparison.OrdinalIgnoreCase)) return true; }
            finally { process.Dispose(); }
        }
        return false;
    }
    public static void Apply(string client, PatchManifest manifest, Action<string> log, string? accountName = null, bool verifyOnly = false)
    {
        if (accountName is not null && !Regex.IsMatch(accountName, @"^[A-Za-z0-9_-]{0,64}$")) throw new InvalidDataException("Account names may contain letters, numbers, underscores and hyphens only.");
        client = Path.GetFullPath(client);
        if (!File.Exists(Path.Combine(client, "SWGEmu.exe")) || !File.Exists(Path.Combine(client, "swgemu.cfg"))) throw new InvalidDataException("Select the folder containing SWGEmu.exe and swgemu.cfg.");
        using var updateLock = new FileStream(Path.Combine(client, "private-launcher.update.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        if (!verifyOnly && IsGameRunning(client)) throw new IOException("Close all game windows before installing updates.");
        var pending = new Dictionary<string, byte[]>();
        if (manifest.Patches.Contains("companion-rider-animations")) {
            log("Preparing companion mount supportâ€¦");
            foreach (string resource in new[] { "object/mobile/shared_dressed_brawler_trainer_01.iff", "object/mobile/shared_dressed_doctor_trainer_human_female_01.iff" })
                pending[resource] = CompanionRider(Resource(client, resource));
        }
        if (manifest.Patches.Contains("mounted-conversations")) { log("Preparing mounted NPC conversationsâ€¦"); pending["datatables/command/command_table.iff"] = MountedConversations(Resource(client, "datatables/command/command_table.iff")); }
        if (manifest.Patches.Contains("area-corpse-commands")) {
            log("Preparing area loot and harvest commandsâ€¦");
            const string commandTable = "datatables/command/command_table.iff";
            pending[commandTable] = AreaCommands(pending.TryGetValue(commandTable, out var table) ? table : Resource(client, commandTable));
        }
        if (manifest.Patches.Contains("map-double-click-travel") || manifest.Patches.Contains("instant-mission-accept") || manifest.Patches.Contains("relaxed-structure-placement"))
        {
            string executableHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(Path.Combine(client, "SWGEmu.exe"))));
            if (executableHash != "58012E57CEBC499454812BA7ED96B1289DB01E520963B4FC364EDB41C322B2A8")
                throw new InvalidDataException("This game executable is not supported by the map double-click extension. Client files were not changed.");
            using var dll = typeof(Patcher).Assembly.GetManifestResourceStream("SWGEmuLauncher.native.dinput8.dll") ?? throw new InvalidDataException("Travel extension payload is missing.");
            using var memory = new MemoryStream(); dll.CopyTo(memory); byte[] payload = memory.ToArray();
            string installed = Path.Combine(client, "dinput8.dll");
            if (File.Exists(installed) && !File.ReadAllBytes(installed).SequenceEqual(payload) &&
                (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(installed))) != "2A35DEA73AE37103FD2107486A5595457DD4C829650BC7F6E5DA757DFE2A7346" &&
                Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(installed))) != "DFDE4DF7AA9D10FA75FC13990D47DFC3F0ADB97B26216381BBC2222CE6CB4F83" &&
                Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(installed))) != "9C59052F9BCD3FCC3F20802029BE9756596DD6907A1060F840F5C64A939FCE23" &&
                Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(installed))) != "72AF92D5C9355D0CE25F05999F357A3DE31DB7C3325B3BD5D3AA368F2F927518" &&
                Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(installed))) != "4479CB8D5AF9551E6C2BB701E850953BE5904E3FE494316EB16A7D728CB72B3F" &&
                Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(installed))) != "FCAE3562EF9D658C26945DF4201F9BA0FFE6CD60B4857DDE53E01025055EF625"))
                throw new InvalidDataException("A different dinput8.dll is already installed. It must be reviewed before adding the travel extension.");
            pending["dinput8.dll"] = payload;
        }

        if (manifest.Patches.Contains("galaxy-wide-travel")) { log("Preparing galaxy-wide travel routesâ€¦"); pending["datatables/travel/travel.iff"] = TravelFares(Resource(client, "datatables/travel/travel.iff")); }
        if (manifest.Patches.Contains("free-crafting-skills")) { log("Preparing free crafting skillsÃ¢â‚¬Â¦"); pending["datatables/skill/skills.iff"] = SkillCosts(Resource(client, "datatables/skill/skills.iff")); }
        if (manifest.Patches.Contains("free-force-sensitive-skills")) {
            log("Preparing free Force Sensitive skill pointsâ€¦");
            const string skills = "datatables/skill/skills.iff";
            pending[skills] = ForceSensitiveCosts(pending.TryGetValue(skills, out var table) ? table : Resource(client, skills));
        }
        if (manifest.Patches.Contains("center-of-being-icon")) { log("Preparing Center of Being iconÃ¢â‚¬Â¦"); pending["datatables/modifiers/modifiers.iff"] = BuffIcon(Resource(client, "datatables/modifiers/modifiers.iff")); }
        if (manifest.Patches.Contains("combat-master-sprint")) {
            log("Preparing combat master Sprint abilityâ€¦");
            byte[] Current(string path) => pending.TryGetValue(path, out var value) ? value : Resource(client,path);
            const string skills="datatables/skill/skills.iff", commands="datatables/command/command_table.iff", modifiers="datatables/modifiers/modifiers.iff";
            pending[skills]=SprintSkills(Current(skills)); pending[commands]=SprintCommands(Current(commands)); pending[modifiers]=SprintModifiers(Current(modifiers));
            pending["ui/ui_styles.inc"]=SprintIcons(Current("ui/ui_styles.inc"));
            pending["string/en/cmd_n.stf"]=SprintStrings(Current("string/en/cmd_n.stf"),"Sprint");
            pending["string/en/cmd_d.stf"]=SprintStrings(Current("string/en/cmd_d.stf"),"Run 30% faster on foot for 30 seconds. Cooldown: 60 seconds from activation. Granted by combat master skill boxes. Mounting ends Sprint.");
        }
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
        const string instances = ".include \"private_launcher_instances.cfg\"";
        configLines.RemoveAll(l => l.Trim() == instances);
        if (manifest.Patches.Contains("multiple-instances"))
        {
            pending["private_launcher_instances.cfg"] = Encoding.ASCII.GetBytes("[SwgClient]\nallowMultipleInstances=true\n");
            configLines.Add(instances);
        }
        const string placement = ".include \"private_launcher_placement.cfg\"";
        configLines.RemoveAll(l => l.Trim() == placement);
        if (manifest.Patches.Contains("relaxed-structure-placement"))
        {
            pending["private_launcher_placement.cfg"] = Encoding.ASCII.GetBytes("[ClientGame]\nalwaysAllowStructurePlacement=true\nrenderLotManager=false\n");
            configLines.Add(placement);
        }
        configLines.Add(icons); pending["swgemu.cfg"] = Encoding.ASCII.GetBytes(string.Join("\r\n", configLines) + "\r\n");
        // Prepare every result before touching the client; back up the complete change set.
        var changed = pending.Where(kv => !File.Exists(SafePath(client, kv.Key)) || !File.ReadAllBytes(SafePath(client, kv.Key)).SequenceEqual(kv.Value)).ToList();
        if (verifyOnly)
        {
            if (changed.Count != 0) throw new IOException("A client update or login setting change is required. Close all game windows, then Update & Play.");
            log($"Patch {manifest.Version} verified. Opening another game window is safe.");
            return;
        }
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
