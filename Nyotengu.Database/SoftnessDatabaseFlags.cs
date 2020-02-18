using System.Collections.Generic;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Nyotengu.Database
{
    public class SoftnessDatabaseFlags : ICLIFlags
    {
        [UsedImplicitly]
        [CLIFlag("paths", Positional = 0)]
        public HashSet<string> Paths { get; set; } = new HashSet<string>();
    }
}
