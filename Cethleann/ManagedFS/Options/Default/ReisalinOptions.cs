namespace Cethleann.ManagedFS.Options.Default
{
    /// <summary>
    ///     Reisalin specific options implementation
    /// </summary>
    public class ReisalinOptions : IReisalinOptions
    {
        /// <inheritdoc />
        public bool PAKA17 { get; set; }
        /// <inheritdoc />
        public bool PAKKeyFix { get; set; }
    }
}
