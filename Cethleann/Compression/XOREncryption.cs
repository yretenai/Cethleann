using System;
using System.Linq;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Compression
{
    /// <summary>
    ///     Encryption helper for TN games.
    /// </summary>
    [PublicAPI]
    public static class XOREncryption
    {
        /// <summary>
        ///     Mutate given key truth with parameters
        /// </summary>
        /// <param name="length"></param>
        /// <param name="truth"></param>
        /// <param name="multiplier"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public static Span<byte> Xor(uint length, byte[] truth, ulong multiplier, ulong divisor)
        {
            var mag = length * multiplier / divisor;
            var bytes = BitConverter.GetBytes(mag).Where(x => x != 0).Reverse().ToArray();
            Logger.Assert(bytes.Length <= 4, "bytes.Length <= 4");
            return Enumerable.Range(0, bytes.Length == 4 ? truth.Length : truth.Length * bytes.Length).Select((x, i) =>
            {
                unchecked
                {
                    return (byte) (truth[i % truth.Length] ^ bytes[i % bytes.Length]);
                }
            }).ToArray();
        }

        /// <summary>
        ///     En/decrypt data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Span<byte> Crypt(Span<byte> data, Span<byte> key)
        {
            var value = new byte[data.Length];
            for (var i = 0; i < data.Length; ++i)
            {
                var a = data[i];
                var b = key[i % key.Length];

                value[i] = a;

                if (a != 0 && a != b) value[i] ^= b;
            }

            return value;
        }
    }
}
