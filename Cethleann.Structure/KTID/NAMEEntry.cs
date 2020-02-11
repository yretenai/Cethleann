using Cethleann.Structure.Resource;

namespace Cethleann.Structure.KTID
{
    public struct NAMEEntry
    {
        public ResourceSectionHeader SectionHeader { get; set; }
        public uint KTID { get; set; }
        public int Count { get; set; }
    }
}
