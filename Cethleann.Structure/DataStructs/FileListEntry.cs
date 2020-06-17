using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Cethleann.Structure.DataStructs
{
    [PublicAPI]
    public struct FileListEntry
    {
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("file_name")]
        public string FileName { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("cacheclear_flag")]
        public int CacheClear { get; set; }

        [JsonPropertyName("bundle_flag")]
        public int Bundle { get; set; }

        [JsonPropertyName("directory")]
        public string Directory { get; set; }

        [JsonPropertyName("firstflag")]
        public int First { get; set; }

        [JsonPropertyName("size")]
        public ulong Size { get; set; }
    }
}
