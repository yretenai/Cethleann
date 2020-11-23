using System;
using System.IO;
using System.Runtime.InteropServices;
using Cethleann.Structure.Archive;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Archive
{
    /// <summary>
    ///     Nioh-like ARCHIVE bin
    /// </summary>
    [PublicAPI]
    public class LINKARCHIVE : IDisposable
    {
        /// <summary>
        ///     Initialize from stream
        /// </summary>
        /// <param name="data"></param>
        public LINKARCHIVE(Stream data)
        {
            DataStream = data;
            var buffer = new Span<byte>(new byte[SizeHelper.SizeOf<LFMArchiveHeader>()]);
            DataStream.Read(buffer);
            Header = MemoryMarshal.Read<LFMArchiveHeader>(buffer);
            buffer = new Span<byte>(new byte[SizeHelper.SizeOf<LFMArchiveEntry>() * Header.Files]);
            DataStream.Read(buffer);
            Entries = MemoryMarshal.Cast<byte, LFMArchiveEntry>(buffer).ToArray();
        }

        /// <summary>
        ///     Entries found in the archive
        /// </summary>
        public LFMArchiveEntry[] Entries { get; set; }

        /// <summary>
        ///     Archive header
        /// </summary>
        public LFMArchiveHeader Header { get; set; }

        /// <summary>
        ///     Underlying data stream
        /// </summary>
        public Stream DataStream { get; set; }


        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(false);
        }

        protected void Dispose(bool disposing)
        {
            DataStream.Dispose();
        }

        /// <summary>
        ///     Read an entry from the data stream
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public Memory<byte> ReadEntry(LFMArchiveEntry entry)
        {
            var pointer = entry.Offset;
            var buffer = new Span<byte>(new byte[entry.Size]);
            DataStream.Position = pointer;
            DataStream.Read(buffer);
            return entry.IsCompressed != 0 ? throw new NotImplementedException() : new Memory<byte>(buffer.ToArray());
        }
    }
}
