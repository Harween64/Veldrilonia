using System.Text.Json.Serialization;

namespace UIFramework.Data;

public partial class Metrics
    {
        [JsonPropertyName("emSize")]
        public long EmSize { get; set; }

        [JsonPropertyName("lineHeight")]
        public double LineHeight { get; set; }

        [JsonPropertyName("ascender")]
        public double Ascender { get; set; }

        [JsonPropertyName("descender")]
        public double Descender { get; set; }

        [JsonPropertyName("underlineY")]
        public double UnderlineY { get; set; }

        [JsonPropertyName("underlineThickness")]
        public double UnderlineThickness { get; set; }
    }