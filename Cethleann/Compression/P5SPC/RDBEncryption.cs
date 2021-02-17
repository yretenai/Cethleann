using System;
using System.Runtime.InteropServices;

namespace Cethleann.Compression.P5SPC
{
    /// <summary>
    /// Implements P5S PC RDB encryption/decryption.
    /// Only used on ZLib compressed data.
    /// </summary>
    public static class RDBEncryption
    {
        /// <summary>
        /// Encrypts a P5S PC RDB ZLib compressed file.
        /// </summary>
        /// <param name="data">The compressed data to encrypt.</param>
        /// <param name="ktid">The KTID of the RDB file.</param>
        public static void Encrypt(Span<byte> data, uint ktid)
            => Decrypt(data, ktid);

        /// <summary>
        /// Decrypts a P5S PC RDB ZLib compressed file.
        /// </summary>
        /// <param name="data">The compressed data to decrypt.</param>
        /// <param name="ktid">The KTID of the RDB file.</param>
        public static void Decrypt(Span<byte> data, uint ktid)
        {
            var key = DeriveKey(ktid);
            var blockSize = DecryptRDBBlockSize(data, key);

            while (blockSize > 0)
            {
                var block = data.Slice(4, (int) blockSize + 4);
                blockSize = DecryptRDBBlock(block, key);
                data = data[block.Length..];
            }
        }

        /// <summary>
        /// Derives an RDB encryption key from a provided seed value.
        /// </summary>
        /// <param name="seed">Value to derive from.</param>
        /// <returns>The derived key.</returns>
        private static uint DeriveKey(uint seed)
        {
            uint count = (uint) ((byte) seed >> 5);
            if (count == 0) count = 1;

            return XORShift32.Get(~seed, count);
        }

        /// <summary>
        /// Decrypts the first 4 bytes of the provided RDB ZLib compressed data block.
        /// The returned value is used to initialize the decryption process.
        /// </summary>
        /// <param name="data">The block to decrypt from.</param>
        /// <param name="key">The key to use.</param>
        /// <returns>The initial block size.</returns>
        private static uint DecryptRDBBlockSize(Span<byte> data, uint key)
        {
            data[0] ^= (byte) key;
            data[1] ^= (byte) XORShift32.Get(key, 1);
            data[2] ^= (byte) XORShift32.Get(key, 2);
            data[3] ^= (byte) XORShift32.Get(key, 3);

            return MemoryMarshal.Read<uint>(data);
        }

        /// <summary>
        /// Decrypts an RDB ZLib compressed block using the provided key.
        /// The next block size is also decrypted and returned.
        /// </summary>
        /// <param name="data">The block to decrypt.</param>
        /// <param name="key">The key to use.</param>
        /// <returns>The next block size.</returns>
        private static uint DecryptRDBBlock(Span<byte> data, uint key)
        {
            var count = 0;

            var xs = new XORShift32(key);
            var keyXor = key;

            // decrypt block data
            if (data.Length != 4)
            {
                do
                {
                    data[count] ^= (byte) keyXor;
                    keyXor = xs.Next();
                }
                while (++count < data.Length - 4);
            }

            xs.Init(key);
            keyXor = key;

            // decrypt next block size
            if (count < data.Length)
            {
                do
                {
                    data[count] ^= (byte) keyXor;
                    keyXor = xs.Next();
                }
                while (++count < data.Length);
            }

            return MemoryMarshal.Read<uint>(data.Slice(count - 4, 4));
        }
    }

    /// <summary>
    /// Implements a XORShift32 PRNG.
    /// </summary>
    public class XORShift32 
    {
        uint state;

        /// <summary>
        /// Initializes a XORShift32 PRNG object with the provided seed.
        /// </summary>
        /// <param name="seed">A (non-zero) seed value.</param>
        public XORShift32(uint seed)
        {
            Init(seed);
        }

        /// <summary>
        /// Initializes the XORShift32 PRNG object with the provided seed.
        /// </summary>
        /// <param name="seed">A (non-zero) seed value.</param>
        public void Init(uint seed)
        {
            state = seed;
        }

        /// <summary>
        /// Calculates and returns the next generator state.
        /// </summary>
        /// <param name="cycles">The number of cycles to perform.</param>
        /// <returns>New generator state.</returns>
        public uint Next(uint cycles = 1)
        {
            state = Get(state, cycles);
            return state;
        }

        /// <summary>
        /// Performs XORShift32 cycles on a given seed.
        /// </summary>
        /// <param name="seed">The initial state.</param>
        /// <param name="cycles">The number of XORShift32 cycles to perform.</param>
        /// <returns>The state after all cycles.</returns>
        public static uint Get(uint seed, uint cycles = 1)
        {
            while (cycles-- > 0)
            {
                seed ^= seed << 13;
                seed ^= seed >> 17;
                seed ^= seed << 5;
            }

            return seed;
        }
    }
}
