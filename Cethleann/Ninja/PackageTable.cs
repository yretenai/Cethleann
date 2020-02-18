using System;
using System.Buffers.Binary;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Compression;
using Cethleann.Structure;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Ninja
{
    /// <summary>
    ///     ID Table parser for both versions
    /// </summary>
    [PublicAPI]
    public class PackageTable
    {
        /// <summary>
        ///     Initialize table with decryption constants
        /// </summary>
        /// <param name="data"></param>
        /// <param name="flags"></param>
        /// <param name="truth"></param>
        /// <param name="multiplier"></param>
        /// <param name="divisor"></param>
        public PackageTable(Span<byte> data, IDTableFlags flags = IDTableFlags.Encrypted | IDTableFlags.Compressed, byte[] truth = null, ulong multiplier = 0, ulong divisor = 0)
        {
            if (flags != IDTableFlags.None)
            {
                var size = BinaryPrimitives.ReadUInt32LittleEndian(data);
                data = Read(data.Slice(4), size, flags, truth, multiplier, divisor);
            }

            Buffer = new Memory<byte>(data.ToArray());
            if (data.GetDataType() == DataType.PackageInfo)
            {
                PackageInfo = new PKGINFO(data);
                // There's extra shit that I can't parse yet because the format is highly dependent on arbitrary values.
                Header = new IDTableHeader
                {
                    Offset = PackageInfo.Resource.Pointers[1] + PackageInfo.InfoTable.Header.TextPointer + SizeHelper.SizeOf<BTIFHeader>()
                };
                Entries = PackageInfo.InfoTable.Entries.Select(x => new IDTableEntry
                {
                    PathOffset = x.NameOffset,
                    OriginalPathOffset = -1,
                    Flags = x.Flags,
                    Checksum = x.Checksum,
                    CompressedSize = x.CompressedSize,
                    DecompressedSize = x.DecompressedSize
                }).ToArray();
            }
            else
            {
                Header = MemoryMarshal.Read<IDTableHeader>(data);
                Entries = MemoryMarshal.Cast<byte, IDTableEntry>(data.Slice(Header.Offset, Header.Count * SizeHelper.SizeOf<IDTableEntry>())).ToArray();
            }
        }

        /// <summary>
        ///     Loaded PKGINFO
        /// </summary>
        public PKGINFO PackageInfo { get; set; }
        
        /// <summary>
        ///     File ID table entries
        /// </summary>
        public IDTableEntry[] Entries { get; set; }

        /// <summary>
        ///     Header of the file
        /// </summary>
        public IDTableHeader Header { get; set; }

        /// <summary>
        ///     Raw table
        /// </summary>
        public Memory<byte> Buffer { get; set; }

        /// <summary>
        ///     Read a file with decryption constants using an id table entry
        /// </summary>
        /// <param name="file"></param>
        /// <param name="entry"></param>
        /// <param name="truth"></param>
        /// <param name="multiplier"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public Span<byte> Read(Span<byte> file, IDTableEntry entry, byte[] truth, ulong multiplier, ulong divisor)
        {
            return Read(file, entry.DecompressedSize, entry.Flags, truth, multiplier, divisor);
        }

        /// <summary>
        ///     Read a file with decryption constants
        /// </summary>
        /// <param name="file"></param>
        /// <param name="size"></param>
        /// <param name="flags"></param>
        /// <param name="truth"></param>
        /// <param name="multiplier"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public Span<byte> Read(Span<byte> file, uint size, IDTableFlags flags, byte[] truth, ulong multiplier, ulong divisor)
        {
            if (flags.HasFlag(IDTableFlags.Encrypted))
            {
                var key = Encryption.Xor(size, truth, multiplier, divisor);
                file = Encryption.Crypt(file, key);
            }

            // ReSharper disable once InvertIf
            if (flags.HasFlag(IDTableFlags.Compressed))
            {
                var decompressedData = Stream8000Compression.Decompress(file, (int) size);
                if (decompressedData.Length != 0) file = decompressedData;
            }

            Logger.Assert(flags == IDTableFlags.Compressed || flags == IDTableFlags.Encrypted || flags == (IDTableFlags.Compressed | IDTableFlags.Encrypted) || flags == IDTableFlags.None, "Flags == Compressed || Flags == Encrypted || Flags == Compresed | Encrypted || Flags == None");

            return file;
        }
    }
}
