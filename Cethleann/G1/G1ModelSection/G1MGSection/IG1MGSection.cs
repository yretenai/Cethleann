using Cethleann.Structure.Resource.Model;

namespace Cethleann.G1.G1ModelSection.G1MGSection
{
    /// <summary>
    ///     Base Wrapper for G1MG Sections
    /// </summary>
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
