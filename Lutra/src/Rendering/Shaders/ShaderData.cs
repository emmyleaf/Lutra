using System.Collections.Generic;
using System.Numerics;
using Lutra.Utility;
using Veldrid;

namespace Lutra.Rendering.Shaders;

public class ShaderData
{
    // TODO:
    // * Check for blittable type
    // * Use Marshal.SizeOf to get value's size
    // * Round to nearest 16
    // * Create buffer and blit using unsafe code
    internal string ShaderName;
    internal ResourceLayoutElementDescription[] Elements;
    internal IBindableResource[] Resources;
    internal Dictionary<string, DeviceBuffer> BufferDictionary;

    public ShaderData(string shaderFilename, params (string Name, object Value)[] parameters)
    {
        ShaderName = System.IO.Path.GetFileName(shaderFilename);
        var shaderBytes = AssetManager.LoadBytes(shaderFilename);

        var count = parameters.Length;

        Elements = new ResourceLayoutElementDescription[count];
        Resources = new IBindableResource[count];
        BufferDictionary = [];

        for (int i = 0; i < count; i++)
        {
            var (Name, Value) = parameters[i];
            var value = Value;

            if (value is LutraTexture texture)
            {
                Elements[i] = new ResourceLayoutElementDescription(Name, ResourceKind.TextureReadOnly, ShaderStages.Fragment);
                Resources[i] = texture.TextureView;
            }
            else if (value is Color color)
            {
                CreateSmallUniformBuffer(i, Name, color);
            }
            else if (value is Vector4 vector4)
            {
                CreateSmallUniformBuffer(i, Name, vector4);
            }
            else if (value is Vector2 vector2)
            {
                CreateSmallUniformBuffer(i, Name, vector2);
            }
            else if (value is float @float)
            {
                CreateSmallUniformBuffer(i, Name, @float);
            }
            else if (value is int @int)
            {
                CreateSmallUniformBuffer(i, Name, @int);
            }
            else if (value is IBindableResource)
            {
                throw new NotImplementedException($"Parameter {Name} type is not implemented for SpriteShader");
            }
            else
            {
                throw new ArgumentException($"Parameter {Name} does not map to a Veldrid.BindableResource");
            }
        }

        var layoutDesc = new ResourceLayoutDescription(Elements);

        Draw.SpriteRenderPipeline.CreateShaderPipeline(ShaderName, shaderBytes, layoutDesc);
    }

    private void CreateSmallUniformBuffer<T>(int index, string name, T value) where T : unmanaged
    {
        Elements[index] = new ResourceLayoutElementDescription($"{name}Buffer", ResourceKind.UniformBuffer, ShaderStages.Fragment);
        var uniformBuffer = VeldridResources.Factory.CreateBuffer(new BufferDescription(32u, BufferUsage.UniformBuffer));
        VeldridResources.GraphicsDevice.UpdateBuffer(uniformBuffer, 0, value);
        Resources[index] = uniformBuffer;
        BufferDictionary.Add(name, uniformBuffer);
    }

    public void UpdateBuffer<T>(string name, T value) where T : unmanaged
    {
        if (BufferDictionary.TryGetValue(name, out var uniformBuffer))
        {
            VeldridResources.GraphicsDevice.UpdateBuffer(uniformBuffer, 0, value);
        }
        else
        {
            Util.LogError($"Shader '{ShaderName}' parameter '{name}' is not a buffer");
        }
    }
}
