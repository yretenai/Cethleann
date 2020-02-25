using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.KTID;
using Cethleann.Structure.KTID.Types.Render;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.KTID.Types.Render
{
    [PublicAPI]
    [KTIDTypeInfo("TypeInfo::Object::Render::Texture::Static")]
    public class StaticTexture : IKTIDSystemType
    {
        public StaticTextureData Data { get; set; }

        public int Read(Span<byte> data, OBJDBEntry entryHeader)
        {
            Data = MemoryMarshal.Read<StaticTextureData>(data);
            return SizeHelper.SizeOf<StaticTextureData>();
        }

        public IEnumerable<string> Dump(NDB ndb, Dictionary<uint, string> filelist)
        {
            return new List<string>
            {
                $"Unknown 1: {Data.Unknown1}",
                $"Unknown 2: {Data.Unknown2}",
                $"Unknown 3: {Data.Unknown3}",
                $"Unknown 4: {Data.Unknown4}",
                $"Unknown 5: {Data.Unknown5}",
                $"Unknown KTID: {Data.UnknownKTID:x8}",
                $"Unknown KTID Name: {Data.UnknownKTID.GetName(ndb, filelist) ?? "unnamed"}",
                $"Texture KTID: {Data.TextureKTID:x8}",
                $"Texture KTID Name: {Data.TextureKTID.GetName(ndb, filelist) ?? "unnamed"}"
            };
        }
    }
}
