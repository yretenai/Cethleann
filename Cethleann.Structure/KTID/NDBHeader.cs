using Cethleann.Structure.Resource;

namespace Cethleann.Structure.KTID
{
    public struct NDBHeader
    {
        public ResourceSectionHeader SectionHeader { get; set; }
        public DataPlatform Platform { get; set; }
        public int Count { get; set; }
        public int TotalSize { get; set; }
    }
}
