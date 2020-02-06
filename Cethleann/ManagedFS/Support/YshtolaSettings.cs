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
        ///     Directory the files and ID table reside in
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        ///     ID Table name
        /// </summary>
        public string TableName { get; set; }

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
