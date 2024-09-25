using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Lutra.Rendering;
using Lutra.Utility;
using Lutra.Utility.Collections;
using Veldrid.Sdl2;

namespace Lutra.Input;

/// <summary>
/// Class used for managing input from the keyboard, mouse, and controllers. Updated by the active Game.
/// </summary>
public static class InputManager
{
    #region Private Fields

    private static List<int> controllerIds = new();

    private static HashSet<Key> prevKeysDown = new();
    private static HashSet<Key> keysDown = new();
    private static HashSet<MouseButton> prevMouseButtonsDown = new();
    private static HashSet<MouseButton> mouseButtonsDown = new();

    private static List<VirtualController> virtualControllers = new();

    #endregion

    #region Internal Fields

    internal static Dictionary<int, Controller> Controllers = new();
    internal static Veldrid.InputSnapshot DEBUG_LatestInput;

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
    /// The controller ids of the currently connected controllers.
    /// </summary>
    public static IReadOnlyList<int> ControllerIds => controllerIds;

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
    /// Get a Controller.
    /// </summary>
    /// <param name="id">The connection id of the Controller.</param>
    /// <returns>The last button pressed. null if invalid id.</returns>
    public static Controller Controller(int id)
    {
        return Controllers.GetValueOrDefault(id);
    }

    /// <summary>
    /// Get the name of the Controller.
    /// </summary>
    /// <param name="id">The connection id of the Controller.</param>
    /// <returns>The name of the Controller. null if invalid id.</returns>
    public unsafe static string GetControllerName(int id)
    {
        if (!Controllers.TryGetValue(id, out var controller)) return null;
        var nameBytes = (IntPtr)Sdl2Native.SDL_GameControllerName(controller.SdlController);
        return Marshal.PtrToStringAnsi(nameBytes);
    }

    /// <summary>
    /// Get the vendor id of the Controller.
    /// </summary>
    /// <param name="id">The connection id of the Controller.</param>
    /// <returns>The vendor id of the Controller. 0 if invalid id.</returns>
    public static int GetControllerVendorId(int id)
    {
        if (!Controllers.TryGetValue(id, out var controller)) return 0;
        return Sdl2Native.SDL_GameControllerGetVendor(controller.SdlController);
    }

    /// <summary>
    /// Get the product id of the Controller.
    /// </summary>
    /// <param name="id">The connection id of the Controller.</param>
    /// <returns>The name of the Controller. 0 if invalid id.</returns>
    public static int GetControllerProductId(int id)
    {
        if (!Controllers.TryGetValue(id, out var controller)) return 0;
        return Sdl2Native.SDL_GameControllerGetProduct(controller.SdlController);
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
        Sdl2Native.SDL_Init(SDLInitFlags.GameController);
        Sdl2Events.Subscribe(ProcessSdlEvent);
        RegisterControllers();
    }

    internal static void Update()
    {
        StartNewFrame();
        ProcessSdl2Events();

        foreach (var virtualController in virtualControllers)
        {
            virtualController.Update();
        }
    }

    public static void MoveMouseTo(int x, int y)
    {
        float mouseX = x;
        var windowStartX = (Util.Scale(Game.Instance.Window.SurfaceBounds.X, -1.0f, 1.0f, 0.0f, 1.0f) * Game.Instance.Window.Width) / Game.Instance.Window.SurfaceScale;
        mouseX += windowStartX;
        mouseX *= Game.Instance.Window.SurfaceScale;
        
        float mouseY = y;
        var windowStartY = (Util.Scale(Game.Instance.Window.SurfaceBounds.Y, -1.0f, 1.0f, 0.0f, 1.0f) * Game.Instance.Window.Height) / Game.Instance.Window.SurfaceScale;
        mouseY += windowStartY;
        mouseY *= Game.Instance.Window.SurfaceScale;
        
        Sdl2Native.SDL_WarpMouseInWindow(VeldridResources.Sdl2Window.SdlWindowHandle, (int)mouseX, (int)mouseY);
    }

    #endregion

    #region Private Methods

    private static void StartNewFrame()
    {
        prevKeysDown = keysDown.Clone();
        prevMouseButtonsDown = mouseButtonsDown.Clone();

        foreach (var controller in Controllers.Values)
        {
            controller.StartNewFrame();
        }
    }

    private static void ProcessSdl2Events()
    {
        DEBUG_LatestInput = VeldridResources.Sdl2Window.PumpEvents();
    }

    private static unsafe void RegisterControllers()
    {
        controllerIds.Clear();

        for (var i = 0; i < Sdl2Native.SDL_NumJoysticks(); i++)
        {
            if (Sdl2Native.SDL_IsGameController(i))
            {
                var sdlController = Sdl2Native.SDL_GameControllerOpen(i);
                var sdlJoystick = Sdl2Native.SDL_GameControllerGetJoystick(sdlController);
                var instanceId = Sdl2Native.SDL_JoystickInstanceID(sdlJoystick);
                controllerIds.Add(instanceId);

                Controllers.Remove(instanceId);
                Controllers.Add(instanceId, new Controller(sdlController, sdlJoystick));
            }
        }

        // Remove controllers that are no longer connected
        foreach (var controllerId in Controllers.Keys.ToArray())
        {
            if (!controllerIds.Contains(controllerId))
            {
                Controllers.Remove(controllerId);
            }
        }
    }

    private static void ProcessSdlEvent(ref SDL_Event sdlEvent)
    {
        switch (sdlEvent.type)
        {
            // Controller Events
            case SDL_EventType.ControllerDeviceAdded:
            case SDL_EventType.ControllerDeviceRemoved:
            case SDL_EventType.ControllerDeviceRemapped:
                RegisterControllers();
                break;
            case SDL_EventType.ControllerButtonUp:
            case SDL_EventType.ControllerButtonDown:
                var buttonEvent = Unsafe.As<SDL_Event, SDL_ControllerButtonEvent>(ref sdlEvent);
                ProcessSdlButtonEvent(ref buttonEvent);
                break;
            case SDL_EventType.ControllerAxisMotion:
                var axisEvent = Unsafe.As<SDL_Event, SDL_ControllerAxisEvent>(ref sdlEvent);
                ProcessSdlAxisEvent(ref axisEvent);
                break;
            // Keyboard Events
            case SDL_EventType.KeyDown:
            case SDL_EventType.KeyUp:
                var keyEvent = Unsafe.As<SDL_Event, SDL_KeyboardEvent>(ref sdlEvent);
                ProcessSdlKeyEvent(ref keyEvent);
                break;
            case SDL_EventType.TextInput:
                var textEvent = Unsafe.As<SDL_Event, SDL_TextInputEvent>(ref sdlEvent);
                ProcessSdlTextEvent(ref textEvent);
                break;
            case SDL_EventType.TextEditing:
                var editEvent = Unsafe.As<SDL_Event, SDL2.SDL_TextEditingEvent>(ref sdlEvent);
                ProcessSdlTextEditEvent(ref editEvent);
                break;
            // Mouse Events
            case SDL_EventType.MouseButtonDown:
            case SDL_EventType.MouseButtonUp:
                var mbEvent = Unsafe.As<SDL_Event, SDL_MouseButtonEvent>(ref sdlEvent);
                ProcessSdlMouseButtonEvent(ref mbEvent);
                break;
            case SDL_EventType.MouseWheel:
                var mwEvent = Unsafe.As<SDL_Event, SDL_MouseWheelEvent>(ref sdlEvent);
                MouseWheelDelta = mwEvent.y;
                break;
            case SDL_EventType.MouseMotion:
                var mmEvent = Unsafe.As<SDL_Event, SDL_MouseMotionEvent>(ref sdlEvent);
                ProcessSdlMouseMotionEvent(ref mmEvent);
                break;
        }
    }

    private static void ProcessSdlButtonEvent(ref SDL_ControllerButtonEvent buttonEvent)
    {
        var controllerId = buttonEvent.which;

        if (Controllers.TryGetValue(controllerId, out var controller))
        {
            var button = (ControllerButton)(buttonEvent.button);
            var down = buttonEvent.state == 1;

            if (down)
            {
                controller.ButtonDownEvent(button);
            }
            else
            {
                controller.ButtonUpEvent(button);
            }
        }
    }

    private static void ProcessSdlAxisEvent(ref SDL_ControllerAxisEvent axisEvent)
    {
        var controllerId = axisEvent.which;

        if (Controllers.TryGetValue(controllerId, out var controller))
        {
            var axis = (ControllerAxis)(axisEvent.axis);
            var value = Util.Scale(axisEvent.value, short.MinValue, short.MaxValue, -1f, 1f);

            controller.AxisEvent(axis, value);
        }
    }

    private static void ProcessSdlKeyEvent(ref SDL_KeyboardEvent keyEvent)
    {
        var key = (Key)keyEvent.keysym.scancode;
        var down = keyEvent.state == 1;

        if (down)
        {
            keysDown.Add(key);
            LastKey = key;

            if(key == Key.Backspace)
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
        // This only adds valid text chars to KeyString.
        // If we want to handle backspace etc, we will need more logic here.
        fixed (byte* textBytes = textEvent.text)
        {
            var textInput = Marshal.PtrToStringAnsi((IntPtr)textBytes);
            KeyString += textInput;
        }
    }

    private unsafe static void ProcessSdlTextEditEvent(ref SDL2.SDL_TextEditingEvent editEvent)
    {
        fixed (byte* textBytes = editEvent.text)
        {
            var editText = Marshal.PtrToStringUTF8((IntPtr)textBytes);
            int startIndex = editEvent.start;
            int editLength = editEvent.length;

            KeyString = KeyString.Remove(startIndex, editLength);
            KeyString = KeyString.Insert(startIndex, editText.Substring(0, editLength));
        }
    }

    private static void ProcessSdlMouseButtonEvent(ref SDL_MouseButtonEvent mbEvent)
    {
        var button = (MouseButton)mbEvent.button;
        var down = mbEvent.state == 1;

        if (down)
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
        MouseDeltaX = mmEvent.xrel;
        MouseDeltaY = mmEvent.yrel;
        MouseRawX = mmEvent.x;
        MouseRawY = mmEvent.y;
    }

    #endregion
}
