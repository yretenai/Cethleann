using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.KTID;

namespace Cethleann.Graphics
{
    public class KTIDTextureSet
    {
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

        public KTIDReference[] Textures { get; set; }
    }
}
