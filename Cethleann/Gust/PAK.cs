using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Gust
{
    /// <summary>
    ///     Atelier PAK files
    /// </summary>
    [PublicAPI]
    public class PAK : IDisposable
    {
        /// <summary>
        ///     Initialize with a path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="a18">set to true if the PAKs are from Atelier Sophie or a newer Atelier</param>
        public PAK(string path, bool a18 = true) : this(File.OpenRead(path), a18) { }

        /// <summary>
        ///     Initialize with a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="a18">set to true if the PAKs are from Atelier Sophie or a newer Atelier</param>
        public PAK(Stream stream, bool a18 = true)
        {
            Is64Bit = a18;
            BaseStream = stream;
            EnsureCanRead();

            var blob = new Span<byte>(new byte[SizeHelper.SizeOf<PAKHeader>()]);
            BaseStream.Read(blob);
            Header = MemoryMarshal.Read<PAKHeader>(blob);

            Logger.Assert(Header.Version == 0x20000, "Header.Version == 0x20000", "Version mismatch");
            Logger.Assert(Header.HeaderSize == 0x10, "Header.HeaderSize != 0x10", "Signature mismatch");
            Logger.Assert(Header.Flags == 0x0D, "Header.Flags == 0x0D", "Flags mismatch");
            Logger.Assert(Header.FileCount <= 0x4000, "Header.FileCount <= 0x4000", "Too many files");

            Entries = new List<PAKEntry>(Header.FileCount);

            var entrySize = Is64Bit ? 0xA8 : 0xA0;
            blob = new Span<byte>(new byte[entrySize * Header.FileCount]);
            BaseStream.Read(blob);
            DataStart = BaseStream.Position;

            var cursor = 0;
            for (var i = 0; i < Header.FileCount; ++i)
            {
                var entryBlob = blob.Slice(cursor, entrySize);
                cursor += entrySize;

                var filenameBlob = entryBlob.Slice(0, 0x80);
                var size = MemoryMarshal.Read<int>(entryBlob.Slice(0x80));
                var key = entryBlob.Slice(0x84, 20).ToArray();
                var infoBlob = entryBlob.Slice(0x98);
                var info = Is64Bit ? MemoryMarshal.Cast<byte, long>(infoBlob).ToArray() : (MemoryMarshal.Cast<byte, int>(infoBlob).ToArray().Select(x => (long) x).ToArray());
                var encrypted = key.Any(x => x != 0);
                if (encrypted) Recode(filenameBlob, key);

                var filename = filenameBlob.ReadString();

                Entries.Add(new PAKEntry
                {
                    Filename = filename,
                    Size = size,
                    Key = key,
                    Offset = info[0],
                    Flags = info[1],
                    IsEncrypted = encrypted
                });
            }
        }

        /// <summary>
        ///     Header of the PAK file
        /// </summary>
        public PAKHeader Header { get; set; }

        /// <summary>
        ///     Entries found in the PAK
        /// </summary>
        public List<PAKEntry> Entries { get; set; }

        private long DataStart { get; set; }

        /// <summary>
        ///     Is the file 64-bit? Atelier games at and after Sophie are.
        /// </summary>
        public bool Is64Bit { get; set; }

        /// <summary>
        ///     The underlying file stream
        /// </summary>
        public Stream BaseStream { get; set; }

        /// <inheritdoc />
        public void Dispose() => BaseStream?.Dispose();

        /// <summary>
        ///     Disposes
        /// </summary>
        ~PAK()
        {
            Dispose();
        }

        /// <summary>
        ///     Gets an entry from it's filepath.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool TryGetEntry(string path, out PAKEntry entry)
        {
            entry = Entries.FirstOrDefault(x => x.Filename == path);
            return entry.Filename != null && entry.Filename == path;
        }

        /// <summary>
        ///     Read entry via entry struct
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public Memory<byte> ReadEntry(PAKEntry entry)
        {
            if (entry.Size == 0) return new Memory<byte>();
            EnsureCanRead();
            var blob = new Memory<byte>(new byte[entry.Size]);
            BaseStream.Position = DataStart + entry.Offset;
            BaseStream.Read(blob.Span);
            if (entry.IsEncrypted) Recode(blob.Span, entry.Key);

            return blob;
        }

        /// <summary>
        ///     Crypt rotate the data blob
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        public static void Recode(Span<byte> data, byte[] key)
        {
            var kl = key.Length;
            for (var i = 0; i < data.Length; ++i) data[i] ^= key[i % kl];
        }

        private void EnsureCanRead()
        {
            if (!Logger.Assert(BaseStream.CanRead, "BaseStream.CanRead == true", "Can't read data stream")) return;
            Dispose();
            throw new ObjectDisposedException(nameof(BaseStream));
        }
    }
}
