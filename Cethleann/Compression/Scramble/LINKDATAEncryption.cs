using System;

namespace Cethleann.Compression.Scramble
{
    /// <summary>
    /// Implements Scramble LINKDATA file encryption/decryption.
    /// </summary>
    public static class LinkEncryption
    {
        /// <summary>
        /// Encrypts a Scramble LINKDATA file.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <param name="id">The ID of the LINKDATA file.</param>
        public static void Encrypt(Span<byte> data, uint id)
            => Decrypt(data, id);

        /// <summary>
        /// Decrypts a Scramble LINKDATA file.
        /// </summary>
        /// <param name="data">The data to decrypt.</param>
        /// <param name="id">The ID of the LINKDATA file.</param>
        public static void Decrypt(Span<byte> data, uint id)
        {
            var gen = new Mersenne(id + 0x7F6BA458);
            var size = data.Length;

            for (var i = 0; i < size; i++)
            {
                var shift = (size - i >= 2) && (gen.Next() & 1) != 0 ? 1 : 0;
                var r = gen.Next();

                data[i] ^= (byte) r;
                data[i + shift] ^= (byte) (r >> 8);
                i += shift;
            }
        }
    }

    /// <summary>
    /// Implements a Mersenne-ish PRNG.
    /// </summary>
    public class Mersenne
    {
        readonly uint[] State = new uint[4];

        /// <summary>
        /// Initializes a generator object using the provided seed value.
        /// </summary>
        /// <param name="seed">Value to initialize with.</param>
        public Mersenne(uint seed)
        {
            Init(seed);
        }

        /// <summary>
        /// Initializes the generator using a provided seed value.
        /// </summary>
        /// <param name="seed">Value to initialize with.</param>
        public void Init(uint seed)
        {
            State[0] = 0x6C078965 * (seed ^ (seed >> 30));

            for (int i = 1; i < 4; i++)
                State[i] = (uint) (0x6C078965 * (State[i - 1] ^ (State[i - 1] >> 30)) + i);
        }

        /// <summary>
        /// Generates and returns a value from the current generator state.
        /// </summary>
        /// <returns>Next generator state.</returns>
        public uint Next()
        {
            var temp = State[0] ^ (State[0] << 11);
            State[0] = State[1];
            State[1] = State[2];
            State[2] = State[3];
            State[3] ^= temp ^ ((temp ^ (State[3] >> 11)) >> 8);

            return State[3];
        }
    }
}
