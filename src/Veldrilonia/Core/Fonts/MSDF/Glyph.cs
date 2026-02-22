using System.Text.Json.Serialization;

namespace Veldridonia.Core.Fonts.MSDF;

public partial class Glyph
    {
        [JsonPropertyName("unicode")]
        public long Unicode { get; set; }

        [JsonPropertyName("advance")]
        public float Advance { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("planeBounds")]
        public Bounds? PlaneBounds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("atlasBounds")]
        public Bounds? AtlasBounds { get; set; }
    }
