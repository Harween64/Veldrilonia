using System.Text.Json.Serialization;

namespace UIFramework.Data;

public partial class Metrics
    {
        [JsonPropertyName("emSize")]
        public float EmSize { get; set; }

        [JsonPropertyName("lineHeight")]
        public float LineHeight { get; set; }

        [JsonPropertyName("ascender")]
        public float Ascender { get; set; }

        [JsonPropertyName("descender")]
        public float Descender { get; set; }

        [JsonPropertyName("underlineY")]
        public float UnderlineY { get; set; }

        [JsonPropertyName("underlineThickness")]
        public float UnderlineThickness { get; set; }
    }