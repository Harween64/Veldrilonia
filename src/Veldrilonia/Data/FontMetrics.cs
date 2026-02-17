using System.Text.Json.Serialization;

namespace UIFramework.Data;

public partial class FontMetrics
    {
        [JsonPropertyName("atlas")]
        public Atlas Atlas { get; set; }

        [JsonPropertyName("metrics")]
        public Metrics Metrics { get; set; }

        [JsonPropertyName("glyphs")]
        public List<Glyph> Glyphs { get; set; }

        [JsonPropertyName("kerning")]
        public List<Kerning> Kerning { get; set; }
    }
