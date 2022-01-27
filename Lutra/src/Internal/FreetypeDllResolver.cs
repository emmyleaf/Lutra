using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Lutra;

internal static class FreetypeDllResolver
{
    private const string FreetypeDll = "freetype6";

    internal static void Register()
    {
        NativeLibrary.SetDllImportResolver(typeof(SharpFont.FT).Assembly, MapAndLoad);
    }

    private static IntPtr MapAndLoad(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        string mappedName = libraryName;

        if (mappedName == FreetypeDll)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (RuntimeInformation.OSArchitecture == Architecture.X64)
                {
                    mappedName = Path.Join("lib/x64/", mappedName);
                }

                if (RuntimeInformation.OSArchitecture == Architecture.X86)
                {
                    mappedName = Path.Join("lib/x86/", mappedName);
                }
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                mappedName = "libfreetype.so.6";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                mappedName = "libfreetype.6.dylib";
            }
        }

        return NativeLibrary.Load(mappedName, assembly, searchPath);
    }
}
