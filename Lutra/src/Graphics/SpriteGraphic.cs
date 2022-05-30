using System.Numerics;
using Lutra.Rendering;
using Lutra.Rendering.Shaders;

namespace Lutra.Graphics;

/// <summary>
/// Abstract class used for anything that can be rendered by the SpriteRenderPipeline.
/// Rather than overriding Render in subclasses, you would usually override UpdateDrawable.
/// UpdateDrawable is only called if we are Dynamic or if NeedsUpdate is true (on Graphic or Transform).
/// The SpriteDrawable will then be drawn every render loop, if it has been updated or not.
/// </summary>
public abstract class SpriteGraphic : Graphic
{
    public SpriteDrawable SpriteDrawable;

    /// <summary>
    /// If true, this SpriteGraphic will always update its SpriteDrawable.
    /// </summary>
    public bool Dynamic;

    /// <summary>
    /// Smooth the texture of a sprite image while scaling it.
    /// </summary>
    public bool Smooth;

    public ShaderData Shader;

    /// <summary>
    /// The blend mode to be applied to this graphic. Defaults to Alpha blend.
    /// </summary>
    public BlendMode Blend = BlendMode.Alpha;

    protected internal override void Render()
    {
        var updateTransforms = Transform.WillBeUpdated || (Entity != null && Entity.Transform.WillBeUpdated);
        if (Dynamic || NeedsUpdate || updateTransforms)
        {
            UpdateDrawable();
        }

        if (SpriteDrawable.Vertices.Count > 0)
        {
            Draw.SpriteDrawable(SpriteDrawable, Blend, Smooth, Shader);
        }
    }

    protected abstract void UpdateDrawable();

    protected void InitializeDrawable(LutraTexture texture, Matrix4x4 worldMatrix)
    {
        if (SpriteDrawable == null)
        {
            SpriteDrawable = new(texture, worldMatrix);
        }
        else
        {
            SpriteDrawable.Reinitialise(texture, worldMatrix);
        }
    }
}
