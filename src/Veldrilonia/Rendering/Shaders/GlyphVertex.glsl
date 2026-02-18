#version 450

// --- Données du Vertex (Le Quad qui va de 0,0 à 1,1) ---
layout(location = 0) in vec2 Position;

// --- Données de l'Instance (UIGlyphData) ---
layout(location = 1) in vec2 GlyphPos;
layout(location = 2) in vec2 GlyphSize;
layout(location = 3) in vec4 GlyphUvBounds; // x=uMin, y=vMin, z=uMax, w=vMax
layout(location = 4) in vec4 GlyphColor;

// --- Uniforms ---
layout(set = 0, binding = 0) uniform ProjectionBuffer {
    mat4 Projection;
};

// --- Sorties vers le Fragment Shader ---
layout(location = 0) out vec2 f_uv;
layout(location = 1) out vec4 f_color;

void main() {
    // 1. Calcul de la position à l'écran (identique aux rectangles)
    vec2 worldPos = (Position * GlyphSize) + GlyphPos;
    gl_Position = Projection * vec4(worldPos, 0.0, 1.0);

    // 2. On transmet la couleur
    f_color = GlyphColor;

    // 3. Calcul des UVs
    // TODO: Assigner la bonne valeur à f_uv
    //f_uv = mix(GlyphUvBounds.xy, GlyphUvBounds.zw, Position);
    f_uv.x = mix(GlyphUvBounds.x, GlyphUvBounds.z, Position.x);
    f_uv.y = mix(GlyphUvBounds.y, GlyphUvBounds.w, Position.y);
}