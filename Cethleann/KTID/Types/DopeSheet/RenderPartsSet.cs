using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure.KTID;
using Cethleann.Structure.KTID.Types.DopeSheet;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.KTID.Types.DopeSheet
{
    [PublicAPI]
    [KTIDTypeInfo("TypeInfo::Object::DopeSheet::RenderParts::PartsSet")]
    public class RenderPartsSet : IKTIDSystemType
    {
        public RenderPartsSetHeader SetHeader { get; set; }
        private KTIDReference[] Parts { get; set; }

        public int Read(Span<byte> data, OBJDBEntry entryHeader)
        {
            SetHeader = MemoryMarshal.Read<RenderPartsSetHeader>(data);
            Parts = MemoryMarshal.Cast<byte, KTIDReference>(data.Slice(SizeHelper.SizeOf<RenderPartsSetHeader>(), SizeHelper.SizeOf<KTIDReference>() * SetHeader.Count)).ToArray();
            return SizeHelper.SizeOf<RenderPartsSetHeader>() + SizeHelper.SizeOf<KTIDReference>() * SetHeader.Count;
        }

        public IEnumerable<string> Dump(NDB ndb, Dictionary<uint, string> filelist)
        {
            var list = new List<string>
            {
                $"Count: {SetHeader.Count}",
                $"Unknown 1: {SetHeader.Unknown1:x8}",
                $"Unknown 1 Name: {SetHeader.Unknown1.GetName(ndb, filelist) ?? "unnamed"}",
                $"Parts: [{string.Join(", ", Parts.Select(x => $"{x:x8}"))}]",
                $"Part Names: [{string.Join(", ", Parts.Select(x => x.GetName(ndb, filelist) ?? "unnamed"))}]"
            };
            return list;
        }
    }
}
