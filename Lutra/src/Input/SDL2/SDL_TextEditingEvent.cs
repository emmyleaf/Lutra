using Veldrid.Sdl2;
using System.Runtime.CompilerServices;
namespace Lutra.Input.SDL2;

/// <summary>
/// Keyboard text edit event structure (event.text.*)
/// </summary>
public unsafe struct SDL_TextEditingEvent
{
        public const int MaxTextSize = 32;

        /// <summary>
        /// SDL_TEXTEDITING
        /// </summary>
        public SDL_EventType type;
        public uint timestamp;

        /// <summary>
        /// The window with keyboard focus, if any.
        /// </summary>
        public uint windowID;

        /// <summary>
        /// The null-terminated edited text in UTF-8.
        /// </summary>
        public fixed byte text[MaxTextSize];

        /// <summary>
        /// The location to begin editing from.
        /// </summary>
        public int start;

        /// <summary>
        /// The length of the edited text.
        /// </summary>
        public int length;
}