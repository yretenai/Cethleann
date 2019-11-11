using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Cethleann
{
    public class UTF8TBL
    {
        public UTF8TBL()
        {
            
        }

        public UTF8TBL(Span<byte> data)
        {
            for (var index = 0; index < data.Length / 4; index++)
            {
                Characters.Add(Encoding.UTF8.GetChars(data.Slice(index * 4, 4).ToArray().Reverse().ToArray()).Last());
            }
        }

        public Span<byte> Write()
        {
            var buffer = new Span<byte>(new byte[Characters.Count * 4]);
            for (var index = 0; index < Characters.Count; index++)
            {
                var c = Characters[index];
                var bytes = new Span<byte>(Encoding.UTF8.GetBytes(c.ToString()).Reverse().ToArray());
                bytes.CopyTo(buffer.Slice(index * 4));
            }
            return buffer;
        }

        public List<char> Characters { get; set; } = new List<char>();
    }
}
