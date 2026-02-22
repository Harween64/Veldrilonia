using System.Numerics;
using System.Text.Json;
using Veldridonia.Core.Fonts.MSDF;
using Veldridonia.Rendering.Features;
using Veldrid;

namespace Veldridonia.Core.Fonts;

public class FontsContext
{
    private readonly GraphicsDevice _device;

    private readonly Dictionary<string, FontMetrics> _fontMetrics = [];

    private readonly Dictionary<string, Texture> _fontAtlas = [];

    public IEnumerable<string> LoadedFonts => _fontAtlas.Keys;

    public FontsContext(GraphicsDevice device)
    {
        _device = device;
    }

    public void LoadFont(string name)
    {
        if (!_fontMetrics.ContainsKey(name))
        {
            var metricsJson = File.ReadAllText($"Assets/Fonts/{name}.json");
            var metrics = JsonSerializer.Deserialize<FontMetrics>(metricsJson)!;
            _fontMetrics[name] = metrics;
        }

        if (!_fontAtlas.ContainsKey(name))
        {
            var atlasImage = new Veldrid.ImageSharp.ImageSharpTexture($"Assets/Fonts/{name}.png", mipmap: true, srgb: false);
            _fontAtlas[name] = atlasImage.CreateDeviceTexture(_device, _device.ResourceFactory);
        }
    }

    public Texture GetFontTexture(string name)
    {
        if (_fontAtlas.TryGetValue(name, out var atlas))
        {
            return atlas;
        }

        throw new Exception($"Font atlas not loaded for '{name}'");
    }

    public GlyphData[] CreateTextInstances(string fontName, string variantName, string text, Vector2 startPosition, float fontSize)
    {
        if (!_fontMetrics.TryGetValue(fontName, out var metrics))
            throw new Exception($"Font metrics not loaded for '{fontName}'");

        var instances = new List<GlyphData>();
        var cursor = startPosition;

        foreach (var character in text)
        {
            var glyph = metrics.GetGlyph(character, variantName);
            if (glyph is null)
            {
                continue; // Skip missing characters
            }

            if (metrics.Atlas is null)
            {
                continue; // Skip if atlas is not loaded
            }

            float atlasWidth = metrics.Atlas.Width;
            float atlasHeight = metrics.Atlas.Height;

            if (glyph.PlaneBounds is not null && glyph.AtlasBounds is not null)
            {
                var uMin = glyph.AtlasBounds.Left / atlasWidth;
                var uMax = glyph.AtlasBounds.Right / atlasWidth;
                // On soustrait les coordonnées Y de la hauteur totale de l'atlas pour les inverser !
                // Note: Le 'Top' géométrique devient le 'vMin' (le haut de la texture)
                var vMin = (atlasHeight - glyph.AtlasBounds.Top) / atlasHeight;
                var vMax = (atlasHeight - glyph.AtlasBounds.Bottom) / atlasHeight;
                instances.Add(new GlyphData(
                    position: cursor + new Vector2(glyph.PlaneBounds.Left, -glyph.PlaneBounds.Top) * fontSize,
                    size: new Vector2(glyph.PlaneBounds.Width, glyph.PlaneBounds.Height) * fontSize,
                    uvBounds: new Vector4(uMin, vMin, uMax, vMax),
                    color: RgbaFloat.Black
                ));
            }

            cursor += new Vector2(glyph.Advance, 0) * fontSize;
        }

        return instances.ToArray();
    }
}