using System.Text.Json.Serialization;

namespace UIFramework.Data;

public partial class Bounds
    {
        [JsonPropertyName("left")]
        public double Left { get; set; }

        [JsonPropertyName("bottom")]
        public double Bottom { get; set; }

        [JsonPropertyName("right")]
        public double Right { get; set; }

        [JsonPropertyName("top")]
        public double Top { get; set; }
    }
