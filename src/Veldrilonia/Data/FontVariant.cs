using System.Text.Json.Serialization;

namespace UIFramework.Data;

public class FontVariant
{
    private Dictionary<char, Glyph> glyphs = new();

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("metrics")]
    public Metrics? Metrics { get; set; }

    [JsonPropertyName("glyphs")]
    public List<Glyph> Glyphs { get => glyphs.Values.ToList(); set => glyphs = value.ToDictionary(g => (char)g.Unicode); }

    [JsonPropertyName("kerning")]
    public List<Kerning>? Kerning { get; set; }

    public Glyph? GetGlyph(char unicode)
    {
        if (glyphs.TryGetValue(unicode, out var glyph))
        {
            return glyph;
        }

        return null;
    }
}
