using System.Collections.Generic;
using Cethleann.Structure;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Nyotengu.AnimationGraph
{
    [UsedImplicitly]
    public class AnimationGraphFlags : ICLIFlags
    {
        [CLIFlag("obj-db-path", Positional = 0, IsRequired = true, Help = "Database file to load", Category = "Animation Graph Options")]
        public string OBJDBPath { get; set; } = string.Empty;

        [CLIFlag("singleton-db-path", Positional = 1, IsRequired = true, Help = "Singleton Database file to load", Category = "Animation Graph Options")]
        public string SingletonPath { get; set; } = string.Empty;

        [UsedImplicitly]
        [CLIFlag("hashes", Positional = 2, IsRequired = true, Help = "Model hashes to find animation for", Category = "Animation Graph Options")]
        public HashSet<uint> Hashes { get; set; } = new HashSet<uint>();

        [CLIFlag("objdb-name", Help = "OPBJDB NAME database path", Category = "Animation Graph Options")]
        public string? OBJDBNDBPath { get; set; }

        [CLIFlag("singleton-name", Help = "Singleton NAME database path", Category = "Animation Graph Options")]
        public string? SingletonDBNDBPath { get; set; }

        [CLIFlag("filelist", Help = "File List to load. Unspecified is automatically determined based on GameId", Category = "Animation Graph Options")]
        public string? FileList { get; set; }

        [CLIFlag("output", Aliases = new[] { "out", "o" }, Help = "Output Directory to copy files", Category = "Animation Graph Options")]
        public string? Output { get; set; }

        [UsedImplicitly]
        [CanBeNull]
        [CLIFlag("dirs", Aliases = new[] { "dir", "d" }, Help = "Directory to find animations from", Category = "Animation Graph Options")]
        public HashSet<string?> AnimationDirectories { get; set; } = new HashSet<string?>();

        [CLIFlag("game", Default = DataGame.None, Aliases = new[] { "g" }, Help = "Game being loaded", Category = "Animation Graph Options")]
        public DataGame GameId { get; set; }
    }
}
