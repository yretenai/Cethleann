using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Text
{
    /// <summary>
    ///     Text Localization files are used to collect text data.
    /// </summary>
    [PublicAPI]
    public class XL19
    {
        /// <summary>
        ///     Initialize with a span buffer
        /// </summary>
        /// <param name="buffer"></param>
        public XL19(Span<byte> buffer)
        {
            if (!buffer.Matches(DataType.TextLocalization19)) throw new InvalidOperationException("Not an XL 19 stream");

            var header = MemoryMarshal.Read<TextLocalizationHeader>(buffer);

            var offset = (int) header.TableOffset;
            var sz = SizeHelper.SizeOf<TextLocalizationHeader>();
            var ripHint = buffer.Slice(sz, header.Width / 4);
            for (var i = 0; i < header.Sets; ++i)
            {
                var set = new List<string>();
                for (var j = 0; j < header.Width / 4; ++j)
                {
                    var rip = MemoryMarshal.Read<int>(buffer.Slice(offset));
                    offset += SizeHelper.SizeOf<int>();
                    if (rip == -1 || ripHint[j] != 0)
                    {
                        set.Add(string.Empty);
                        continue;
                    }

                    set.Add(buffer.Slice(header.TableOffset + rip).ReadString(returnNull: false));
                }

                Entries.Add(set);
            }
        }

        /// <summary>
        ///     List of string entries in this text blob
        /// </summary>
        public List<List<string>> Entries { get; set; } = new List<List<string>>();
    }
}
