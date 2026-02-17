using System.Text.Json.Serialization;

namespace UIFramework.Data;

public partial class Glyph
    {
        [JsonPropertyName("unicode")]
        public long Unicode { get; set; }

        [JsonPropertyName("advance")]
        public double Advance { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("planeBounds")]
        public Bounds PlaneBounds { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("atlasBounds")]
        public Bounds AtlasBounds { get; set; }
    }
