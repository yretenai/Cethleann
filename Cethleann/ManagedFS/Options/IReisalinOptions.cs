using DragonLib.CLI;

namespace Cethleann.ManagedFS.Options
{
    public interface IReisalinOptions : IManagedFSOptionsBase
    {
        [CLIFlag("reisalin-32bit", Help = "Parse older, 32-bit Atelier PAKs", Category = "Reisalin Options")]
        public bool ReisalinA17 { get; set; }
    }
}
