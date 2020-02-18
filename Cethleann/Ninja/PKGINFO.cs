using System;
using JetBrains.Annotations;

namespace Cethleann.Ninja
{
    /// <summary>
    ///     Package Info parser
    /// </summary>
    [PublicAPI]
    public class PKGINFO
    {
        public PKGINFO(Span<byte> buffer)
        {
            Resource = new RESPACK(buffer);
            InfoTable = new BTIF(Resource.Entries[1].Span);
        }

        public RESPACK Resource { get; set; }
        public BTIF InfoTable { get; set; }
    }
}
