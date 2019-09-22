using Cethleann.Structure;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Cethleann
{
    public class TextLocalization
    {
        public IEnumerable<string> Entries { get; set; }

        public TextLocalization(Span<byte> buffer)
        {
            if (DATA0Helper.GuessType(buffer) != DataType.XL_19) throw new InvalidOperationException("Not an XL 19 stream");
            var header = MemoryMarshal.Read<TextLocalizationHeader>(buffer);

            Helper.Assert(header.Size == 0x14);
            Helper.Assert(header.Width == 4);

            var strings = new List<string>();
            for (var i = 0; i < header.Count; ++i)
            {
                var pin = buffer.Slice(header.Size + header.Width * i, header.Width);
                var offset = MemoryMarshal.Read<int>(pin);
                var size = 1;
                if (i < header.Count - 1)
                {
                    var peekPin = buffer.Slice(header.Size + header.Width * (i + 1), header.Width);
                    var peekOffset = MemoryMarshal.Read<int>(peekPin);
                    size = peekOffset - offset;
                }
                else
                {
                    size = buffer.Length - offset - header.Size;
                }

                if (size == 1)
                    strings.Add("");
                else
                    strings.Add(Encoding.UTF8.GetString(buffer.Slice(offset + header.Size, size - 1)).Split('\0')[0]);
            }
            Entries = strings;
        }
    }
}
