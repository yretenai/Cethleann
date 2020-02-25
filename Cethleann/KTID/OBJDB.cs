using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.KTID;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.KTID
{
    [PublicAPI]
    public class OBJDB
    {
        public OBJDB(Span<byte> buffer, NDB ndb = default)
        {
            Header = MemoryMarshal.Read<OBJDBHeader>(buffer);
            var offset = Header.SectionHeader.Size;
            for (var i = 0; i < Header.Count; ++i)
            {
                var entry = MemoryMarshal.Read<OBJDBEntry>(buffer.Slice(offset));
                var size = entry.SectionHeader.Size - SizeHelper.SizeOf<OBJDBEntry>();
                var kodBuffer = size == 0 ? Span<byte>.Empty : buffer.Slice(offset + SizeHelper.SizeOf<OBJDBEntry>(), size);
                IKTIDSystemType instance;
                if (!TypeInfo.Instance.Value.TypeMap.TryGetValue(entry.TypeInfoKTID, out var typeInfoImpl))
                {
                    if (TypeInfo.Instance.Value.NagList.Add(entry.TypeInfoKTID)) Logger.Warn("KTID", $"KTID TypeInfo {entry.TypeInfoKTID:x8} ({(ndb.HashMap.TryGetValue(entry.TypeInfoKTID, out var typeInfoName) ? typeInfoName : "unnamed")}) is not implemented! Offset {offset:x}, size {entry.SectionHeader.Size:x}");

                    instance = new UnimplementedType();
                }
                else
                {
                    instance = (IKTIDSystemType) Activator.CreateInstance(typeInfoImpl);
                }

                instance.Read(kodBuffer, entry);
                Entries[entry.KTID] = (entry, instance);

                offset += entry.SectionHeader.Size;
                offset = offset.Align(4);
            }
        }

        public OBJDBHeader Header { get; set; }
        public Dictionary<KTIDReference, (OBJDBEntry entry, IKTIDSystemType instance)> Entries { get; set; } = new Dictionary<KTIDReference, (OBJDBEntry entry, IKTIDSystemType instance)>();
    }
}
