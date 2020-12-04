using Cethleann.ManagedFS.Options;
using Cethleann.Structure;
using DragonLib.CLI;

namespace Cethleann.Unbundler
{
    public class UnbundlerFlags : ICLIFlags, IManagedFSOptions
    {
        [CLIFlag("recursive", Aliases = new[] { "R" }, Help = "Recursively unbundle files", Category = "Unbundler Options")]
        public bool Recursive { get; set; }
        
        [CLIFlag("deep-recursive", Aliases = new[] { "r" }, Help = "Unbundle files from unbundled files", Category = "Unbundler Options")]
        public bool DeepRecursive { get; set; }

        [CLIFlag("depth", Aliases = new[] { "D" }, Default = uint.MaxValue, Help = "Recursion Depth", Category = "Unbundler Options")]
        public uint Depth { get; set; }

        [CLIFlag("write-zero", Aliases = new[] { "z" }, Help = "Write Empty files", Category = "Unbundler Options")]
        public bool WriteZero { get; set; }

        [CLIFlag("overwrite", Aliases = new[] { "y" }, Help = "Overwrite files", Category = "Unbundler Options")]
        public bool Overwrite { get; set; }

        [CLIFlag("keep", Aliases = new[] { "k" }, Help = "Keep duplicate files", Category = "Unbundler Options")]
        public bool KeepBoth { get; set; }

        [CLIFlag("wbh-alt", Help = "Alternate names for WBH streams", Category = "Unbundler Options")]
        public bool WBHAlternateNames { get; set; }

        [CLIFlag("no-pcm", Aliases = new[] { "p" }, Help = "Don't convert ADPCM streams to PCM", Category = "Unbundler Options")]
        public bool SkipADPCM { get; set; }

        [CLIFlag("ktsr-raw", Help = "Output Raw KTSR streams", Category = "Unbundler Options")]
        public bool RawKTSR { get; set; }

        #region IManagedFSOptions

        public DataPlatform Platform { get; set; }
        public string GameId { get; set; } = "";

        #endregion
    }
}
