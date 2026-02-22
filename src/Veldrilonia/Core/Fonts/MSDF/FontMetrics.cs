using System.Text.Json.Serialization;

namespace Veldridonia.Core.Fonts.MSDF;

public partial class FontMetrics
{
    private Dictionary<char, Glyph> glyphs = new();

    [JsonPropertyName("atlas")]
    public Atlas? Atlas { get; set; }

    [JsonPropertyName("metrics")]
    public Metrics? Metrics { get; set; }

    [JsonPropertyName("glyphs")]
    public List<Glyph> Glyphs { get => glyphs.Values.ToList(); set => glyphs = value.ToDictionary(g => (char)g.Unicode); }

    [JsonPropertyName("kerning")]
    public List<Kerning>? Kerning { get; set; }

    [JsonPropertyName("variants")]
    public List<FontVariant>? Variants { get; set; }

    public Glyph? GetGlyph(char unicode, string variantName = FontVariantName.Regular)
    {
        if (Variants != null)
        {
            var variant = Variants.FirstOrDefault(v => string.Equals(v.Name, variantName, StringComparison.OrdinalIgnoreCase));
            if (variant != null)
            {
                return variant.GetGlyph(unicode);
            }
        }

        if (glyphs.TryGetValue(unicode, out var glyph))
        {
            return glyph;
        }

        return null;
    }
}
