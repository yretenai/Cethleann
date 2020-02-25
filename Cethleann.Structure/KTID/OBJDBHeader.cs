using Cethleann.Structure.Resource;

namespace Cethleann.Structure.KTID
{
    public struct OBJDBHeader
    {
        public ResourceSectionHeader SectionHeader { get; set; }
        public DataSystem System { get; set; }
        public int Count { get; set; }
        public KTIDReference NameKTID { get; set; }
        public int TotalSize { get; set; }
    }
}
