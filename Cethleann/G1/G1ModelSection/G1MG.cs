using Cethleann.Structure.Art;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    /// Geometry Section of G1M models
    /// </summary>
    public class G1MG : IG1Section
    {

        /// <inheritdoc/>
        public int SupportedVersion { get; } = 44;

        /// <inheritdoc/>
        public ResourceSectionHeader Section { get; }
    }
}
