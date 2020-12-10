using Cethleann.Structure;
using DragonLib.CLI;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace Cethleann.Identify
{
    public class IdentifyFlags : ICLIFlags
    {
        [UsedImplicitly]
        [CLIFlag("paths", Positional = 0, Help = "Files to identify", Category = "Identify Options")]
        public List<string> Paths { get; set; } = new List<string>();

        [CLIFlag("type", Default = DataType.None, IsRequired = false, Help = "Force file type to this type", Category = "Identify Options")]
        public DataType ForceType { get; set; } = DataType.None;
    }
}
