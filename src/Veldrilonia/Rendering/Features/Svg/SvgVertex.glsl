#version 450

// Vertex attributes
layout(location = 0) in vec2 vPos;
layout(location = 1) in vec4 vColor;

// Uniform
layout(set = 0, binding = 0) uniform ProjectionBuffer {
    mat4 Projection;
};

// Sorties vers le fragment shader
layout(location = 0) out vec4 fColor;

void main() {
    gl_Position = Projection * vec4(vPos, 0.0, 1.0);
    fColor = vColor;
}
