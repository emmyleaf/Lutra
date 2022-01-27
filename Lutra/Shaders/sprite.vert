#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;
layout(location = 2) in vec2 TextureCoords;

layout(location = 0) out vec4 fsin_Color;
layout(location = 1) out vec2 fsin_TextureCoords;

layout(set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
};
layout(set = 0, binding = 1) uniform ViewBuffer
{
    mat4 View;
};
layout(set = 1, binding = 0) uniform WorldBuffer
{
    mat4 World;
};

void main()
{
    vec4 worldPosition = World * vec4(Position, 0, 1);
    vec4 viewPosition = View * worldPosition;
    gl_Position = Projection * viewPosition;
    fsin_Color = Color;
    fsin_TextureCoords = TextureCoords;
}
