using DragonLib.CLI;

namespace Cethleann.ManagedFS.Options
{
    /// <summary>
    ///     Nyotengu specific options
    /// </summary>
    public interface INyotenguOptions : IManagedFSOptionsBase
    {
        /// <summary>
        ///     Debug Filenames
        /// </summary>
        [CLIFlag("nyotengu-prefix-id", Help = "Debug Filenames", Hidden = true, Category = "Nyotengu Options")]
        public bool NyotenguPrefixFilenames { get; set; }
    }
}
