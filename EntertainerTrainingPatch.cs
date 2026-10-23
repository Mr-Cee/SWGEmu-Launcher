using System.Buffers.Binary;
using System.Text;
namespace PrivateSwgLauncher;
public static partial class Patcher
{
    public const string TrainingStringsPath="string/en/private_entertainer_training.stf";
    public const string TrainingCrcPath="misc/object_template_crc_string_table.iff";
    public const string TrainingGroupsPath="datatables/crafting/schematic_group.iff";
    public static string TrainingDraft(int tier,bool shared=true)=>"object/draft_schematic/instrument/"+(shared?"shared_":"")+"instrument_slitherhorn_ent_heal"+(tier==1?"":"_2")+".iff";
    public static string TrainingItem(int tier)=>"object/tangible/instrument/shared_slitherhorn_ent_heal"+(tier==1?"":"_2")+".iff";
    static byte[] TrainingChunk(byte[] data,Chunk chunk,byte[] payload) {
        int delta=payload.Length-chunk.Size;byte[] result=new byte[checked(data.Length+delta)];
        data.AsSpan(0,chunk.Offset+4).CopyTo(result);BinaryPrimitives.WriteInt32BigEndian(result.AsSpan(chunk.Offset+4,4),payload.Length);
        payload.CopyTo(result,chunk.Offset+8);data.AsSpan(chunk.Offset+8+chunk.Size).CopyTo(result.AsSpan(chunk.Offset+8+payload.Length));
        foreach(int parent in chunk.Parents)BinaryPrimitives.WriteInt32BigEndian(result.AsSpan(parent+4,4),checked(BE(data,parent+4)+delta));
        _=Chunks(result);return result;
    }
    static byte[] TrainingField(byte[] data,string name,byte[] value,byte[]? expected=null) {
        var key=Encoding.ASCII.GetBytes(name+"\0");
        var matches=Chunks(data).Where(c=>c.Tag=="XXXX"&&data.AsSpan(c.Offset+8,c.Size).StartsWith(key)&&
            (expected is null||data.AsSpan(c.Offset+8+key.Length,c.Size-key.Length).SequenceEqual(expected))).ToList();
        if(matches.Count!=1)throw new InvalidDataException("Unexpected entertainer training template field: "+name);
        return TrainingChunk(data,matches[0],[..key,..value]);
    }
    static byte[] TrainingStringId(string key)=>[1,1,..Encoding.ASCII.GetBytes("private_entertainer_training"),0,1,..Encoding.ASCII.GetBytes(key),0];
    static byte[] TrainingString(string text)=>[1,..Encoding.ASCII.GetBytes(text),0];
    public static byte[] TrainingTemplate(byte[] original,int tier,bool draft) {
        if(tier is not (1 or 2))throw new ArgumentOutOfRangeException(nameof(tier));
        string suffix=tier==1?"":"_2",key="slitherhorn_ent_heal"+suffix;
        var data=TrainingField(original,"objectName",TrainingStringId(key));
        data=TrainingField(data,"detailedDescription",TrainingStringId(key+"_desc"));
        if(draft) {
            data=TrainingField(data,"craftedSharedTemplate",TrainingString(TrainingItem(tier)));
            // This unique DSSA value belongs to the original 90-XP attribute.
            byte[] oldXp=[3,0x20,..BitConverter.GetBytes(90),..BitConverter.GetBytes(90)];
            int xp=tier==1?500:1000;
            data=TrainingField(data,"value",[3,0x20,..BitConverter.GetBytes(xp),..BitConverter.GetBytes(xp)],oldXp);
        } else {
            data=TrainingField(data,"gameObjectType",[1,0x20,..BitConverter.GetBytes(0x2013)]); // generic item
            data=TrainingField(data,"arrangementDescriptorFilename",TrainingString(""));
            data=TrainingField(data,"slotDescriptorFilename",TrainingString(""));
        }
        return data;
    }
    public static byte[] TrainingSkills(byte[] data)=>SprintTable(data,(columns,types,rows)=>{
        int field=columns.IndexOf("SCHEMATICS_GRANTED");
        if(field!=23||!types[field].StartsWith('s'))throw new InvalidDataException("Unexpected training skill schema.");
        int matched=0;
        foreach(var row in rows) {
            string name=SprintText(row[0]);
            string? group=name=="social_entertainer_novice"?"entertainer_healing_crafting_1":
                name is "social_dancer_novice" or "social_musician_novice"?"entertainer_healing_crafting_2":null;
            if(group is null)continue;
            var groups=SprintText(row[field]).Split(',',StringSplitOptions.RemoveEmptyEntries).ToList();
            if(!groups.Contains(group))groups.Add(group);
            row[field]=SprintCell(string.Join(',',groups));matched++;
        }
        if(matched!=3)throw new InvalidDataException("Missing entertainer training novice skills.");
    });
    public static byte[] TrainingGroups(byte[] data)=>SprintTable(data,(columns,types,rows)=>{
        if(!columns.SequenceEqual(new[]{"GroupId","SchematicName"})||types.Any(t=>t!="s"))throw new InvalidDataException("Unexpected schematic group schema.");
        var owned=new[]{TrainingDraft(1,false),TrainingDraft(2,false)};
        rows.RemoveAll(row=>owned.Contains(SprintText(row[1])));
        for(int tier=1;tier<=2;tier++)rows.Add(new[]{"entertainer_healing_crafting_"+tier,TrainingDraft(tier,false)}.Select(SprintCell).ToArray());
    });
    public static uint TrainingCrc(string text) {
        uint crc=0xffffffff;
        foreach(byte value in Encoding.ASCII.GetBytes(text)) {
            crc^=(uint)value<<24;
            for(int i=0;i<8;i++)crc=(crc&0x80000000)!=0?(crc<<1)^0x04c11db7:crc<<1;
        }
        return ~crc;
    }
    public static byte[] TrainingCrcTable(byte[] data) {
        Chunk Get(string tag)=>Chunks(data).Single(c=>c.Tag==tag);
        var countChunk=Get("DATA");if(countChunk.Size!=4)throw new InvalidDataException("Invalid template count.");
        int count=LE(data,countChunk.Offset+8);
        var crcs=Get("CRCT");var offsets=Get("STRT");var strings=Get("STNG");
        if(count<0||count>1000000||crcs.Size!=count*4||offsets.Size!=count*4)throw new InvalidDataException("Invalid template table.");
        var entries=new SortedDictionary<uint,string>();
        for(int i=0;i<count;i++){
            uint crc=BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(crcs.Offset+8+i*4,4));
            int relative=LE(data,offsets.Offset+8+i*4);
            if(relative<0||relative>=strings.Size)throw new InvalidDataException("Invalid template offset.");
            int pos=strings.Offset+8+relative;string name=CString(data,ref pos);
            if(pos>strings.Offset+8+strings.Size||!entries.TryAdd(crc,name))throw new InvalidDataException("Invalid template string.");
        }
        foreach(string name in new[]{TrainingDraft(1),TrainingDraft(2),TrainingItem(1),TrainingItem(2)}) {
            uint crc=TrainingCrc(name);
            if(entries.TryGetValue(crc,out var old)&&old!=name)throw new InvalidDataException("Training template CRC collision.");
            entries[crc]=name;
        }
        using var crcStream=new MemoryStream();using var offsetStream=new MemoryStream();using var stringStream=new MemoryStream();
        using var cw=new BinaryWriter(crcStream);using var ow=new BinaryWriter(offsetStream);using var sw=new BinaryWriter(stringStream);
        foreach(var entry in entries){cw.Write(entry.Key);ow.Write(checked((int)stringStream.Position));sw.Write(Encoding.ASCII.GetBytes(entry.Value));sw.Write((byte)0);}
        data=TrainingChunk(data,Get("DATA"),BitConverter.GetBytes(entries.Count));
        data=TrainingChunk(data,Get("CRCT"),crcStream.ToArray());
        data=TrainingChunk(data,Get("STRT"),offsetStream.ToArray());
        data=TrainingChunk(data,Get("STNG"),stringStream.ToArray());return data;
    }
    public static byte[] TrainingStrings() {
        var entries=new[]{
            ("slitherhorn_ent_heal","Slitherhorn [Ent Heal EXP]"),
            ("slitherhorn_ent_heal_2","Slitherhorn II [Ent Heal EXP]"),
            ("slitherhorn_ent_heal_desc","Entertainer healing practice prop. Cannot be played as an instrument. Crafting awards 500 base entertainer healing XP. Requires Novice Entertainer; 30 metal frame and 15 metal valving."),
            ("slitherhorn_ent_heal_2_desc","Advanced entertainer healing practice prop. Cannot be played as an instrument. Crafting awards 1,000 base entertainer healing XP. Requires Novice Dancer or Novice Musician; 45 metal frame and 23 metal valving.")
        };
        using var stream=new MemoryStream();using var w=new BinaryWriter(stream);
        w.Write((uint)0xabcd);w.Write((byte)0);w.Write((uint)5);w.Write((uint)entries.Length);
        for(int i=0;i<entries.Length;i++){w.Write((uint)(i+1));w.Write((uint)0);w.Write((uint)entries[i].Item2.Length);w.Write(Encoding.Unicode.GetBytes(entries[i].Item2));}
        for(int i=0;i<entries.Length;i++){w.Write((uint)(i+1));byte[] key=Encoding.ASCII.GetBytes(entries[i].Item1);w.Write((uint)key.Length);w.Write(key);}
        return stream.ToArray();
    }
}
