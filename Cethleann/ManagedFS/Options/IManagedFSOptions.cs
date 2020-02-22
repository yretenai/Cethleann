using Cethleann.Structure;
using DragonLib.CLI;

namespace Cethleann.ManagedFS.Options
{
    public interface IManagedFSOptions : IManagedFSOptionsBase
    {
        [CLIFlag("platform", Default = DataPlatform.Windows, Aliases = new[] { "P" }, Help = "Platform the game is from", Category = "Unbundler Options")]
        public DataPlatform Platform { get; set; }

        [CLIFlag("game", Default = DataGame.None, Aliases = new[] { "g" }, Help = "Game being loaded", Category = "Unbundler Options")]
        public DataGame GameId { get; set; }
    }
}
