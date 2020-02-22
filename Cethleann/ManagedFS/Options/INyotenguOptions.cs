using DragonLib.CLI;

namespace Cethleann.ManagedFS.Options
{
    public interface INyotenguOptions : IManagedFSOptionsBase
    {
        [CLIFlag("rdb-prefix-id", Help = "Debug Filenames", Hidden = true, Category = "Nyotengu Options")]
        public bool PrefixFilenames { get; set; }
    }
}
