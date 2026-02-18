using System.Numerics;
using System.Text.Json;
using UIFramework.Data;
using Veldrid;

namespace UIFramework.Core;

public class FontsContext
{
    private readonly GraphicsDevice _device;

    private readonly Dictionary<string, FontMetrics> _fontMetrics = [];

    private readonly Dictionary<string, Texture> _fontAtlas = [];

    public FontsContext(GraphicsDevice device)
    {
        _device = device;
    } 

    public void LoadFont(string name = "default")
    {
        if (!_fontMetrics.TryGetValue(name, out var cachedMetrics))
        {
            var metricsJson = File.ReadAllText("Assets/font.json");
            var metrics = JsonSerializer.Deserialize<FontMetrics>(metricsJson);
            _fontMetrics[name] = metrics;
        }

        if (!_fontAtlas.TryGetValue(name, out var cachedAtlas))
        {
            var atlasImage = new Veldrid.ImageSharp.ImageSharpTexture("Assets/font.png");
            _fontAtlas[name] = atlasImage.CreateDeviceTexture(_device, _device.ResourceFactory);
        }
    }

    public Texture GetFontTexture(string name = "default")
    {
        if (_fontAtlas.TryGetValue(name, out var atlas))
        {
            return atlas;
        }

        throw new Exception($"Font atlas not loaded for '{name}'");
    }

    public UIGlyphData[] CreateTextInstances(string text, Vector2 startPosition, float fontSize)
    {
        if (!_fontMetrics.TryGetValue("default", out var metrics))
            throw new Exception("Font metrics not loaded for 'default'");

        var instances = new List<UIGlyphData>();
        var cursor = startPosition;

        foreach (var character in text)
        {
            var glyph = metrics.GetGlyph(character);
            if (glyph is null)
            {
                continue; // Skip missing characters
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
                instances.Add(new UIGlyphData(
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