using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Lutra.Rendering;
using Lutra.Rendering.Text;

namespace Lutra.Utility
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Writes an IO.Stream into a byte array.
        /// </summary>
        public static byte[] ToArray(this Stream stream)
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }

    public static class AssetManager
    {
        public const string DEFAULT_ASSET_PATH = "Assets/";
        public const string BUILT_IN_SHADER_PATH = "Shaders/";

        public static string BasePath { get; internal set; }
        public static string AssetPath = DEFAULT_ASSET_PATH;
        public static bool DisableCache = false;

        internal static Dictionary<string, byte[]> BuiltinShaderBytesCache = [];
        private static readonly Dictionary<string, Font> FontCache = [];
        private static readonly Dictionary<string, LutraTexture> TextureCache = [];

        public static Action PreloadAssets = delegate { };

        public static void Initialize()
        {
            BasePath = AppDomain.CurrentDomain.BaseDirectory;

            PreloadAssets();
        }

        public static Stream LoadStream(string filename)
        {
            return File.OpenRead(GetAssetPath(filename));
        }

        public static byte[] LoadBytes(string filename)
        {
            return File.ReadAllBytes(GetAssetPath(filename));
        }

        public static Font GetFont(string filename, bool antialiased = true)
        {
            if (DisableCache || !FontCache.TryGetValue(filename, out Font font))
            {
                font = LoadFont(filename, antialiased);
                if (!DisableCache) { FontCache.Add(filename, font); }
            }
            return font;
        }

        public static LutraTexture GetTexture(string filename)
        {
            if (DisableCache || !TextureCache.TryGetValue(filename, out LutraTexture texture))
            {
                texture = LoadTexture(filename);
                if (!DisableCache) { TextureCache.Add(filename, texture); }
            }
            return texture;
        }

        internal static void PreloadBuiltinShaders()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            foreach (var name in names)
            {
                if (name.StartsWith("Lutra.Shaders"))
                {
                    using var stream = assembly.GetManifestResourceStream(name);
                    BuiltinShaderBytesCache.Add(name, stream.ToArray());
                }
            }
        }

        private static string GetAssetPath(string filename)
        {
            if (Path.IsPathRooted(filename) || filename.Contains("Assets/"))
            {
                return filename;
            }
            return Path.Join(BasePath, AssetPath, filename);
        }

        private static Font LoadFont(string filename, bool antialiased = true)
        {
            return new Font(LoadBytes(filename), antialiased);
        }

        private static LutraTexture LoadTexture(string filename)
        {
            using var fileStream = LoadStream(filename);
            return new LutraTexture(fileStream);
        }
    }
}
