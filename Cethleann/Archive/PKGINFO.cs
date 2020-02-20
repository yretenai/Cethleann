using System;
using Cethleann.Pack;
using JetBrains.Annotations;

namespace Cethleann.Archive
{
    /// <summary>
    ///     Package Info parser
    /// </summary>
    [PublicAPI]
    public class PKGINFO
    {
        /// <summary>
        ///     Initialize with buffer data
        /// </summary>
        /// <param name="buffer"></param>
        public PKGINFO(Span<byte> buffer)
        {
            Resource = new RESPACK(buffer);
            InfoTable = new BTIF(Resource.Entries[1].Span);
        }

        /// <summary>
        ///     underlying RESPACK resource
        /// </summary>
        public RESPACK Resource { get; set; }

        /// <summary>
        ///     Binary Table Information? Lol idk.
        /// </summary>
        public BTIF InfoTable { get; set; }
    }
}
