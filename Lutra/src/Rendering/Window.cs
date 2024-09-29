using Lutra.Utility;
using SDL;

namespace Lutra.Rendering;

public class Window
{
    #region Private Fields

    //TODO: only use opengl flag when opengl is the selected backend!
    private const SDL_WindowFlags DEFAULT_FLAGS =
        SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL_WindowFlags.SDL_WINDOW_RESIZABLE;

    private readonly static RectFloat FullWindowBounds = new RectFloat(-1, -1, 2, 2);

    private readonly Game _game;

    private bool _windowTitleUpdated;
    private string _titleSuffix;

    #endregion

    #region Internal Fields

    internal unsafe SDL_Window* SdlWindowHandle;
    internal SDL_WindowID SdlWindowID;

    #endregion

    #region Public Properties

    /// <summary>
    /// The title of the game displayed in the window.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The window width of the game.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// The window height of the game.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// The scale factor applied to the game's width to obtain the window's width.
    /// </summary>
    public int ScaleX => Width / _game.Width;

    /// <summary>
    /// The scale factor applied to the game's height to obtain the window's height.
    /// </summary>
    public int ScaleY => Height / _game.Height;

    /// <summary>
    /// The bounds of where the game surface will be drawn to the window.
    /// Every point of this rectangle must fit within [-1,1] for both X and Y.
    /// </summary>
    public RectFloat SurfaceBounds { get; private set; }

    /// <summary>
    /// Maintain the original aspect ratio of the game when scaling the window.
    /// </summary>
    public bool LockAspectRatio { get; set; }

    /// <summary>
    /// Maintain integer scaling of the game when scaling the window.
    /// Only applies when LockAspectRatio is also true.
    /// </summary>
    public bool LockIntegerScale { get; set; }

    /// <summary>
    /// This property returns the current relative scale
    /// of the rendered Surface along its biggest dimension.
    /// </summary>
    public float SurfaceScale { get; private set; }

    /// <summary>
    /// The visibilty of the mouse.
    /// </summary>
    public bool MouseVisible
    {
        get => SDL3.SDL_CursorVisible().Bool();
        set => _ = value ? SDL3.SDL_ShowCursor() : SDL3.SDL_HideCursor();
    }

    /// <summary>
    /// If the game window is currently focused.
    /// </summary>
    public unsafe bool Focused =>
        SDL3.SDL_GetWindowFlags(SdlWindowHandle).HasFlag(SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS);

    /// <summary>
    /// If the game window is currently fullscreen.
    /// </summary>
    public unsafe bool Fullscreen =>
        SDL3.SDL_GetWindowFlags(SdlWindowHandle).HasFlag(SDL_WindowFlags.SDL_WINDOW_FULLSCREEN);

    /// <summary>
    /// If the game window is currently resizable.
    /// </summary>
    public unsafe bool Resizable
    {
        get => SDL3.SDL_GetWindowFlags(SdlWindowHandle).HasFlag(SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
        set => SDL3.SDL_SetWindowResizable(SdlWindowHandle, value.SdlBool());
    }

    /// <summary>
    /// If the game window currently has a border.
    /// </summary>
    public unsafe bool BorderVisible
    {
        get => SDL3.SDL_GetWindowFlags(SdlWindowHandle).HasFlag(SDL_WindowFlags.SDL_WINDOW_BORDERLESS);
        set => SDL3.SDL_SetWindowBordered(SdlWindowHandle, value.SdlBool());
    }

    /// <summary>
    /// If the game window is currently visible.
    /// </summary>
    public unsafe bool Visible
    {
        get => !SDL3.SDL_GetWindowFlags(SdlWindowHandle).HasFlag(SDL_WindowFlags.SDL_WINDOW_HIDDEN);
        set => _ = value ? SDL3.SDL_ShowWindow(SdlWindowHandle) : SDL3.SDL_HideWindow(SdlWindowHandle);
    }

    /// <summary>
    /// If the game window currently exists!
    /// </summary>
    public bool Exists { get; private set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Close the window.
    /// </summary>
    public void Close()
    {
        unsafe { SDL3.SDL_DestroyWindow(SdlWindowHandle); }
        Exists = false;
    }

    /// <summary>
    /// Sets the window to the resolution of the screen in fullscreen mode.
    /// If window is already set to fullscreen, forces surface bounds recalculation in case other options have changed.
    /// </summary>
    public void SetFullscreen()
    {
        if (!Fullscreen)
        {
            unsafe { SDL3.SDL_SetWindowFullscreen(SdlWindowHandle, SDL_bool.SDL_TRUE); }
        }
        else
        {
            UpdateSurfaceBounds();
        }
    }

    /// <summary>
    /// Set the window size to a scalar multiplier of the game's size.
    /// </summary>
    /// <param name="scaleXY">The scaling value for both directions.</param>
    public unsafe void SetScale(float scaleXY)
    {
        SDL3.SDL_SetWindowFullscreen(SdlWindowHandle, SDL_bool.SDL_FALSE);
        SDL3.SDL_SetWindowSize(SdlWindowHandle, (int)(_game.Width * scaleXY), (int)(_game.Height * scaleXY));
    }

    /// <summary>
    /// Set the window title.
    /// </summary>
    /// <param name="title">The new title.</param>
    public void SetTitle(string title, string suffix = null)
    {
        Title = title;
        _titleSuffix = suffix;
        _windowTitleUpdated = true;
    }

    public unsafe void MoveMouseTo(int x, int y)
    {
        SDL3.SDL_WarpMouseInWindow(SdlWindowHandle, x, y);
    }

    //TODO: setting the window size if the game's size changes!

    #endregion

    #region Internal

    internal Window(Game game, GameOptions options)
    {
        _game = game;
        Title = options.Title;
        Width = (int)(game.Width * options.ScaleXY);
        Height = (int)(game.Height * options.ScaleXY);
        LockAspectRatio = options.LockAspectRatio;
        LockIntegerScale = options.LockIntegerScale;
        UpdateSurfaceBounds();
    }

    internal unsafe void Initialize()
    {
        SDL3.SDL_SetHint("SDL_MOUSE_FOCUS_CLICKTHROUGH", "1");

        SdlWindowHandle = SDL3.SDL_CreateWindow(Title, Width, Height, DEFAULT_FLAGS);
        SdlWindowID = SDL3.SDL_GetWindowID(SdlWindowHandle);
        Exists = true;

        // TODO: care about window position more... // UpdateWindowPosition();
        UpdateWindowSize();
    }

    internal void Update()
    {
        CheckWindowTitle();
    }

    internal void ProcessSdlEvent(ref SDL_Event sdlEvent)
    {
        switch (sdlEvent.Type)
        {
            case SDL_EventType.SDL_EVENT_QUIT:
            case SDL_EventType.SDL_EVENT_TERMINATING:
            case SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED:
                Close();
                break;
            case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
            case SDL_EventType.SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED:
            case SDL_EventType.SDL_EVENT_WINDOW_MINIMIZED:
            case SDL_EventType.SDL_EVENT_WINDOW_MAXIMIZED:
            case SDL_EventType.SDL_EVENT_WINDOW_RESTORED:
                UpdateWindowSize();
                break;
            case SDL_EventType.SDL_EVENT_WINDOW_MOVED:
                // TODO: set window position? // (windowEvent.data1, windowEvent.data2);
                break;
            default:
                break; // Ignore
        }
    }

    #endregion

    #region Private Methods

    private void CheckWindowTitle()
    {
        if (_windowTitleUpdated)
        {
            _windowTitleUpdated = false;
            unsafe { SDL3.SDL_SetWindowTitle(SdlWindowHandle, Title + _titleSuffix); }
        }
    }

    private unsafe void UpdateWindowSize()
    {
        int width, height;
        SDL3.SDL_GetWindowSize(SdlWindowHandle, &width, &height);
        Width = width;
        Height = height;

        UpdateSurfaceBounds();

        VeldridResources.WindowResized = true;
    }

    private void UpdateSurfaceBounds()
    {
        if (LockAspectRatio)
        {
            float gameAspectRatio = (float)_game.Width / (float)_game.Height;
            float windowAspectRatio = (float)Width / (float)Height;

            if (gameAspectRatio < windowAspectRatio)
            {
                SurfaceScale = (float)Height / (float)_game.Height;
            }
            else
            {
                SurfaceScale = (float)Width / (float)_game.Width;
            }

            if (LockIntegerScale)
            {
                SurfaceScale = MathF.Max(MathF.Floor(SurfaceScale), 1f);

                var surfaceX = (SurfaceScale * _game.Width) / (-2f * MathF.Floor(Width / 2f));
                var surfaceY = (SurfaceScale * _game.Height) / (-2f * MathF.Floor(Height / 2f));

                var surfaceWidth = (SurfaceScale * _game.Width) / Width;
                var surfaceHeight = (SurfaceScale * _game.Height) / Height;

                SurfaceBounds = new RectFloat(surfaceX, surfaceY, (2f * surfaceWidth), (2f * surfaceHeight));
            }
            else
            {
                var surfaceWidth = (SurfaceScale * _game.Width) / Width;
                var surfaceHeight = (SurfaceScale * _game.Height) / Height;

                SurfaceBounds = new RectFloat(-surfaceWidth, -surfaceHeight, (2f * surfaceWidth), (2f * surfaceHeight));
            }
        }
        else
        {
            SurfaceBounds = FullWindowBounds;
            SurfaceScale = (float)Width / (float)_game.Width;
        }
    }

    #endregion
}
