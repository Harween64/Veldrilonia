#version 450

layout(location = 0) in vec2 vPos;
layout(location = 1) in vec2 iPos;
layout(location = 2) in vec2 iSize;
layout(location = 3) in vec4 iColor;
layout(location = 4) in float iRadius;
layout(location = 5) in float iThickness;
layout(location = 6) in vec4 iBorderColor;
layout(location = 7) in float iDepth;

layout(set = 0, binding = 0) uniform ProjectionBuffer {
    mat4 Projection;
};

layout(location = 0) out vec4 fColor;
layout(location = 1) out vec2 fUV;
layout(location = 2) out vec2 fSize;
layout(location = 3) out float fRadius;
layout(location = 4) out float fThickness;
layout(location = 5) out vec4 fBorderColor;

void main() {
    vec2 worldPos = iPos + (vPos * iSize);
    gl_Position = Projection * vec4(worldPos, 0.0, 1.0);
    gl_Position.z = iDepth;

    fColor = iColor;
    fUV = vPos;
    fSize = iSize;
    fRadius = iRadius;
    fThickness = iThickness;
    fBorderColor = iBorderColor;
}
