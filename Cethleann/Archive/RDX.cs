using Cethleann.Structure;
using Cethleann.Structure.KTID;
using DragonLib;
using DragonLib.IO;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Cethleann.Archive
{
    // why
    public class RDX
    {
        public RDXEntry[] PortableEntries { get; }
        public List<(RDBEntry, KTIDReference fdata, int offset)> Entries { get; } = new List<(RDBEntry, KTIDReference fdata, int offset)>();
        public Dictionary<KTIDReference, int> KTIDToEntryId { get; set; } = new Dictionary<KTIDReference, int>();

        public RDX(Span<byte> buffer, string directory)
        {;
            Directory = directory;
            PortableEntries = MemoryMarshal.Cast<byte, RDXEntry>(buffer).ToArray();

            var entryBuffer = new RDBEntry[1].AsSpan();
            foreach (var entry in PortableEntries)
            {
                var path = Path.Combine(directory, $"0x{entry.PortableId.KTID:x8}.fdata");
                Logger.Info("RDX", $"Loading {entry.PortableId.KTID:x8}");
                if (!File.Exists(path))
                {
                    continue; // rip lol
                }

                try
                {
                    using var stream = File.OpenRead(path);
                    var fdata = new byte[0x10].AsSpan();
                    stream.Read(fdata);
                    if (fdata.GetDataType() != DataType.PortableRDB)
                    {
                        continue;
                    }

                    stream.Position = BinaryPrimitives.ReadInt32LittleEndian(fdata.Slice(0x8));
                    while (stream.Position < stream.Length)
                    {
                        var pos = stream.Position;
                        stream.Read(MemoryMarshal.AsBytes(entryBuffer));
                        if (entryBuffer[0].Magic != DataType.RDBIndex)
                        {
                            // rip lol
                            break;
                        }

                        KTIDToEntryId[entryBuffer[0].FileKTID] = Entries.Count;
                        Entries.Add((entryBuffer[0], entry.PortableId, (int) pos));
                        stream.Position = (pos + entryBuffer[0].EntrySize).Align(16);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("RDX", e);
                }
            }
        }

        public Memory<byte> ReadEntry(int index)
        {
            var (entry, id, pos) = Entries[index];
            var path = Path.Combine(Directory, $"0x{id.KTID:x8}.fdata");
            if (!File.Exists(path))
            {
                return Memory<byte>.Empty;
            }

            using var stream = File.OpenRead(path);
            var buffer = new byte[entry.EntrySize].AsMemory();
            stream.Position = pos;
            stream.Read(buffer.Span);
            return buffer;
        }

        public string Directory { get; set; }
    }
}
