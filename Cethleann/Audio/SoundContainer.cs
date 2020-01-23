using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using JetBrains.Annotations;

namespace Cethleann.Audio
{
    /// <summary>
    ///     Parser for KTSC files
    /// </summary>
    [PublicAPI]
    public class SoundContainer
    {
        /// <summary>
        ///     Initialize with blob data
        /// </summary>
        /// <param name="data"></param>
        public SoundContainer(Span<byte> data)
        {
            Header = MemoryMarshal.Read<SoundContainerHeader>(data);
            Identifiers = MemoryMarshal.Cast<byte, int>(data.Slice(Header.IdTablePointer, 4 * Header.Count)).ToArray();
            Pointers = MemoryMarshal.Cast<byte, int>(data.Slice(Header.PointerTablePointer, 4 * Header.Count)).ToArray();
            foreach (var pointer in Pointers)
            {
                var header = MemoryMarshal.Read<SoundResourceHeader>(data.Slice(pointer));
                KTSR.Add(new Memory<byte>(data.Slice(pointer, header.CompressedSize).ToArray()));
            }
        }

        /// <summary>
        ///     KTSR Header
        /// </summary>
        public SoundContainerHeader Header { get; set; }

        /// <summary>
        ///     ?
        /// </summary>
        public int[] Identifiers { get; set; }

        /// <summary>
        ///     ?
        /// </summary>
        public int[] Pointers { get; set; }

        /// <summary>
        ///     Underlying KTSR
        /// </summary>
        public List<Memory<byte>> KTSR { get; set; } = new List<Memory<byte>>();
    }
}
