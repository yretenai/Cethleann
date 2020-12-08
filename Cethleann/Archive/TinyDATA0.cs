using Cethleann.Structure.Archive;
using JetBrains.Annotations;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Cethleann.Archive
{
    /// <summary>
    ///     DATA0 is a list of information for which to read DATA1 with.
    /// </summary>
    [PublicAPI]
    public class TinyDATA0 : DATA0
    {
        /// <summary>
        ///     Reads a DATA0 file list from a path
        /// </summary>
        /// <param name="path">File path to read</param>
#pragma warning disable IDE0068 // Use recommended dispose pattern, reason: disposed in sub-method DATA0(Stream, bool) when bool leaveOpen is false.
        public TinyDATA0(string path) : this(File.OpenRead(path)) { }
#pragma warning restore IDE0068 // Use recommended dispose pattern

        /// <summary>
        ///     Reads a DATA0 file list from a stream
        /// </summary>
        /// <param name="stream">Binary Read-capable Stream of DATA0</param>
        /// <param name="leaveOpen">If true, won't dispose <paramref name="stream" /></param>
        public TinyDATA0(Stream stream, bool leaveOpen = false)
        {
            try
            {
                if (!stream.CanRead) throw new InvalidOperationException("Cannot read from stream!");

                var buffer = new Span<byte>(new byte[stream.Length]);
                stream.Read(buffer);
                var entries = MemoryMarshal.Cast<byte, TinyDATA0Entry>(buffer).ToArray();
                Entries = entries.Select(x => new DATA0Entry
                {
                    Offset = x.Offset,
                    UncompressedSize = x.Size,
                    CompressedSize = 0,
                    IsCompressed = false
                }).ToList();
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
    }
}
