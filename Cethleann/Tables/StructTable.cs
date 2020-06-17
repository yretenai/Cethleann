using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure.Table;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Tables
{
    /// <summary>
    ///     Parses a memory buffer by assuming it consists of fixed width structs.
    /// </summary>
    [PublicAPI]
    public class StructTable
    {
        /// <summary>
        ///     Initialize with given buffer
        /// </summary>
        /// <param name="buffer"></param>
        public StructTable(Span<byte> buffer)
        {
            var header = MemoryMarshal.Read<StructTableHeader>(buffer);
            for (var i = 0; i < header.Count; ++i) Entries.Add(buffer.Slice(SizeHelper.SizeOf<StructTableHeader>() + i * header.Size, header.Size).ToArray());
        }

        /// <summary>
        ///     Lsof Entries found in the struct table
        /// </summary>
        public List<Memory<byte>> Entries { get; } = new List<Memory<byte>>();

        /// <summary>
        ///     Cast all entries to the specified struct type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> Cast<T>() where T : struct => Entries.Select(x => MemoryMarshal.Read<T>(x.Span)).ToList();

        /// <summary>
        ///     Cast all entries to the specified struct type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public List<object?> Cast(Type t) =>
            Entries.Select(x =>
            {
                var cursor = 0;
                return SpanHelper.ReadStruct(x.Span, t, ref cursor);
            }).ToList();
    }
}
