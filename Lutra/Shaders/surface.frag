#version 450

layout(location = 0) in vec2 fsin_TextureCoords;

layout(location = 0) out vec4 fsout_Color;

layout(set = 0, binding = 0) uniform texture2D Surface;
layout(set = 0, binding = 1) uniform sampler Sampler;

void main()
{
    fsout_Color = texture(sampler2D(Surface, Sampler), fsin_TextureCoords);
}
