#version 450

// --- Entrées (depuis le Vertex Shader) ---
layout(location = 0) in vec2 f_uv;
layout(location = 1) in vec4 f_color;

// --- Uniforms (Texture et Sampler) ---
layout(set = 0, binding = 1) uniform texture2D FontTexture;
layout(set = 0, binding = 2) uniform sampler FontSampler;

// --- Sortie (La couleur du pixel à l'écran) ---
layout(location = 0) out vec4 outColor;

void main() {
    // 1. Lire les données MSDF depuis la texture
    vec4 msdfData = texture(sampler2D(FontTexture, FontSampler), f_uv);
    float median = max(min(msdfData.r, msdfData.g), min(max(msdfData.r, msdfData.g), msdfData.b)); // Calcul du médian des 3 canaux
    
    //float alpha = step(0.5, median); // Seuil à 0.5 pour une lecture binaire (on peut aussi faire du lissage)
    
    //float pixelVariation = fwidth(median); // Calcul de la variation du pixel pour l'anti-aliasing
    //float alpha = smoothstep(0.5 - pixelVariation, 0.5 + pixelVariation, median); // Lissage autour du seuil de 0.5

    float pxRange = 8.0; // La plage de pixels que le MSDF représente (doit correspondre à ce qui a été utilisé pour générer la texture)
    vec2 msdfUnit = pxRange / vec2(textureSize(sampler2D(FontTexture, FontSampler), 0));
    vec2 screenTexSize = vec2(1.0) / fwidth(f_uv);
    float screenPxRange = max(0.5 * dot(msdfUnit, screenTexSize), 1.0);
    float screenPxDistance = screenPxRange * (median - 0.5);
    float alpha = clamp(screenPxDistance + 0.5, 0.0, 1.0);

    outColor = vec4(f_color.rgb, f_color.a * alpha); // Appliquer la couleur de l'instance avec l'alpha du MSDF
}