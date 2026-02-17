#version 450

// Entrées
layout(location = 0) in vec4 fColor;
layout(location = 1) in vec2 fUV;
layout(location = 2) in vec2 fSize;
layout(location = 3) in float fRadius;
layout(location = 4) in float fThickness;
layout(location = 5) in vec4 fBorderColor;

// Sortie
layout(location = 0) out vec4 outColor;

void main() 
{
    // 1. Coordonnées centrées en pixels (0,0 est au centre du bouton)
    vec2 pixelPos = fUV * fSize;
    vec2 p = pixelPos - (fSize / 2.0);

    // 2. Calcul SDF (Signed Distance Field) pour une Boîte Arrondie
    vec2 q = abs(p) - (fSize / 2.0 - vec2(fRadius));
    float dist = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - fRadius;

    // 3. Masque d'opacité (Anti-aliasing)
    float alphaMask = 1.0 - smoothstep(-0.5, 0.5, dist);
    if (alphaMask <= 0.0) 
    {
        discard;
    }

    if (fThickness > 0.0) 
    {
        // Distance to the inner edge (inset by border thickness)
        float innerDist = dist + fThickness;

        // fillMask: 1 inside fill area, 0 in border or outside (with AA)
        float fillMask = 1.0 - smoothstep(-0.5, 0.5, innerDist);

        // Pick fill color inside, border color in the border strip
        vec4 blendedColor = mix(fBorderColor, fColor, fillMask);

        // Clip everything to the outer shape boundary
        outColor = vec4(blendedColor.rgb, blendedColor.a * alphaMask);
    } 
    else 
    {
        // Couleur finale
        outColor = vec4(fColor.rgb, fColor.a * alphaMask);
    }
}
