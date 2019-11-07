using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using DragonLib.IO;

namespace Cethleann
{
    public class SCEN
    {
        public List<Memory<byte>> Entries { get; set; } = new List<Memory<byte>>();
        
        public SCEN(Span<byte> data)
        {
            var header = MemoryMarshal.Read<SCENHeader>(data);
            Logger.Assert(header.OffsetCount == header.SizeCount, "header.OffsetCount == header.SizeCount");
            Logger.Assert(header.Unknown1 == 0, "header.Unknown1 == 0");
            Logger.Assert(header.Unknown2 == 0, "header.Unknown2 == 0");
            Logger.Assert(header.Unknown3 == 1, "header.Unknown3 == 1");
            Logger.Assert(header.Unknown4 == 1, "header.Unknown4 == 1");
            Logger.Assert(header.Unknown5 == 0, "header.Unknown5 == 0");
            Logger.Assert(header.UnknownCount == 0, "header.UnknownCount == 0");
            Logger.Assert(header.UnknownTableOffset == 0, "header.UnknownTableOffset == 0");

            var offsets = MemoryMarshal.Cast<byte, int>(data.Slice(header.OffsetTableOffset, header.OffsetCount * 4));
            var sizes = MemoryMarshal.Cast<byte, int>(data.Slice(header.SizeTableOffset, header.SizeTableOffset * 4));
            for (var index = 0; index < offsets.Length; index++)
            {
                Entries.Add(new Memory<byte>(data.Slice(offsets[index], sizes[index]).ToArray()));
            }
        }
    }
}
