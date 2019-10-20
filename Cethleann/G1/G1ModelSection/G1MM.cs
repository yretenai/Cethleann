using Cethleann.Structure.Art;

namespace Cethleann.G1.G1ModelSection
{
    /// <summary>
    /// Material Section of G1M models
    /// </summary>
    public class G1MM : IG1Section
    {

        /// <inheritdoc/>
        public int SupportedVersion { get; } = 20;

        /// <inheritdoc/>
        public ResourceSectionHeader Section { get; }
    }
}
