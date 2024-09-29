using System.Runtime.CompilerServices;
using SDL;

namespace Lutra.Utility;

public static class SdlExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SDL_bool SdlBool(this bool value)
    {
        return value ? SDL_bool.SDL_TRUE : SDL_bool.SDL_FALSE;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Bool(this SDL_bool value)
    {
        return value == SDL_bool.SDL_TRUE;
    }
}
