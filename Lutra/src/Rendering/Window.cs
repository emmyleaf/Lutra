using System.Runtime.InteropServices;
using Lutra.Utility;

namespace Lutra.Rendering;

public class Window
{
    #region Private Fields

    private readonly static RectFloat FullWindowBounds = new RectFloat(-1, -1, 2, 2);

    private readonly Game _game;

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
        get => VeldridResources.Sdl2Window.CursorVisible;
        set => VeldridResources.Sdl2Window.CursorVisible = value;
    }

    /// <summary>
    /// If the game window is currently fullscreen.
    /// </summary>
    public bool Fullscreen => VeldridResources.Sdl2Window.WindowState == Veldrid.WindowState.FullScreen;
    
    /// <summary>
    /// If the game window is currently borderless fullscreen.
    /// </summary>
    public bool BorderlessFullscreen => VeldridResources.Sdl2Window.WindowState == Veldrid.WindowState.BorderlessFullScreen;

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the window to the resolution of the screen in fullscreen mode.
    /// If window is already set to fullscreen, forces surface bounds recalculation in case other options have changed.
    /// </summary>
    public void SetFullscreen()
    {
        if (VeldridResources.Sdl2Window.WindowState != Veldrid.WindowState.FullScreen)
        {
            unsafe
            {
                Veldrid.Sdl2.SDL_DisplayMode displayMode;
                if (Veldrid.Sdl2.Sdl2Native.SDL_GetDesktopDisplayMode(0, &displayMode) == 0)
                {
                    VeldridResources.Sdl2Window.Width = displayMode.w;
                    VeldridResources.Sdl2Window.Height = displayMode.h;
                }
                else
                {
                    var errorMsg = Marshal.PtrToStringAnsi((IntPtr)Veldrid.Sdl2.Sdl2Native.SDL_GetError());
                    Util.LogError($"SDL_GetDesktopDisplayMode failed: {errorMsg}");
                }
            }
            VeldridResources.Sdl2Window.WindowState = Veldrid.WindowState.FullScreen;
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
    public void SetScale(float scaleXY)
    {
        VeldridResources.Sdl2Window.WindowState = Veldrid.WindowState.Normal;
        VeldridResources.Sdl2Window.Width = (int)(_game.Width * scaleXY);
        VeldridResources.Sdl2Window.Height = (int)(_game.Height * scaleXY);
    }
    
    /// <summary>
    /// Set the window to use a borderless fullscreen mode.
    /// </summary>
    public void SetBorderlessFullscreen()
    {
        VeldridResources.Sdl2Window.WindowState = Veldrid.WindowState.BorderlessFullScreen;
    }

    /// <summary>
    /// Set the window title.
    /// </summary>
    /// <param name="title">The new title.</param>
    public void SetTitle(string title)
    {
        Title = title;
        VeldridResources.Sdl2Window.Title = title;
    }

    #endregion

    #region Internal

    internal Window(Game game, GameOptions options)
    {
        _game = game;
        Title = options.Title;
        Width = (int)(game.Width * options.ScaleXY);
        Height = (int)(game.Height * options.ScaleXY);
        SurfaceBounds = FullWindowBounds;
        LockAspectRatio = options.LockAspectRatio;
        LockIntegerScale = options.LockIntegerScale;
        UpdateSurfaceBounds();
    }

    internal void OnResized()
    {
        Width = VeldridResources.Sdl2Window.Width;
        Height = VeldridResources.Sdl2Window.Height;
        UpdateSurfaceBounds();
    }

    private void UpdateSurfaceBounds()
    {
        if (LockAspectRatio)
        {
            float gameAspectRatio = (float)_game.Width / (float)_game.Height;
            float windowAspectRatio = (float)Width / (float)Height;
            float surfaceScale;

            if (gameAspectRatio < windowAspectRatio)
            {
                surfaceScale = (float)Height / (float)_game.Height;
            }
            else
            {
                surfaceScale = (float)Width / (float)_game.Width;
            }
            SurfaceScale = surfaceScale;

            if (LockIntegerScale)
            {
                surfaceScale = MathF.Max(MathF.Floor(surfaceScale), 1f);
                SurfaceScale = surfaceScale;

                var surfaceX = (surfaceScale * _game.Width) / (-2f * MathF.Floor(Width / 2f));
                var surfaceY = (surfaceScale * _game.Height) / (-2f * MathF.Floor(Height / 2f));

                var surfaceWidth = (surfaceScale * _game.Width) / Width;
                var surfaceHeight = (surfaceScale * _game.Height) / Height;

                SurfaceBounds = new RectFloat(surfaceX, surfaceY, (2f * surfaceWidth), (2f * surfaceHeight));
            }
            else
            {
                var surfaceWidth = (surfaceScale * _game.Width) / Width;
                var surfaceHeight = (surfaceScale * _game.Height) / Height;

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
