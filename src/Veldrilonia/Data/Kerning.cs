using System.Text.Json.Serialization;

namespace UIFramework.Data;

public partial class Kerning
    {
        [JsonPropertyName("unicode1")]
        public long Unicode1 { get; set; }

        [JsonPropertyName("unicode2")]
        public long Unicode2 { get; set; }

        [JsonPropertyName("advance")]
        public double Advance { get; set; }
    }
