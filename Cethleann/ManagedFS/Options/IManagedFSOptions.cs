using Cethleann.Structure;
using DragonLib.CLI;

namespace Cethleann.ManagedFS.Options
{
    /// <summary>
    ///     Generic ManagedFS Options
    /// </summary>
    public interface IManagedFSOptions : IManagedFSOptionsBase
    {
        /// <summary>
        ///     Platform the game is from
        /// </summary>
        [CLIFlag("platform", Default = DataPlatform.Windows, Aliases = new[] { "P" }, Help = "Platform the game is from", Category = "Unbundler Options")]
        public DataPlatform Platform { get; set; }

        /// <summary>
        ///     Game being loaded
        /// </summary>
        [CLIFlag("game", Default = DataGame.None, Aliases = new[] { "g" }, Help = "Game being loaded", Category = "Unbundler Options")]
        public DataGame GameId { get; set; }
    }
}
