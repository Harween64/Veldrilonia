using Svg;

namespace Veldridonia.Core.Svg;

/// <summary>
/// Charge et tesselle des fichiers SVG en maillages prets pour le GPU.
/// </summary>
public static class SvgLoader
{
    /// <summary>
    /// Charge un fichier SVG et retourne le maillage tesselle.
    /// </summary>
    public static SvgMeshData LoadFromFile(string filePath)
    {
        var document = SvgDocument.Open(filePath);
        return SvgTessellator.Tessellate(document);
    }

    /// <summary>
    /// Charge un SVG depuis une chaine XML et retourne le maillage tesselle.
    /// </summary>
    public static SvgMeshData LoadFromString(string svgContent)
    {
        var document = SvgDocument.FromSvg<SvgDocument>(svgContent);
        return SvgTessellator.Tessellate(document);
    }
}
