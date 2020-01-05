using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Texture;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann
{
    /// <summary>
    ///     KSLT parser
    /// </summary>
    [PublicAPI]
    public class ScreenLayoutTexture
    {
        /// <summary>
        ///     Initialize with byte data
        /// </summary>
        /// <param name="data"></param>
        public ScreenLayoutTexture(Span<byte> data)
        {
            Header = MemoryMarshal.Read<KSLTHeader>(data);
            var offset = SizeHelper.SizeOf<KSLTHeader>();
            Matrices = MemoryMarshal.Cast<byte, KSLTMatrix>(data.Slice(offset, SizeHelper.SizeOf<KSLTMatrix>() * Header.Count)).ToArray();
            offset += Header.PointerTablePointer;
            Pointers = MemoryMarshal.Cast<byte, KSLTPointer>(data.Slice(offset, SizeHelper.SizeOf<KSLTPointer>() * Header.Count)).ToArray();
            offset += SizeHelper.SizeOf<KSLTPointer>() * Header.Count;
            Logger.Assert(Header.Count == Header.NameCount, "Header.FileCount == Header.NameCount");
            for (var i = 0; i < Header.NameCount; ++i)
            {
                var str = data.Slice(offset).ReadString();
                offset += (str?.Length ?? 0) + 1;
                Names.Add(str);
            }

            foreach (var pointer in Pointers)
            {
                offset = pointer.Pointer;
                var header = MemoryMarshal.Read<KSLTImage>(data.Slice(offset));
                var blob = new Memory<byte>(data.Slice(offset + SizeHelper.SizeOf<KSLTImage>(), header.Size).ToArray());
                Entries.Add((header, blob));
            }
        }


        /// <summary>
        ///     KSLT Header data
        /// </summary>
        public KSLTHeader Header { get; set; }

        /// <summary>
        ///     Matrix section
        /// </summary>
        public KSLTMatrix[] Matrices { get; set; }

        /// <summary>
        ///     Image pointers
        /// </summary>
        public KSLTPointer[] Pointers { get; set; }

        /// <summary>
        ///     Image names
        /// </summary>
        public List<string> Names { get; set; } = new List<string>();

        /// <summary>
        ///     Images
        /// </summary>
        public List<(KSLTImage header, Memory<byte> data)> Entries { get; set; } = new List<(KSLTImage, Memory<byte>)>();
    }
}
