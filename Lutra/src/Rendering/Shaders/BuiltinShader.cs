using Lutra.Utility;

namespace Lutra.Rendering.Shaders;

public static class BuiltinShader
{
    internal static string InstancedVertex = "instanced.vert";
    internal static string InstancedFragment = "instanced.frag";
    internal static string SpriteVertex = "sprite.vert";
    internal static string SpriteFragment = "sprite.frag";
    internal static string SurfaceVertex = "surface.vert";
    internal static string SurfaceFragment = "surface.frag";

    internal static string[] Names = new[] {
        InstancedVertex, InstancedFragment,
        SpriteVertex, SpriteFragment,
        SurfaceVertex, SurfaceFragment
    };

    // TODO: Internal shaders as built in resources instead of this jank
    public static byte[] InstancedVertexBytes => AssetManager.BuiltinShaderBytesCache[InstancedVertex];
    public static byte[] InstancedFragmentBytes => AssetManager.BuiltinShaderBytesCache[InstancedFragment];
    public static byte[] SpriteVertexBytes => AssetManager.BuiltinShaderBytesCache[SpriteVertex];
    public static byte[] SpriteFragmentBytes => AssetManager.BuiltinShaderBytesCache[SpriteFragment];
    public static byte[] SurfaceVertexBytes => AssetManager.BuiltinShaderBytesCache[SurfaceVertex];
    public static byte[] SurfaceFragmentBytes => AssetManager.BuiltinShaderBytesCache[SurfaceFragment];
}
