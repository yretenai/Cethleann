using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Audio
{
    /// <summary>
    ///     Initialize and decrypt a KOVS stream
    /// </summary>
    [PublicAPI]
    public class KOVSSound
    {
        /// <summary>
        ///     Initialize KOVS data
        /// </summary>
        /// <param name="data"></param>
        public KOVSSound(Span<byte> data)
        {
            Header = MemoryMarshal.Read<KOVSHeader>(data);
            var offset = SizeHelper.SizeOf<KOVSHeader>();
            var blob = data.Slice(offset, Header.Size);
            if (blob.Length % 4 != 0)
            {
                var tmp = new Span<byte>(new byte[blob.Length.Align(4)]);
                blob.CopyTo(tmp);
                blob = tmp;
            }

            unsafe
            {
                var sz = (byte) Math.Min(0x100, blob.Length);
                fixed (byte* ptr = &blob.GetPinnableReference())
                {
                    for (var i = 0; i < sz; ++i)
                        *(ptr + i) ^= i;
                }
            }

            Stream = new Memory<byte>(blob.Slice(0, Header.Size).ToArray());
        }

        /// <summary>
        ///     KOVS Header
        /// </summary>
        public KOVSHeader Header { get; set; }

        /// <summary>
        ///     Audio Stream
        /// </summary>
        public Memory<byte> Stream { get; set; }
    }
}
