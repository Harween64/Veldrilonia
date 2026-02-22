using System.Text.Json.Serialization;

namespace Veldridonia.Core.Fonts.MSDF;

public partial class Bounds
    {
        [JsonPropertyName("left")]
        public float Left { get; set; }

        [JsonPropertyName("bottom")]
        public float Bottom { get; set; }

        [JsonPropertyName("right")]
        public float Right { get; set; }

        [JsonPropertyName("top")]
        public float Top { get; set; }

        [JsonIgnore]
        public float Width => Right - Left;

        [JsonIgnore]
        public float Height => Top - Bottom;
    }
