using System;
using System.Collections.Generic;
using Cethleann.Structure.KTID;
using JetBrains.Annotations;

namespace Cethleann.KTID
{
    [PublicAPI]
    public class UnimplementedType : IKTIDSystemType
    {
        public Memory<byte> Buffer { get; set; }

        public int Read(Span<byte> data, OBJDBEntry entryHeader)
        {
            Buffer = new Memory<byte>(data.ToArray());
            return data.Length;
        }

        public IEnumerable<string> Dump(NDB ndb, Dictionary<uint, string> filelist) => Array.Empty<string>();
    }
}
