using System.Numerics;
using Lutra.Rendering;

namespace Lutra.Graphics;

/// <summary>
/// Abstract class used for anything that can be rendered by the SpriteRenderPipeline.
/// Rather than overriding Render in subclasses, you would usually override UpdateDrawable.
/// UpdateDrawable is only called if we are Dynamic or if NeedsUpdate is true (on Graphic or Transform).
/// The SpriteDrawable will then be drawn every render loop, if it has been updated or not.
/// </summary>
public abstract class SpriteGraphic : Graphic
{
    protected SpriteDrawable SpriteDrawable;

    /// <summary>
    /// If true, this SpriteGraphic will always update its SpriteDrawable.
    /// </summary>
    public bool Dynamic;

    /// <summary>
    /// Smooth the texture of a sprite image while scaling it.
    /// </summary>
    // TODO: Implement this?!
    public bool Smooth;

    public SpriteShader Shader;

    protected override void Render()
    {
        var updateTransforms = Transform.WillBeUpdated || (Entity != null && Entity.Transform.WillBeUpdated);
        if (Dynamic || NeedsUpdate || updateTransforms)
        {
            UpdateDrawable();
        }

        if (SpriteDrawable.Vertices.Count > 0)
        {
            if (Shader != null)
            {
                Draw.SpriteDrawableShaders(SpriteDrawable, Shader);
            }
            else
            {
                Draw.SpriteDrawable(SpriteDrawable);
            }
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
