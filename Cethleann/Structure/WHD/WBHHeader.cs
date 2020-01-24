using Cethleann.Structure.Resource;

namespace Cethleann.Structure.WHD
{
#pragma warning disable 1591
    public struct WBHHeader
    {
        public ResourceSectionHeader SectionHeader { get; set; }
        public WBHSoundbankType SoundbankType { get; set; }
    }
#pragma warning restore 1591
}
