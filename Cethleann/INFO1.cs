using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using DragonLib;

namespace Cethleann
{
    /// <summary>
    ///     INFO1 is a list of information of base files deleted with this patch.
    ///     Usually this means that the files are contained in a DLC.
    /// </summary>
    public class INFO1
    {
        /// <summary>
        ///     Reads a INFO1 file list from a path
        /// </summary>
        /// <param name="INFO2"></param>
        /// <param name="path">File path to read</param>
#pragma warning disable IDE0068 // Use recommended dispose pattern, reason: disposed in sub-method DATA0(Stream, bool) when bool leaveOpen is false.
        public INFO1(INFO2 INFO2, string path) : this(INFO2, File.OpenRead(path)) { }
#pragma warning restore IDE0068 // Use recommended dispose pattern

        /// <summary>
        ///     Reads a INFO1 file list from a stream
        /// </summary>
        /// <param name="INFO2"></param>
        /// <param name="stream"></param>
        /// <param name="leaveOpen"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public INFO1(INFO2 INFO2, Stream stream, bool leaveOpen = false)
        {
            try
            {
                if (!stream.CanRead) throw new InvalidOperationException("Cannot read from stream!");

                Entries = new List<(INFO1Entry entry, string path)>(INFO2.INFO1Count);
                var buffer = new Span<byte>(new byte[SizeHelper.SizeOf<INFO1Entry>() + 0x100]);
                for (int i = 0; i < INFO2.INFO1Count; ++i)
                {
                    stream.Read(buffer);
                    var entry = MemoryMarshal.Read<INFO1Entry>(buffer);
                    var path = buffer.Slice(SizeHelper.SizeOf<INFO1Entry>()).ReadString();
                    Entries.Add((entry, path));
                }
            }
            finally
            {
                if (!leaveOpen)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        /// <summary>
        ///     Entries found in the INFO1
        /// </summary>
        public List<(INFO1Entry entry, string path)> Entries { get; }
    }
}
