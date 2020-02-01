using System;
using System.IO;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Omega
{
    /// <summary>
    ///     LINKDATA_*.BIN Parser
    /// </summary>
    [PublicAPI]
    public class LINKDATA : IDisposable
    {
        /// <summary>
        ///     Parse from Binary Stream
        /// </summary>
        /// <param name="stream"></param>
        public LINKDATA(Stream stream)
        {
            DataStream = stream;
            var buffer = new Span<byte>(new byte[SizeHelper.SizeOf<LINKDATAHeader>()]);
            stream.Read(buffer);
            Header = MemoryMarshal.Read<LINKDATAHeader>(buffer);
            buffer = new Span<byte>(new byte[Header.EntryCount * SizeHelper.SizeOf<LINKDATAEntry>()]);
            stream.Read(buffer);
            Entries = MemoryMarshal.Cast<byte, LINKDATAEntry>(buffer).ToArray();
        }

        /// <summary>
        ///     Underlying Data Stream
        /// </summary>
        public Stream DataStream { get; }

        /// <summary>
        ///     Lsof LINKDATA entries
        /// </summary>
        public LINKDATAEntry[] Entries { get; set; }

        /// <summary>
        ///     Linkdata Header
        /// </summary>
        public LINKDATAHeader Header { get; set; }

        /// <summary>
        ///     Dispose of <seealso cref="DataStream" />
        /// </summary>
        public void Dispose() => DataStream.Dispose();

        /// <summary>
        ///     Read file entry
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public Memory<byte> ReadEntry(LINKDATAEntry entry)
        {
            var pointer = entry.Offset * Header.OffsetMultiplier;
            var buffer = new Span<byte>(new byte[entry.Size]);
            DataStream.Position = pointer;
            DataStream.Read(buffer);
            return entry.DecompressedSize > 0 ? new Memory<byte>(Compression.Decompress(buffer, entry.DecompressedSize).ToArray()) : new Memory<byte>(buffer.ToArray());
        }
    }
}
