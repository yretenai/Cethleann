using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Texture;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Graphics
{
    /// <summary>
    ///     KSLT parser
    /// </summary>
    [PublicAPI]
    public class KSLT
    {
        /// <summary>
        ///     Initialize with byte data
        /// </summary>
        /// <param name="data"></param>
        public KSLT(Span<byte> data)
        {
            Header = MemoryMarshal.Read<ScreenLayoutTextureHeader>(data);
            var offset = SizeHelper.SizeOf<ScreenLayoutTextureHeader>();
            Matrices = MemoryMarshal.Cast<byte, ScreenLayoutTextureMatrix>(data.Slice(offset, SizeHelper.SizeOf<ScreenLayoutTextureMatrix>() * Header.Count)).ToArray();
            offset += Header.PointerTablePointer;
            Pointers = MemoryMarshal.Cast<byte, ScreenLayoutTexturePointer>(data.Slice(offset, SizeHelper.SizeOf<ScreenLayoutTexturePointer>() * Header.Count)).ToArray();
            offset += SizeHelper.SizeOf<ScreenLayoutTexturePointer>() * Header.Count;
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
                var header = MemoryMarshal.Read<ScreenLayoutTextureImage>(data.Slice(offset));
                var blob = new Memory<byte>(data.Slice(offset + SizeHelper.SizeOf<ScreenLayoutTextureImage>(), header.Size).ToArray());
                Entries.Add((header, blob));
            }
        }


        /// <summary>
        ///     KSLT Header data
        /// </summary>
        public ScreenLayoutTextureHeader Header { get; set; }

        /// <summary>
        ///     Matrix section
        /// </summary>
        public ScreenLayoutTextureMatrix[] Matrices { get; set; }

        /// <summary>
        ///     Image pointers
        /// </summary>
        public ScreenLayoutTexturePointer[] Pointers { get; set; }

        /// <summary>
        ///     Image names
        /// </summary>
        public List<string> Names { get; set; } = new List<string>();

        /// <summary>
        ///     Images
        /// </summary>
        public List<(ScreenLayoutTextureImage header, Memory<byte> data)> Entries { get; set; } = new List<(ScreenLayoutTextureImage, Memory<byte>)>();
    }
}
