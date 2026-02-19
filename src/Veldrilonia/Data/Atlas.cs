using System.Text.Json.Serialization;

namespace UIFramework.Data;

public partial class Atlas
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("distanceRange")]
        public long DistanceRange { get; set; }

        [JsonPropertyName("distanceRangeMiddle")]
        public long DistanceRangeMiddle { get; set; }

        [JsonPropertyName("size")]
        public float Size { get; set; }

        [JsonPropertyName("width")]
        public long Width { get; set; }

        [JsonPropertyName("height")]
        public long Height { get; set; }

        [JsonPropertyName("yOrigin")]
        public string? YOrigin { get; set; }
    }
