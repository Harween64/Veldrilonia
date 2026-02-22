param(
    [string]$fontsDir = "Assets\Fonts",
    [string]$toolsDir = "..\..\tools"
)

Add-Type -AssemblyName PresentationCore

$ttfFiles = Get-ChildItem -Path $fontsDir -Filter "*.ttf"

$families = @{}

foreach ($file in $ttfFiles) {
    $ttfPath = $file.FullName
    $glyphTypeface = New-Object -TypeName Windows.Media.GlyphTypeface -ArgumentList $ttfPath
    
    $familyName = $glyphTypeface.FamilyNames['en-US']
    if ([string]::IsNullOrEmpty($familyName)) {
        if ($glyphTypeface.FamilyNames.Values.Count -gt 0) {
            $familyName = $glyphTypeface.FamilyNames.Values[0]
        }
        else {
            $familyName = $file.BaseName
        }
    }
    
    $faceName = $glyphTypeface.FaceNames['en-US']
    if ([string]::IsNullOrEmpty($faceName)) {
        if ($glyphTypeface.FaceNames.Values.Count -gt 0) {
            $faceName = $glyphTypeface.FaceNames.Values[0]
        }
        else {
            $faceName = "Regular"
        }
    }
    
    $style = $glyphTypeface.Style
    $weight = $glyphTypeface.Weight
    $stretch = $glyphTypeface.Stretch
    
    $variantName = "$faceName $style $weight $stretch"
    
    if (-not $families.ContainsKey($familyName)) {
        $families[$familyName] = @()
    }
    
    $families[$familyName] += [PSCustomObject]@{
        Path        = $ttfPath
        VariantName = $variantName
    }
}

foreach ($family in $families.Keys) {
    $familyFonts = $families[$family]
    
    $argsList = @()
    foreach ($font in $familyFonts) {
        if ($argsList.Count -gt 0) {
            $argsList += "-and"
        }
        $argsList += "-font"
        $argsList += $font.Path
        $argsList += "-fontname"
        $argsList += $font.VariantName
    }
    
    $outPng = Join-Path $fontsDir "$family.png"
    $outJson = Join-Path $fontsDir "$family.json"
    
    $msdfGen = Join-Path $toolsDir "msdf-atlas-gen.exe"
    
    $fullArgs = $argsList + @("-type", "msdf", "-potr", "-pxrange", "8", "-pxalign", "on", "-imageout", $outPng, "-json", $outJson)
    
    Write-Host "Generating atlas for family: $family"
    
    & $msdfGen @fullArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "msdf-atlas-gen failed for family $family"
        exit $LASTEXITCODE
    }
}
