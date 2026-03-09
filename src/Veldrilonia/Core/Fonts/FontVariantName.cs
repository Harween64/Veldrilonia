namespace Veldridonia.Core.Fonts;

public static class FontVariantName
{
    public static class FontFace
    {
        public const string Regular = "Regular";
        public const string Italic = "Italic";
        public const string Oblique = "Oblique";
    }
    
    public static class FontStyle
    {
        public const string Normal = "Normal";
        public const string Italic = "Italic";
        public const string Oblique = "Oblique";
    }

    public static class FontWeight
    {
        public const string Thin = "Thin";
        public const string ExtraLight = "ExtraLight";
        public const string Light = "Light";
        public const string Regular = "Regular";
        public const string Medium = "Medium";
        public const string SemiBold = "SemiBold";
        public const string Bold = "Bold";
        public const string ExtraBold = "ExtraBold";
        public const string Black = "Black";
    }

    public static class FontStretch
    {
        public const string UltraCondensed = "UltraCondensed";
        public const string ExtraCondensed = "ExtraCondensed";
        public const string Condensed = "Condensed";
        public const string SemiCondensed = "SemiCondensed";
        public const string Normal = "Normal";
        public const string SemiExpanded = "SemiExpanded";
        public const string Expanded = "Expanded";
        public const string ExtraExpanded = "ExtraExpanded";
        public const string UltraExpanded = "UltraExpanded";
    }

    public const string Regular = "Regular Normal Normal Normal";
    public const string Bold = "Bold Normal Bold Normal";
    public const string Medium = "Medium Normal Medium Normal";
    public const string SemiBold = "SemiBold Normal SemiBold Normal";
    public const string Italic = "Italic Italic Normal Normal";
    public const string BoldItalic = "Bold Italic Bold Normal";
    public const string MediumItalic = "Medium Italic Medium Normal";
    public const string SemiBoldItalic = "SemiBold Italic SemiBold Normal";

    public static string Build(string face, string style, string weight, string stretch)
    {
        return $"{face} {style} {weight} {stretch}";
    }
}
