using Cethleann.Structure.Resource;

namespace Cethleann.G1
{
    /// <summary>
    ///     Boiler interface for G1 Sections.
    /// </summary>
    public interface IG1Section
    {
        /// <summary>
        ///     Version implemented by this class
        /// </summary>
        int SupportedVersion { get; }

        /// <summary>
        ///     Section header for this section
        /// </summary>
        ResourceSectionHeader Section { get; }
    }
}
