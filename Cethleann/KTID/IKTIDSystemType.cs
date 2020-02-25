using System;
using System.Collections.Generic;
using Cethleann.Structure.KTID;
using JetBrains.Annotations;

namespace Cethleann.KTID
{
    [PublicAPI]
    public interface IKTIDSystemType
    {
        public IEnumerable<string> Dump(NDB ndb, Dictionary<uint, string> filelist);
        public int Read(Span<byte> buffer, OBJDBEntry header);
    }
}
