using System.Text.Json.Serialization;

namespace UIFramework.Data;

public partial class FontMetrics
{
    private Dictionary<char, Glyph> glyphs;

    [JsonPropertyName("atlas")]
    public Atlas Atlas { get; set; }

    [JsonPropertyName("metrics")]
    public Metrics Metrics { get; set; }

    [JsonPropertyName("glyphs")]
    public List<Glyph> Glyphs { get => glyphs.Values.ToList(); set => glyphs = value.ToDictionary(g => (char)g.Unicode); }

    [JsonPropertyName("kerning")]
    public List<Kerning> Kerning { get; set; }

    public Glyph? GetGlyph(char unicode)
    {
        if (glyphs.TryGetValue(unicode, out var glyph))
        {
            return glyph;
        }
        
        return null;
    }
}
