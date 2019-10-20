using Cethleann.Structure.Art;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    /// F Section of G1M models
    /// </summary>
    public class G1MF : IG1Section
    {

        /// <inheritdoc/>
        public int SupportedVersion { get; } = 32;

        /// <inheritdoc/>
        public ResourceSectionHeader Section { get; }
    }
}
