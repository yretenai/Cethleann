using System.Collections.Generic;
using Cethleann.Structure;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace Cethleann.DataProcessor
{
    public class DataExporterFlags : ICLIFlags
    {
        [CLIFlag("struct", Positional = 0, Help = "Struct Name", Category = "DataProcessor Options")]
        public string StructName { get; set; } = string.Empty;

        [UsedImplicitly]
        [CLIFlag("paths", Positional = 1, Help = "Struct files", Category = "DataProcessor Options")]
        public List<string> Paths { get; set; } = new List<string>();

        [CLIFlag("platform", Default = DataPlatform.Unspecified, Aliases = new[] { "P" }, Help = "Platform the game is from", Category = "DataProcessor Options")]
        public DataPlatform Platform { get; set; }

        [CLIFlag("game", Default = DataGame.None, ValidValues = new[] { "DissidiaNT", "DissidiaOO", "ThreeHouses" }, IsRequired = true, Aliases = new[] { "g" }, Help = "Game being loaded", Category = "DataProcessor Options")]
        public DataGame GameId { get; set; }
    }
}
