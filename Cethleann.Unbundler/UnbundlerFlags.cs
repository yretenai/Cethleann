using Cethleann.Structure;
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

        [CLIFlag("keep", Aliases = new[] { "k" }, Help = "Keep duplicate files", Category = "Unbundler Options")]
        public bool KeepBoth { get; set; }

        [CLIFlag("wbh-alt", Help = "Alternate names for WBH streams", Category = "Unbundler Options")]
        public bool WBHAlternateNames { get; set; }

        [CLIFlag("pcm", Default = true, Aliases = new[] { "p" }, Help = "Convert ADPCM streams to PCM", Category = "Unbundler Options")]
        public bool ConvertADPCM { get; set; }

        [CLIFlag("ktsr-raw", Help = "Output Raw KTSR streams", Category = "Unbundler Options")]
        public bool RawKTSR { get; set; }

        [CLIFlag("platform", Default = DataPlatform.Windows, Aliases = new[] { "P" }, Help = "Platform the game is from", Category = "Unbundler Options")]
        public DataPlatform Platform { get; set; }

        [CLIFlag("game", Default = DataGame.None, Aliases = new[] { "g" }, Help = "Game being loaded", Category = "Unbundler Options")]
        public DataGame GameId { get; set; }
    }
}
