using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Pack
{
    /// <summary>
    ///     Gust Elixir parser
    /// </summary>
    [PublicAPI]
    public class Elixir
    {
        /// <summary>
        ///     Initialize from buffer
        /// </summary>
        /// <param name="buffer"></param>
        public Elixir(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<ElixirHeader>(buffer);
            Logger.Assert(Header.Version <= 1, "Header.Version <= 1");
            var textLength = Header.Version == 1 ? 0x30 : 0x20;
            var ptr = Header.HeaderSize;
            for (var i = 0; i < Header.Count; ++i)
            {
                var entry = MemoryMarshal.Read<ElixirEntry>(buffer.Slice(ptr));
                ptr += SizeHelper.SizeOf<ElixirEntry>();
                var filename = buffer.Slice(ptr, textLength).ReadString();
                ptr += textLength;
                Entries.Add((entry, filename));
                Blobs.Add(entry.Size > 0 ? new Memory<byte>(buffer.Slice(entry.Offset, entry.Size).ToArray()) : Memory<byte>.Empty);
            }
        }

        /// <summary>
        ///     File Header
        /// </summary>
        public ElixirHeader Header { get; set; }

        /// <summary>
        ///     File Buffers
        /// </summary>
        public List<Memory<byte>> Blobs { get; set; } = new List<Memory<byte>>();

        /// <summary>
        ///     File Meta Entries
        /// </summary>
        public List<(ElixirEntry info, string filename)> Entries { get; set; } = new List<(ElixirEntry, string)>();
    }
}
