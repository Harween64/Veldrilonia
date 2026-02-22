param(
    [Parameter(Mandatory = $true)]
    [string]$FontPath
)

Add-Type -AssemblyName PresentationCore
$ttfPath = [System.IO.Path]::GetFullPath($FontPath)
if (!(Test-Path $ttfPath)) {
    Write-Error "path not exists: $ttfPath"
    exit 1
}
$glyphTypeface = New-Object -TypeName Windows.Media.GlyphTypeface -ArgumentList $ttfPath
Write-Host "[$($glyphTypeface.FamilyNames['en-US'])] ($($glyphTypeface.FaceNames['en-US'])) $($glyphTypeface.Style) $($glyphTypeface.Weight) $($glyphTypeface.Stretch)"