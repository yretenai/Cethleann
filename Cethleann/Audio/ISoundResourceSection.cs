using Cethleann.Structure.Resource.Audio;
using JetBrains.Annotations;

namespace Cethleann.Audio
{
    /// <summary>
    ///     Section helper
    /// </summary>
    [PublicAPI]
    public interface ISoundResourceSection
    {
        /// <summary>
        ///     Underlying entry
        /// </summary>
        SoundResourceEntry Base { get; }
    }
}
