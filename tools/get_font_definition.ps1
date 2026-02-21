Add-Type -AssemblyName PresentationCore
$ttfPath = [System.IO.Path]::GetFullPath('./src/Veldrilonia/Assets/Fonts/FiraCode-Bold.ttf')
if (!(Test-Path $ttfPath)) {
    Write-Error "path not exits: $ttfPath"
    exit 1
}
$glyphTypeface = New-Object -TypeName Windows.Media.GlyphTypeface -ArgumentList $ttfPath
Write-Host "[$($glyphTypeface.FamilyNames['en-US'])] ($($glyphTypeface.FaceNames['en-US'])) $($glyphTypeface.Style) $($glyphTypeface.Weight) $($glyphTypeface.Stretch)"