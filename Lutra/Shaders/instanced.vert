#version 450

layout(location = 0) in vec2 Position;

layout(location = 0) out vec4 fsin_Color;
layout(location = 1) out vec2 fsin_TextureCoords;

struct Instance 
{
    vec4 Color;
    mat4 Source;
    mat4 World;
};

layout(set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
};
layout(set = 0, binding = 1) uniform ViewBuffer
{
    mat4 View;
};
layout(set = 1, binding = 0) readonly buffer InstancesBuffer
{
    Instance Instances[];
};

void main()
{
    Instance instance = Instances[gl_InstanceIndex];
    vec4 pos = vec4(Position, 0, 1);

    gl_Position = Projection * View * instance.World * pos;

    fsin_Color = instance.Color;
    fsin_TextureCoords = (instance.Source * pos).xy;
}
