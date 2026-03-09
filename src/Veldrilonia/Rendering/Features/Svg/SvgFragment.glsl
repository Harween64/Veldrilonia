#version 450

// Entree interpolee depuis le vertex shader
layout(location = 0) in vec4 fColor;

// Sortie
layout(location = 0) out vec4 outColor;

void main() {
    // Couleur interpolee (gere les degradees via l'interpolation GPU des vertex colors)
    if (fColor.a <= 0.0) {
        discard;
    }
    outColor = fColor;
}
