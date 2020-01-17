using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Save.DFFNT;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Ninja
{
    /// <summary>
    ///     Parses Dissidia NT Free Save Data
    /// </summary>
    [PublicAPI]
    public class DFFNTSave
    {
        /// <summary>
        ///     Parse Dissidia Save Data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        public DFFNTSave(Span<byte> data, Span<byte> key)
        {
            var decrypted = CompressionEncryption.CryptAESCBC(data.Slice(0x18), key, Span<byte>.Empty);
            decrypted.CopyTo(data.Slice(0x18));
            Header = MemoryMarshal.Read<DFFNTSaveHeader>(data);
            SaveData = data.Slice(SizeHelper.SizeOf<DFFNTSaveHeader>()).ReadString() ?? "{}";
            TailByte = data[^1];
        }

        /// <summary>
        ///     Save Data Header
        /// </summary>
        public DFFNTSaveHeader Header { get; set; }

        /// <summary>
        ///     JSON Save Data
        /// </summary>
        public string SaveData { get; set; }

        /// <summary>
        ///     Last byte of the file.
        /// </summary>
        public byte TailByte { get; set; }

        /// <summary>
        ///     Write save data
        /// </summary>
        /// <returns></returns>
        public Span<byte> Write()
        {
            var buffer = new Span<byte>(new byte[0x300000 + SizeHelper.SizeOf<DFFNTSaveHeader>()]);
            var header = Header;
            MemoryMarshal.Write(buffer, ref header);
            SaveData.ToSpan().CopyTo(buffer.Slice(SizeHelper.SizeOf<DFFNTSaveHeader>()));
            buffer[^1] = TailByte;
            return buffer;
        }

        /// <summary>
        ///     Write save data and encrypt.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Span<byte> Write(Span<byte> key)
        {
            var buffer = Write();
            var crypted = CompressionEncryption.CryptAESCBC(buffer.Slice(0x18), key, Span<byte>.Empty);
            crypted.CopyTo(buffer.Slice(0x18));
            return buffer;
        }
    }
}
