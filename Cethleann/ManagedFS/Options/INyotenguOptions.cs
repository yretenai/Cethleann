using DragonLib.CLI;

namespace Cethleann.ManagedFS.Options
{
    public interface INyotenguOptions : IManagedFSOptionsBase
    {
        [CLIFlag("nyotengu-prefix-id", Help = "Debug Filenames", Hidden = true, Category = "Nyotengu Options")]
        public bool NyotenguPrefixFilenames { get; set; }
    }
}
