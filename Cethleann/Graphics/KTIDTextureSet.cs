using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.KTID;

namespace Cethleann.Graphics
{
    /// <summary>
    ///     KTID / KTIDSOBJDB Texture Set File
    /// </summary>
    public class KTIDTextureSet
    {
        /// <summary>
        ///     Parse from Buffer
        /// </summary>
        /// <param name="buffer"></param>
        public KTIDTextureSet(Span<byte> buffer)
        {
            var data = MemoryMarshal.Cast<byte, uint>(buffer);
            Textures = new KTIDReference[buffer.Length / 0x8];
            for (var i = 0; i < data.Length; i += 2)
            {
                var index = data[i];
                var ktid = data[i + 1];
                Textures[index] = ktid;
            }
        }

        /// <summary>
        ///     List of KTID References
        /// </summary>
        public KTIDReference[] Textures { get; set; }
    }
}
