using DragonLib.CLI;

namespace Cethleann.ManagedFS.Options
{
    public interface IReisalinOptions : IManagedFSOptionsBase
    {
        [CLIFlag("32bit", Aliases = new[] { "a17" }, Help = "Parse older, 32-bit Atelier PAKs", Category = "Reisalin Options")]
        public bool A17 { get; set; }
    }
}
