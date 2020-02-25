using Cethleann.Structure.Resource;

namespace Cethleann.Structure.KTID
{
    public struct NDBEntry
    {
        public ResourceSectionHeader SectionHeader { get; set; }
        public KTIDReference KTID { get; set; }
        public int Count { get; set; }
    }
}
