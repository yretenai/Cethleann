using Cethleann.Koei.Structure.Resource.Model;
using JetBrains.Annotations;

namespace Cethleann.Koei.G1.G1ModelSection.G1MGSection
{
    /// <summary>
    ///     Base Wrapper for G1MG Sections
    /// </summary>
    [PublicAPI]
    public interface IG1MGSection
    {
        /// <summary>
        ///     Type of this section
        /// </summary>
        ModelGeometryType Type { get; }

        /// <summary>
        ///     Section header
        /// </summary>
        ModelGeometrySection Section { get; }
    }
}
