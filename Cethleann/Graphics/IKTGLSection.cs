using Cethleann.Structure.Resource;
using JetBrains.Annotations;

namespace Cethleann.Graphics
{
    /// <summary>
    ///     Boiler interface for G1 Sections.
    /// </summary>
    [PublicAPI]
    public interface IKTGLSection
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
