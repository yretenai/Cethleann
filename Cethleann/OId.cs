using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann
{
    /// <summary>
    ///     Object ID / Bone ID list
    /// </summary>
    [PublicAPI]
    public class OId
    {
        /// <summary>
        ///     Initialize with buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="encoding"></param>
        public OId(Span<byte> buffer, Encoding encoding = null)
        {
            var pointer = 0;
            while (pointer < buffer.Length)
            {
                var size = buffer[pointer];
                pointer += 1;
                var str = default(string);
                if (size == 0xFF) break;
                if (size > 0)
                {
                    str = (encoding ?? Encoding.UTF8).GetString(buffer.Slice(pointer, size));
                    pointer += str?.Length ?? 0;
                }

                Names.Add(str);
            }
        }

        /// <summary>
        ///     lsof string names
        /// </summary>
        public List<string> Names { get; set; } = new List<string>();

        /// <summary>
        ///     Write to buffer
        /// </summary>
        /// <returns></returns>
        public Span<byte> Write()
        {
            var buffer = new Span<byte>(new byte[Names.Sum(x => (x?.Length ?? 0) + 1) + 1]);
            var pointer = 0;
            foreach (var name in Names)
            {
                buffer[pointer] = (byte) (name?.Length ?? 0);
                pointer += 1;
                if (name == null) continue;
                name.ToSpan().CopyTo(buffer.Slice(pointer));
                pointer += name.Length;
            }

            buffer[pointer] = 0xFF;

            return buffer;
        }
    }
}
