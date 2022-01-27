using Lutra.Utility;
using Veldrid;

namespace Lutra.Rendering;

public class SpriteShader
{
    internal string ShaderName;
    internal BindableResource[] Resources;

    public SpriteShader(string shaderFilename, params (string Name, object Value)[] parameters)
    {
        ShaderName = System.IO.Path.GetFileName(shaderFilename);
        var shaderBytes = AssetManager.LoadBytes(shaderFilename);

        var count = parameters.Length;

        ResourceLayoutElementDescription[] elements = new ResourceLayoutElementDescription[count];
        Resources = new BindableResource[count];

        for (int i = 0; i < count; i++)
        {
            var param = parameters[i];
            var value = param.Value;

            if (value is LutraTexture)
            {
                elements[i] = new ResourceLayoutElementDescription(param.Name, ResourceKind.TextureReadOnly, ShaderStages.Fragment);
                Resources[i] = (value as LutraTexture).TextureView;
            }
            else if (value is Color)
            {
                elements[i] = new ResourceLayoutElementDescription($"{param.Name}Buffer", ResourceKind.UniformBuffer, ShaderStages.Fragment);
                var uniformBuffer = VeldridResources.Factory.CreateBuffer(new BufferDescription(16u, BufferUsage.UniformBuffer));
                VeldridResources.GraphicsDevice.UpdateBuffer(uniformBuffer, 0, ((Color)value).ToVector4());
                Resources[i] = uniformBuffer;
            }
            else if (value is float)
            {
                elements[i] = new ResourceLayoutElementDescription($"{param.Name}Buffer", ResourceKind.UniformBuffer, ShaderStages.Fragment);
                var uniformBuffer = VeldridResources.Factory.CreateBuffer(new BufferDescription(16u, BufferUsage.UniformBuffer));
                VeldridResources.GraphicsDevice.UpdateBuffer(uniformBuffer, 0, (float)value);
                Resources[i] = uniformBuffer;
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

        var layoutDesc = new ResourceLayoutDescription(elements);

        Draw.SpriteRenderPipeline.CreateShaderPipeline(ShaderName, shaderBytes, layoutDesc);
    }
}
