using DragonLib.CLI;

namespace Cethleann.Unbundler
{
    public class UnbundlerFlags : ICLIFlags
    {
        [CLIFlag("recursive", Aliases = new[] { "R" }, Help = "Recursively parse and unbundle files", Category = "Unbundler Options")]
        public bool Recursive { get; set; }

        [CLIFlag("write-zero", Aliases = new[] { "z" }, Help = "Write Empty files", Category = "Unbundler Options")]
        public bool WriteZero { get; set; }

        [CLIFlag("overwrite", Aliases = new[] { "y" }, Help = "Overwrite files", Category = "Unbundler Options")]
        public bool Overwrite { get; set; }

        [CLIFlag("wbh-alt", Help = "Alternate names for WBH streams", Category = "Unbundler Options")]
        public bool WBHAlternateNames { get; set; }

        [CLIFlag("platform", Default = Platform.Switch, Aliases = new[] { "p" }, Help = "Target Platform", Category = "Unbundler Options")]
        public Platform Platform { get; set; }
    }
}
