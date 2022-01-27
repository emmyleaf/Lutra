#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 1) in vec2 fsin_TextureCoords;

layout(location = 0) out vec4 fsout_Color;

layout(set = 1, binding = 1) uniform texture2D Sprite;
layout(set = 1, binding = 2) uniform sampler Sampler;

void main()
{
    fsout_Color = fsin_Color * texture(sampler2D(Sprite, Sampler), fsin_TextureCoords);
}
