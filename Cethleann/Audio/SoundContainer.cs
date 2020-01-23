using System;
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
            KTSR = new SoundResource(data.Slice(Header.ResourcePointer));
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
        public SoundResource KTSR { get; set; }
    }
}
