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
    internal BindableResource[] Resources;
    internal Dictionary<string, DeviceBuffer> BufferDictionary;

    public ShaderData(string shaderFilename, params (string Name, object Value)[] parameters)
    {
        ShaderName = System.IO.Path.GetFileName(shaderFilename);
        var shaderBytes = AssetManager.LoadBytes(shaderFilename);

        var count = parameters.Length;

        Elements = new ResourceLayoutElementDescription[count];
        Resources = new BindableResource[count];
        BufferDictionary = new();

        for (int i = 0; i < count; i++)
        {
            var param = parameters[i];
            var value = param.Value;

            if (value is LutraTexture)
            {
                Elements[i] = new ResourceLayoutElementDescription(param.Name, ResourceKind.TextureReadOnly, ShaderStages.Fragment);
                Resources[i] = (value as LutraTexture).TextureView;
            }
            else if (value is Color)
            {
                CreateSmallUniformBuffer(i, param.Name, (Color)value);
            }
            else if (value is Vector2)
            {
                CreateSmallUniformBuffer(i, param.Name, (Vector2)value);
            }
            else if (value is float)
            {
                CreateSmallUniformBuffer(i, param.Name, (float)value);
            }
            else if (value is int)
            {
                CreateSmallUniformBuffer(i, param.Name, (int)value);
            }
            else if (value is BindableResource)
            {
                throw new NotImplementedException($"Parameter {param.Name} type is not implemented for SpriteShader");
            }
            else
            {
                throw new ArgumentException($"Parameter {param.Name} does not map to a Veldrid.BindableResource");
            }
        }

        var layoutDesc = new ResourceLayoutDescription(Elements);

        Draw.SpriteRenderPipeline.CreateShaderPipeline(ShaderName, shaderBytes, layoutDesc);
    }

    private void CreateSmallUniformBuffer<T>(int index, string name, T value) where T : unmanaged
    {
        Elements[index] = new ResourceLayoutElementDescription($"{name}Buffer", ResourceKind.UniformBuffer, ShaderStages.Fragment);
        var uniformBuffer = VeldridResources.Factory.CreateBuffer(new BufferDescription(16u, BufferUsage.UniformBuffer));
        VeldridResources.GraphicsDevice.UpdateBuffer(uniformBuffer, 0, value);
        Resources[index] = uniformBuffer;
        BufferDictionary.Add(name, uniformBuffer);
    }

    public void UpdateBuffer<T>(string name, T value) where T : unmanaged
    {
        if (BufferDictionary.TryGetValue(name, out var uniformBuffer))
        {
            VeldridResources.GraphicsDevice.UpdateBuffer(uniformBuffer, 0, (T)value);
        }
        else
        {
            Util.LogError($"Shader '{ShaderName}' parameter '{name}' is not a buffer");
        }
    }
}
