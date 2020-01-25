using Cethleann.Structure.Resource;

namespace Cethleann.Structure.WHD
{
    public struct WBHHeader
    {
        public ResourceSectionHeader SectionHeader { get; set; }
        public WBHSoundbankType SoundbankType { get; set; }
    }
}
