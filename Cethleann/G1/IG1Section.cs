using Cethleann.Structure.Resource;

namespace Cethleann.G1
{
    /// <summary>
    /// Boiler interface for G1 Sections.
    /// </summary>
    public interface IG1Section
    {
        /// <summary>
        /// Version implemented by this class
        /// </summary>
        public int SupportedVersion { get; }

        /// <summary>
        /// Section header for this section
        /// </summary>
        public ResourceSectionHeader Section { get; }
    }
}
