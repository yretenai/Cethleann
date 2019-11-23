using Cethleann.Koei.Structure.Resource;
using JetBrains.Annotations;

namespace Cethleann.Koei.G1
{
    /// <summary>
    ///     Boiler interface for G1 Sections.
    /// </summary>
    [PublicAPI]
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
