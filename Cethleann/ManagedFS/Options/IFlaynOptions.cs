using DragonLib.CLI;

namespace Cethleann.ManagedFS.Options
{
    /// <summary>
    ///     Nyotengu specific options
    /// </summary>
    public interface IFlaynOptions : IManagedFSOptionsBase
    {
        /// <summary>
        ///     Alternate format
        /// </summary>
        [CLIFlag("flayn-tiny", Help = "Tiny Index Format (HWL)", Category = "Flayn Options")]
        public bool TinyFlayn { get; set; }
    }
}
