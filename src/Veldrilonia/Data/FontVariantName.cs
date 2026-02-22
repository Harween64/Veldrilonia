namespace UIFramework.Data;

public static class FontVariantName
{
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
