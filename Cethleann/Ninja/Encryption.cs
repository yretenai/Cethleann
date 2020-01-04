using System;
using System.Linq;
using JetBrains.Annotations;

namespace Cethleann.Ninja
{
    /// <summary>
    ///     Encryption helper for TN games.
    /// </summary>
    [PublicAPI]
    public static class Encryption
    {
        /// <summary>
        ///     Mutate given key truth with parameters
        /// </summary>
        /// <param name="length"></param>
        /// <param name="truth"></param>
        /// <param name="multiplier"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public static Span<byte> Xor(uint length, Span<byte> truth, ulong multiplier, ulong divisor)
        {
            var mag = length * multiplier / divisor;
            var bytes = BitConverter.GetBytes(mag).Where(x => x != 0).Reverse().ToArray();
            return truth.ToArray().Select((x, i) =>
            {
                unchecked
                {
                    return (byte) (x ^ bytes[i % bytes.Length]);
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
