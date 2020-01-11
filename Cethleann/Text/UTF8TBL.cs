using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Cethleann.Text
{
    /// <summary>
    ///     UTF-8 texture sheet character table
    /// </summary>
    [PublicAPI]
    public class UTF8TBL
    {
        /// <summary>
        /// </summary>
        public UTF8TBL()
        {
        }

        /// <summary>
        ///     Initialize with databuffer
        /// </summary>
        /// <param name="data"></param>
        public UTF8TBL(Span<byte> data)
        {
            for (var index = 0; index < data.Length / 4; index++) Characters.Add(Encoding.UTF8.GetChars(data.Slice(index * 4, 4).ToArray().Reverse().ToArray()).Last());
        }

        /// <summary>
        ///     Chars found in the table.
        /// </summary>
        public List<char> Characters { get; set; } = new List<char>();

        /// <summary>
        ///     Write table to buffer
        /// </summary>
        /// <returns></returns>
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
    }
}
