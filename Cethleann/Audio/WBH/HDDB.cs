using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Cethleann.Structure.Resource.Audio.WHD;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Audio.WBH
{
    /// <summary>
    ///     HDDB File Name Table
    /// </summary>
    [PublicAPI]
    public class HDDB
    {
        public HDDB()
        {
        }

        /// <summary>
        ///     Initialize with buffer and count
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="count"></param>
        public HDDB(Span<byte> buffer, int count)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Header = MemoryMarshal.Read<HDDBHeader>(buffer);
            Name = buffer.Slice(Header.NameTablePointer).ReadString(Encoding.GetEncoding(932));
            for (var i = 0; i < count; ++i)
            {
                var offset = Header.EntryTablePointer + i * Header.EntrySize;
                var pointers = MemoryMarshal.Cast<byte, int>(buffer.Slice(offset, Header.EntrySize));
                var strings = new string[pointers.Length];
                for (var index = 0; index < pointers.Length; index++)
                {
                    var pointer = pointers[index];
                    if (pointer > 0) strings[index] = buffer.Slice(offset + pointer).ReadString(Encoding.GetEncoding(932)) ?? string.Empty;
                }

                Entries.Add(strings);
            }
        }

        /// <summary>
        ///     Underlying Header
        /// </summary>
        public HDDBHeader Header { get; set; }

        /// <summary>
        ///     Filename
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///     Lsof tags
        /// </summary>
        public List<string[]> Entries { get; set; } = new List<string[]>();
    }
}
