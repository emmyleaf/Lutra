using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Lutra.Cameras;
using Lutra.Collision;
using Lutra.Graphics;
using Lutra.Input;
using Lutra.Rendering;
using Lutra.Utility;
using Lutra.Utility.Debugging;
using Lutra.Utility.Profiling;

namespace Lutra;

/// <summary>
/// The core Game class of Lutra. Only one instance is allowed to exist at once.
/// Create a new Game, then use Game.Start() to run it!
/// </summary>
public class Game
{
    #region Static Properties

    /// <summary>
    /// A reference to the active Game instance.
    /// </summary>
    public static Game Instance { get; private set; }

    #endregion

    private readonly GameOptions initialOptions;

    private string gameFolder = "lutragame";
    private Dictionary<string, Session> Sessions = new();

    // Internal Systems
    internal GameLoop GameLoop;
    internal CollisionSystem CollisionSystem;
    internal SceneSystem SceneSystem;

    // Public Systems
    public CameraManager CameraManager;
    public Window Window;

    /// <summary>
    /// If the game should draw all scenes on the stack including inactive scenes.
    /// </summary>
    public bool DrawInactiveScenes = false; // TODO

    /// <summary>
    /// The default color to draw in the letterboxed areas of the window.
    /// </summary>
    public Color LetterBoxColor = Color.Black;

    /// <summary>
    /// How long the game has been active.  Measured in units of delta time.
    /// </summary>
    public float Timer;

    /// <summary>
    /// An action that fires when the game is drawing to ImGui.
    /// </summary>
    public Action IMGUIDraw = delegate { };

    #region Public Properties

    /// <summary>
    /// A reference to the current scene being updated by the game.
    /// </summary>
    public Scene Scene { get; internal set; }

    /// <summary>
    /// The main surface that the game renders to. Only use after initialization.
    /// </summary>
    public Surface Surface { get; private set; }

    /// <summary>
    /// The default background color of the game. Only use after initialization.
    /// </summary>
    public Color Color
    {
        get => Surface.ClearColor;
        set => Surface.ClearColor = value;
    }

    /// <summary>
    /// How much time has passed since the last update.
    /// </summary>
    public float DeltaTime { get; private set; }

    /// <summary>
    /// When true, DeltaTime will be multiplied by 60.
    /// Useful when calculating values against the length of a 60fps frame, regardless of current framerate.
    /// </summary>
    public bool MeasureTimeInSixtiethSeconds { get; private set; }

    /// <summary>
    /// The internal width of the game.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// The internal height of the game.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Half of the internal width of the game.
    /// </summary>
    public int HalfWidth => Width / 2;

    /// <summary>
    /// Half of the internal height of the game.
    /// </summary>
    public int HalfHeight => Height / 2;

    /// <summary>
    /// The default folder to use for storing data files for the game.
    /// This will be a folder created in the current user's My Documents folder.
    /// The default is 'lutragame' so it will create a folder 'lutragame' in My Documents.
    /// </summary>
    public string GameFolder
    {
        get
        {
            return gameFolder;
        }
        set
        {
            gameFolder = value;

            var path = GameFolderFullPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }

    /// <summary>
    /// Get the full path for GameFolder.
    /// </summary>
    public string GameFolderFullPath
    {
        get
        {
            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                myDocs = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(myDocs, gameFolder);
        }
    }

    /// <summary>
    /// The desired framerate that the game should update at.
    /// </summary>
    public double TargetFramerate
    {
        get => GameLoop.TargetFrameRate;
        set => GameLoop.SetTargetFrameRate(value);
    }

    /// <summary>
    /// If the timestep should be fixed and the game should update faster/slower to hit the target when the framerate changes.
    /// </summary>
    public bool FixedTimeStep
    {
        get => GameLoop.FixedTimeStep;
        set => GameLoop.SetFixedTimestep(value);
    }

    public Action OnInitialize = delegate { };

    #endregion

    #region Constructors

    public Game(GameOptions options)
    {
        if (Instance != null) throw new NotSupportedException("Cannot create more than one Lutra.Game");

        initialOptions = options;

        Width = options.Width;
        Height = options.Height;
        MeasureTimeInSixtiethSeconds = options.MeasureTimeInSixtiethSeconds;

        GameLoop = new GameLoop(this, options.TargetFrameRate);
        CollisionSystem = new();
        SceneSystem = new SceneSystem(this);
        Window = new Window(this, options);

        CameraManager = new CameraManager(options.Width, options.Height);

        Instance = this;
    }

    #endregion

    #region Lifecycle Methods

    protected internal virtual void Initialize()
    {
        AssetManager.PreloadBuiltinShaders();
        Window.Initialize();
        VeldridResources.Initialize(this, initialOptions.PreferredGraphicsBackend);
        AssetManager.Initialize();
        InputManager.Initialize();
        DebugConsole.Initialize();
        Draw.Initialize();

        Surface = new Surface(initialOptions.Width, initialOptions.Height, initialOptions.ClearColor);

        OnInitialize?.Invoke();
    }

    protected internal virtual void Update()
    {
        DeltaTime = (float)GameLoop.ElapsedGameTime.TotalSeconds;
        if (MeasureTimeInSixtiethSeconds) DeltaTime *= 60f;

        Timer += DeltaTime;

        InputManager.Update();
        DebugConsole.HandleInput();
        VeldridResources.DEBUG_ImGuiUpdate(GameLoop.ElapsedGameTime.TotalSeconds); // imgui expects seconds always
        VeldridResources.HandleResize();

        SceneSystem.UpdateStack();
        CollisionSystem.Update();
        Scene?.InternalUpdate();

        if (!Window.Exists)
        {
            GameLoop.Exit();
        }
    }

    protected internal virtual void Render()
    {
        Draw.Begin(Surface);

        Scene?.InternalRender();

        Draw.SurfaceToWindow(Surface, Window, LetterBoxColor);

        DEBUG_Draw();

        Draw.End();
    }

    protected internal virtual void ShutDown()
    {
        SceneSystem.EndAllScenes();
        VeldridResources.ShutDown();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Start the game using a specific Scene. Shortcut for calling AddScene then using Start.
    /// </summary>
    /// <param name="firstScene">The Scene to begin the game with.</param>
    public void Start(Scene firstScene)
    {
        AddScene(firstScene);
        Start();
    }

    /// <summary>
    /// Start the game. No code past this call in your entry point will run until the game is stopped.
    /// </summary>
    public void Start()
    {
        GameLoop.Run();
    }

    /// <summary>
    /// Stop the game. After the game loop has stopped itself, execution will return to your entry point.
    /// </summary>
    public void Stop()
    {
        GameLoop.Exit();
        Window.Close();
    }

    /// <summary>
    /// Close the game. This is the same as Stop().
    /// </summary>
    public void Close()
    {
        Stop();
    }

    /// <summary>
    /// Switch to a new scene. This removes the scene stack!
    /// </summary>
    /// <param name="scene">The scene to switch to.</param>
    public void SwitchScene(Scene scene)
    {
        SceneSystem.SwitchToScene = scene;
    }

    /// <summary>
    /// Add a scene to the top of the stack.
    /// </summary>
    /// <param name="scene">The scene to add.</param>
    public void AddScene(Scene scene)
    {
        SceneSystem.ScenesToAdd.Enqueue(scene);
    }

    /// <summary>
    /// Remove the scene from the top of the scene stack. Note: this does not remove scenes added in the same frame.
    /// </summary>
    public void RemoveScene()
    {
        SceneSystem.RemoveSceneCount++;
    }

    /// <summary>
    /// Get the current scene cast to a specific type. Useful for when you extend Scene to your own class.
    /// </summary>
    /// <typeparam name="T">The class to return the scene as.</typeparam>
    /// <returns>The scene as T.</returns>
    public T GetScene<T>() where T : Scene
    {
        return SceneSystem.Stack.Peek() as T;
    }

    /// <summary>
    /// Adds a new Session to the game.
    /// </summary>
    /// <param name="name">The name of the Session.</param>
    /// <returns>The Session.</returns>
    public Session AddSession(string name)
    {
        var session = new Session(this, name);
        Sessions.Add(name, session);
        return session;
    }

    /// <summary>
    /// Get a session by the name.
    /// </summary>
    /// <param name="name">The name of the session.</param>
    /// <returns>A session if one is found, or null.</returns>
    public Session Session(string name)
    {
        return Sessions.GetValueOrDefault(name);
    }

    #endregion

    #region DEBUG

    private float fpsCounterElapsed = 0f;
    private int fpsCounter = 0;

    [Conditional("DEBUG")]
    private void DEBUG_Draw()
    {
        Scene?.IMGUIDraw();
        IMGUIDraw();
        DebugConsole.RenderDebugConsoleIMGUI();

        fpsCounter++;
        fpsCounterElapsed += (float)GameLoop.ElapsedGameTime.TotalSeconds;
        if (fpsCounterElapsed >= 1f)
        {
            var memoryInMegabytes = (GC.GetTotalMemory(false) / 1048576f).ToString("F");
            Window.Title = $"{Window.Title} ~ {fpsCounter.ToString()} fps ~ {GameLoop.ElapsedGameTime.TotalSeconds} delta ~ {memoryInMegabytes} MB";
            fpsCounter = 0;
            fpsCounterElapsed -= 1f;
        }
    }

    #endregion
}
