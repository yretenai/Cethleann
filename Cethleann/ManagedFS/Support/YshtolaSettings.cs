using JetBrains.Annotations;

namespace Cethleann.ManagedFS.Support
{
    /// <summary>
    ///     Abstract base settings
    /// </summary>
    [PublicAPI]
    public abstract class YshtolaSettings
    {
        /// <summary>
        ///     ID Table name
        /// </summary>
        public string[] TableNames { get; set; }

        /// <summary>
        ///     Key truth
        /// </summary>
        public byte[] XorTruth { get; set; }

        /// <summary>
        ///     Key multiplier constant
        /// </summary>
        public ulong Multiplier { get; set; }

        /// <summary>
        ///     Key divisor constant
        /// </summary>
        public ulong Divisor { get; set; }
    }
}
