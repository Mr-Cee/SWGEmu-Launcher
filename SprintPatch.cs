using System.Buffers.Binary;
using System.Text;
namespace PrivateSwgLauncher;
public static partial class Patcher
{
    public static bool SprintMaster(string name) => name.EndsWith("_master", StringComparison.Ordinal) &&
        (name.StartsWith("combat_", StringComparison.Ordinal) || name.StartsWith("force_discipline_", StringComparison.Ordinal) ||
         new[] { "outdoors_scout_master", "outdoors_ranger_master", "outdoors_creaturehandler_master", "outdoors_squadleader_master", "science_combatmedic_master" }.Contains(name));

    // Preserve every original cell except the requested command/grant/icon fields.
    static byte[] SprintTable(byte[] data, Action<List<string>, List<string>, List<byte[][]>> edit)
    {
        var (columns, types, rows) = Table(data); int count = LE(data, rows.Offset + 8), p = rows.Offset + 12;
        if (count < 0 || count > 100000) throw new InvalidDataException("Invalid table row count.");
        List<byte[][]> entries = [];
        for (int i = 0; i < count; ++i) {
            var row = new byte[types.Count][];
            for (int c = 0; c < types.Count; ++c) {
                int start = p; if (types[c].StartsWith('s')) _ = CString(data, ref p); else p += 4;
                if (p > rows.Offset + 8 + rows.Size) throw new InvalidDataException("Truncated Sprint table.");
                row[c] = data[start..p];
            }
            entries.Add(row);
        }
        if (p != rows.Offset + 8 + rows.Size) throw new InvalidDataException("Invalid Sprint table boundary.");
        edit(columns, types, entries);
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
    static string SprintText(byte[] bytes) => Encoding.UTF8.GetString(bytes).TrimEnd('\0');
    static byte[] SprintCell(string text) => Encoding.UTF8.GetBytes(text + "\0");
    public static byte[] SprintSkills(byte[] data) => SprintTable(data, (columns, types, rows) => {
        int name = columns.IndexOf("NAME"), commands = columns.IndexOf("COMMANDS");
        if (name != 0 || commands != 21 || !types[commands].StartsWith('s')) throw new InvalidDataException("Unexpected skill schema.");
        int matched = 0;
        foreach (var row in rows) if (SprintMaster(SprintText(row[name]))) {
            var grants = SprintText(row[commands]).Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!grants.Contains("sprint")) grants.Add("sprint"); row[commands] = SprintCell(string.Join(',', grants)); matched++;
        }
        if (matched != 22) throw new InvalidDataException("Unexpected combat master skill set.");
    });
    public static byte[] SprintCommands(byte[] data) => SprintTable(data, (columns, types, rows) => {
        int name = columns.IndexOf("commandName"); if (name != 0) throw new InvalidDataException("Unexpected command schema.");
        var templates = rows.Where(row => SprintText(row[0]).Equals("burstRun", StringComparison.OrdinalIgnoreCase)).ToList();
        if (templates.Count != 1 || rows.Count(row => SprintText(row[0]).Equals("sprint", StringComparison.OrdinalIgnoreCase)) > 1) throw new InvalidDataException("Invalid Sprint command templates.");
        rows.RemoveAll(row => SprintText(row[0]).Equals("sprint", StringComparison.OrdinalIgnoreCase));
        var entry = templates[0].Select(cell => (byte[])cell.Clone()).ToArray();
        void Text(string col, string value) { int i=columns.IndexOf(col); if(i<0||!types[i].StartsWith('s'))throw new InvalidDataException("Missing command text field."); entry[i]=SprintCell(value); }
        void Number(string col, int value) { int i=columns.IndexOf(col); if(i<0||types[i].StartsWith('s'))throw new InvalidDataException("Missing command numeric field."); entry[i]=BitConverter.GetBytes(value); }
        Text("commandName", "sprint"); Text("characterAbility", "sprint");
        foreach(var col in new[]{"scriptHook","failScriptHook","cppHook","failCppHook","tempScript","stringId"})Text(col, "");
        foreach(var col in new[]{"defaultTime","defaultPriority","targetType","maxRangeToTarget","godLevel","addToCombatQueue","callOnTarget","disabled"})Number(col, 0);
        Number("visible",1); rows.Add(entry);
    });
    public static byte[] SprintModifiers(byte[] data) => SprintTable(data, (columns, types, rows) => {
        if (!columns.SequenceEqual(new[]{"ModifierName","DisplayName","DisplayDescription","IconImageStyle"}) || types.Any(t=>t!="s")) throw new InvalidDataException("Unexpected modifier schema.");
        if(rows.Count(row=>SprintText(row[0])=="sprint")>1)throw new InvalidDataException("Duplicate Sprint modifier.");
        rows.RemoveAll(row=>SprintText(row[0])=="sprint");
        rows.Add(new[]{"sprint","@cmd_n:sprint","@cmd_d:sprint","/Styles.Icon.command.burstRun"}.Select(SprintCell).ToArray());
    });
    public static byte[] SprintStrings(byte[] data, string text)
    {
        using var input=new MemoryStream(data,false);using var reader=new BinaryReader(input,Encoding.UTF8,true);
        try {
            if(reader.ReadUInt32()!=0xabcd)throw new InvalidDataException("Invalid string file.");
            byte version=reader.ReadByte();uint next=reader.ReadUInt32(),count=reader.ReadUInt32();
            if(count>100000)throw new InvalidDataException("Invalid string count.");
            List<(uint Id,uint Crc,byte[] Text)> values=[];List<(uint Id,byte[] Name)> names=[];
            byte[] Read(int length){if(length<0||length>input.Length-input.Position)throw new InvalidDataException("Truncated string file.");return reader.ReadBytes(length);}
            var ids=new HashSet<uint>();
            for(uint i=0;i<count;i++){uint id=reader.ReadUInt32(),crc=reader.ReadUInt32(),length=reader.ReadUInt32();if(length>int.MaxValue/2||!ids.Add(id))throw new InvalidDataException("Invalid string entry.");values.Add((id,crc,Read((int)length*2)));}
            var named=new HashSet<uint>();
            for(uint i=0;i<count;i++){uint id=reader.ReadUInt32(),length=reader.ReadUInt32();if(length>int.MaxValue||!ids.Contains(id)||!named.Add(id))throw new InvalidDataException("Invalid string key.");names.Add((id,Read((int)length)));}
            if(input.Position!=input.Length)throw new InvalidDataException("Invalid string boundary.");
            var existing=names.Where(n=>Encoding.UTF8.GetString(n.Name)=="sprint").ToList();
            if(existing.Count>1)throw new InvalidDataException("Duplicate Sprint string.");
            uint key;
            if(existing.Count==1){key=existing[0].Id;int index=values.FindIndex(v=>v.Id==key);values[index]=(key,0,Encoding.Unicode.GetBytes(text));}
            else {key=ids.Count==0?next:Math.Max(next,checked(ids.Max()+1));next=checked(key+1);values.Add((key,0,Encoding.Unicode.GetBytes(text)));names.Add((key,Encoding.UTF8.GetBytes("sprint")));}
            using var output=new MemoryStream();using var writer=new BinaryWriter(output,Encoding.UTF8,true);
            writer.Write((uint)0xabcd);writer.Write(version);writer.Write(next);writer.Write((uint)values.Count);
            foreach(var value in values){writer.Write(value.Id);writer.Write(value.Crc);writer.Write((uint)(value.Text.Length/2));writer.Write(value.Text);}
            foreach(var name in names){writer.Write(name.Id);writer.Write((uint)name.Name.Length);writer.Write(name.Name);}
            return output.ToArray();
        }catch(EndOfStreamException e){throw new InvalidDataException("Truncated string file.",e);}
    }
}
