using Lutra.Utility;

namespace Lutra.Rendering.Shaders;

public static class BuiltinShader
{
    internal static string InstancedVertex = "Lutra.Shaders.instanced.vert";
    internal static string SpriteVertex = "Lutra.Shaders.sprite.vert";
    internal static string SpriteFragment = "Lutra.Shaders.sprite.frag";
    internal static string SurfaceVertex = "Lutra.Shaders.surface.vert";
    internal static string SurfaceFragment = "Lutra.Shaders.surface.frag";

    public static byte[] InstancedVertexBytes => AssetManager.BuiltinShaderBytesCache[InstancedVertex];
    public static byte[] SpriteVertexBytes => AssetManager.BuiltinShaderBytesCache[SpriteVertex];
    public static byte[] SpriteFragmentBytes => AssetManager.BuiltinShaderBytesCache[SpriteFragment];
    public static byte[] SurfaceVertexBytes => AssetManager.BuiltinShaderBytesCache[SurfaceVertex];
    public static byte[] SurfaceFragmentBytes => AssetManager.BuiltinShaderBytesCache[SurfaceFragment];
}
