using System.Collections.Generic;
using System.Linq;
using Lutra.Utility;
using Lutra.Utility.Collections;
using SDL;

namespace Lutra.Input;

/// <summary>
/// Class used for managing input from the keyboard, mouse, and controllers. Updated by the active Game.
/// </summary>
public static class InputManager
{
    #region Private Fields

    private static List<SDL_JoystickID> controllerIds = new();

    private static HashSet<Key> prevKeysDown = new();
    private static HashSet<Key> keysDown = new();
    private static HashSet<MouseButton> prevMouseButtonsDown = new();
    private static HashSet<MouseButton> mouseButtonsDown = new();

    private static List<VirtualController> virtualControllers = new();

    private static Dictionary<SDL_JoystickID, Controller> controllers = new();

    #endregion

    #region Internal Fields & Properties

    internal static IEnumerable<Key> KeysPressedThisFrame => keysDown.Except(prevKeysDown);
    internal static IEnumerable<Key> KeysReleasedThisFrame => prevKeysDown.Except(keysDown);
    internal static string KeyStringPerFrame = "";

    #endregion

    #region Public Fields

    /// <summary>
    /// The maximum size of the string of recent key presses.
    /// </summary>
    public static int KeystringSize = 500;

    /// <summary>
    /// Determines if the mouse should be locked to the center of the screen.
    /// </summary>
    public static bool CenteredMouse = false; // TODO

    #endregion

    #region Public Properties

    /// <summary>
    /// The current number of controllers connected.
    /// </summary>
    public static int ControllersConnected => controllerIds.Count;

    /// <summary>
    /// The currently connected controllers.
    /// </summary>
    public static IReadOnlyCollection<Controller> Controllers => controllers.Values;

    /// <summary>
    /// The last known key that was pressed.
    /// </summary>
    public static Key LastKey { get; private set; }

    /// <summary>
    /// The last known mouse button that was pressed.
    /// </summary>
    public static MouseButton LastMouseButton { get; private set; }

    /// <summary>
    /// The change in the mouse wheel value this update.
    /// </summary>
    public static float MouseWheelDelta { get; private set; }

    /// <summary>
    /// The X movement of the mouse since the last update.
    /// </summary>
    public static int MouseDeltaX { get; private set; }

    /// <summary>
    /// The Y movement of the mouse since the last update.
    /// </summary>
    public static int MouseDeltaY { get; private set; }

    /// <summary>
    /// The raw X position of the mouse.
    /// </summary>
    public static int MouseRawX { get; private set; }

    /// <summary>
    /// The raw Y position of the mouse.
    /// </summary>
    public static int MouseRawY { get; private set; }

    /// <summary>
    /// The current X position of the mouse.
    /// </summary>
    public static int MouseX
    {
        get
        {   // NOTE: Scaling might only work with locked aspect scaling? Needs testing.
            float mouseX = MouseRawX;
            var windowStart = (Util.Scale(Game.Instance.Window.SurfaceBounds.X, -1.0f, 1.0f, 0.0f, 1.0f) * Game.Instance.Window.Width) / Game.Instance.Window.SurfaceScale;
            mouseX -= windowStart;
            mouseX /= Game.Instance.Window.SurfaceScale;
            return (int)mouseX;
        }
    }

    /// <summary>
    /// The current Y position of the mouse.
    /// </summary>
    public static int MouseY
    {
        get
        {   // NOTE: Scaling might only work with locked aspect scaling? Needs testing.
            float mouseY = MouseRawY;
            var windowStart = (Util.Scale(Game.Instance.Window.SurfaceBounds.Y, -1.0f, 1.0f, 0.0f, 1.0f) * Game.Instance.Window.Height) / Game.Instance.Window.SurfaceScale;
            mouseY -= windowStart;
            mouseY /= Game.Instance.Window.SurfaceScale;
            return (int)mouseY;
        }
    }

    /// <summary>
    /// The X position of the mouse in world space.
    /// </summary>
    public static float MouseWorldX => MouseX + (Game.Instance.CameraManager.ActiveCamera.X - Game.Instance.HalfWidth);

    /// <summary>
    /// The Y position of the mouse in world space.
    /// </summary>
    public static float MouseWorldY => MouseY + (Game.Instance.CameraManager.ActiveCamera.Y - Game.Instance.HalfHeight);

    /// <summary>
    /// The current string of keys that were pressed.
    /// </summary>
    public static string KeyString = "";

    #endregion

    #region Public Methods

    /// <summary>
    /// Get the name of the Controller.
    /// </summary>
    /// <param name="id">The connection id of the Controller.</param>
    /// <returns>The name of the Controller. null if invalid id.</returns>
    public static string GetControllerName(Controller controller)
    {
        return controllerIds.Contains(controller.SdlJoystickID) ? SDL3.SDL_GetGamepadNameForID(controller.SdlJoystickID) : null;
    }

    /// <summary>
    /// Get the vendor id of the Controller.
    /// </summary>
    /// <param name="id">The connection id of the Controller.</param>
    /// <returns>The vendor id of the Controller. 0 if invalid id.</returns>
    public static ushort GetControllerVendorId(Controller controller)
    {
        return controllerIds.Contains(controller.SdlJoystickID) ? SDL3.SDL_GetGamepadVendorForID(controller.SdlJoystickID) : default;
    }

    /// <summary>
    /// Get the product id of the Controller.
    /// </summary>
    /// <param name="id">The connection id of the Controller.</param>
    /// <returns>The name of the Controller. 0 if invalid id.</returns>
    public static ushort GetControllerProductId(Controller controller)
    {
        return controllerIds.Contains(controller.SdlJoystickID) ? SDL3.SDL_GetGamepadProductForID(controller.SdlJoystickID) : default;
    }

    /// <summary>
    /// Check if any key has been pressed this update.
    /// </summary>
    /// <returns>True if a key has been pressed.</returns>
    public static bool AnyKeyPressed()
    {
        foreach (var key in keysDown)
        {
            if (!prevKeysDown.Contains(key)) return true;
        }
        return false;
    }

    /// <summary>
    /// Check if a key has been pressed this update.
    /// </summary>
    /// <param name="k">The key to check.</param>
    /// <returns>True if the key has been pressed.</returns>
    public static bool KeyPressed(Key k)
    {
        return keysDown.Contains(k) && !prevKeysDown.Contains(k);
    }

    /// <summary>
    /// Check if a key has been released this update.
    /// </summary>
    /// <param name="k">The key to check.</param>
    /// <returns>True if the key has been released.</returns>
    public static bool KeyReleased(Key k)
    {
        return !keysDown.Contains(k) && prevKeysDown.Contains(k);
    }

    /// <summary>
    /// Check if a key is currently down.
    /// </summary>
    /// <param name="k">The key to check.</param>
    /// <returns>True if the key is down.</returns>
    public static bool KeyDown(Key k)
    {
        return keysDown.Contains(k);
    }

    /// <summary>
    /// Check if a key is currently up.
    /// </summary>
    /// <param name="k">The key to check.</param>
    /// <returns>True if the key is up.</returns>
    public static bool KeyUp(Key k)
    {
        return !keysDown.Contains(k);
    }

    /// <summary>
    /// Check if a MouseButton is pressed.
    /// </summary>
    /// <param name="b">The MouseButton to check.</param>
    /// <returns>True if the MouseButton is pressed.</returns>
    public static bool MouseButtonPressed(MouseButton b)
    {
        return mouseButtonsDown.Contains(b) && !prevMouseButtonsDown.Contains(b);
    }

    /// <summary>
    /// Check if a MouseButton is pressed.
    /// </summary>
    /// <param name="b">The MouseButton to check.</param>
    /// <returns>True if the MouseButton is pressed.</returns>
    public static bool MouseButtonReleased(MouseButton b)
    {
        return !mouseButtonsDown.Contains(b) && prevMouseButtonsDown.Contains(b);
    }

    /// <summary>
    /// Check if a MouseButton is pressed.
    /// </summary>
    /// <param name="b">The MouseButton to check.</param>
    /// <returns>True if the MouseButton is pressed.</returns>
    public static bool MouseButtonDown(MouseButton b)
    {
        return mouseButtonsDown.Contains(b);
    }

    /// <summary>
    /// Check if a MouseButton is pressed.
    /// </summary>
    /// <param name="b">The MouseButton to check.</param>
    /// <returns>True if the MouseButton is pressed.</returns>
    public static bool MouseButtonUp(MouseButton b)
    {
        return !mouseButtonsDown.Contains(b);
    }

    /// <summary>
    /// Clear the string of recently pressed keys.
    /// </summary>
    public static void ClearKeystring()
    {
        KeyString = "";
    }

    /// <summary>
    /// Add a virtual controller to be updated.
    /// </summary>
    public static void AddVirtualController(VirtualController virtualController)
    {
        virtualControllers.Add(virtualController);
    }

    #endregion

    #region Internal Methods

    internal static void Initialize()
    {
        SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_GAMEPAD);
        unsafe { SDL3.SDL_StartTextInput(Game.Instance.Window.SdlWindowHandle); }
        ProcessSdlEvents();
        RegisterControllers();
    }

    internal static void Update()
    {
        StartNewFrame();
        ProcessSdlEvents();

        foreach (var virtualController in virtualControllers)
        {
            virtualController.Update();
        }
    }

    public static void MoveMouseTo(int x, int y)
    {
        var window = Game.Instance.Window;

        var boundsStartX = Util.Scale(window.SurfaceBounds.X, -1.0f, 1.0f, 0.0f, 1.0f);
        var windowStartX = boundsStartX * window.Width / window.SurfaceScale;
        var mouseX = (x + windowStartX) * window.SurfaceScale;

        var boundsStartY = Util.Scale(window.SurfaceBounds.Y, -1.0f, 1.0f, 0.0f, 1.0f);
        var windowStartY = boundsStartY * window.Height / window.SurfaceScale;
        var mouseY = (y + windowStartY) * window.SurfaceScale;

        window.MoveMouseTo((int)mouseX, (int)mouseY);
    }

    #endregion

    #region Private Methods

    private static void StartNewFrame()
    {
        prevKeysDown = keysDown.Clone();
        prevMouseButtonsDown = mouseButtonsDown.Clone();
        KeyStringPerFrame = "";

        foreach (var controller in controllers.Values)
        {
            controller.StartNewFrame();
        }
    }

    private unsafe static void ProcessSdlEvents()
    {
        SDL_Event sdlEvent;
        while (SDL3.SDL_PollEvent(&sdlEvent).Bool())
        {
            ProcessSdlEvent(ref sdlEvent);
            Game.Instance?.Window?.ProcessSdlEvent(ref sdlEvent);
        }
    }

    private static unsafe void RegisterControllers()
    {
        controllerIds.Clear();

        var gamepadIDs = SDL3.SDL_GetGamepads();
        for (var i = 0; i < gamepadIDs.Count; i++)
        {
            var joystickID = gamepadIDs[i];
            var sdlGamepad = SDL3.SDL_OpenGamepad(joystickID);
            var sdlJoystick = SDL3.SDL_GetGamepadJoystick(sdlGamepad);
            controllerIds.Add(joystickID);

            controllers.Remove(joystickID);
            controllers.Add(joystickID, new Controller(*sdlGamepad, joystickID));
        }

        // Remove controllers that are no longer connected
        foreach (var controllerId in controllers.Keys.ToArray())
        {
            if (!controllerIds.Contains(controllerId))
            {
                controllers.Remove(controllerId);
            }
        }
    }

    private static void ProcessSdlEvent(ref SDL_Event sdlEvent)
    {
        switch (sdlEvent.Type)
        {
            // Controller Events
            case SDL_EventType.SDL_EVENT_GAMEPAD_ADDED:
            case SDL_EventType.SDL_EVENT_GAMEPAD_REMOVED:
            case SDL_EventType.SDL_EVENT_GAMEPAD_REMAPPED:
                RegisterControllers();
                break;
            case SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_UP:
            case SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_DOWN:
                ProcessSdlButtonEvent(ref sdlEvent.gbutton);
                break;
            case SDL_EventType.SDL_EVENT_GAMEPAD_AXIS_MOTION:
                ProcessSdlAxisEvent(ref sdlEvent.gaxis);
                break;
            // Keyboard Events
            case SDL_EventType.SDL_EVENT_KEY_DOWN:
            case SDL_EventType.SDL_EVENT_KEY_UP:
                ProcessSdlKeyEvent(ref sdlEvent.key);
                break;
            case SDL_EventType.SDL_EVENT_TEXT_INPUT:
                ProcessSdlTextEvent(ref sdlEvent.text);
                break;
            case SDL_EventType.SDL_EVENT_TEXT_EDITING:
                ProcessSdlTextEditEvent(ref sdlEvent.edit);
                break;
            // Mouse Events
            case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
            case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                ProcessSdlMouseButtonEvent(ref sdlEvent.button);
                break;
            case SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                MouseWheelDelta = sdlEvent.wheel.y;
                break;
            case SDL_EventType.SDL_EVENT_MOUSE_MOTION:
                ProcessSdlMouseMotionEvent(ref sdlEvent.motion);
                break;
        }
    }

    private static void ProcessSdlButtonEvent(ref SDL_GamepadButtonEvent buttonEvent)
    {
        var controllerId = buttonEvent.which;

        if (controllers.TryGetValue(controllerId, out var controller))
        {
            var button = (ControllerButton)buttonEvent.button;

            if (buttonEvent.down.Bool())
            {
                controller.ButtonDownEvent(button);
            }
            else
            {
                controller.ButtonUpEvent(button);
            }
        }
    }

    private static void ProcessSdlAxisEvent(ref SDL_GamepadAxisEvent axisEvent)
    {
        var controllerId = axisEvent.which;

        if (controllers.TryGetValue(controllerId, out var controller))
        {
            var axis = (ControllerAxis)(axisEvent.axis);
            var value = Util.Scale(axisEvent.value, short.MinValue, short.MaxValue, -1f, 1f);

            controller.AxisEvent(axis, value);
        }
    }

    private static void ProcessSdlKeyEvent(ref SDL_KeyboardEvent keyEvent)
    {
        var key = (Key)keyEvent.scancode;

        if (keyEvent.down.Bool())
        {
            keysDown.Add(key);
            LastKey = key;

            if (key == Key.Backspace)
            {
                KeyString = KeyString.Substring(0, (int)Util.Max(0, KeyString.Length - 1));
            }
        }
        else
        {
            keysDown.Remove(key);
        }
    }

    private unsafe static void ProcessSdlTextEvent(ref SDL_TextInputEvent textEvent)
    {
        KeyString += textEvent.GetText();
        KeyStringPerFrame += textEvent.GetText();
    }

    private static void ProcessSdlTextEditEvent(ref SDL_TextEditingEvent editEvent)
    {
        var editText = editEvent.GetText();
        int startIndex = editEvent.start;
        int editLength = editEvent.length;

        KeyString = KeyString.Remove(startIndex, editLength);
        KeyString = KeyString.Insert(startIndex, editText.Substring(0, editLength));
    }

    private static void ProcessSdlMouseButtonEvent(ref SDL_MouseButtonEvent mbEvent)
    {
        var button = (MouseButton)mbEvent.button;

        if (mbEvent.down.Bool())
        {
            mouseButtonsDown.Add(button);
            LastMouseButton = button;
        }
        else
        {
            mouseButtonsDown.Remove(button);
        }
    }

    private static void ProcessSdlMouseMotionEvent(ref SDL_MouseMotionEvent mmEvent)
    {
        // TODO: test this
        MouseDeltaX = (int)mmEvent.xrel;
        MouseDeltaY = (int)mmEvent.yrel;
        MouseRawX = (int)mmEvent.x;
        MouseRawY = (int)mmEvent.y;
    }

    #endregion
}
