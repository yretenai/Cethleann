using DragonLib.CLI;

namespace Cethleann.ManagedFS.Options
{
    /// <summary>
    ///     Reisalin specific options
    /// </summary>
    public interface IReisalinOptions : IManagedFSOptionsBase
    {
        /// <summary>
        ///     Parse older, 32-bit Atelier PAKs
        /// </summary>
        [CLIFlag("reisalin-32bit", Help = "Parse older, 32-bit Atelier PAKs", Category = "Reisalin Options")]
        public bool ReisalinA17 { get; set; }
    }
}
