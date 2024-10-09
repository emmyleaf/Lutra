using System.Collections.Generic;

namespace Lutra.Input;

/// <summary>
/// Class used for interpreting input as a button. 
/// Can use multiple sources of input like keyboard, mouse buttons, or controller axes and buttons.
/// The button input can also be controlled in code.
/// </summary>
public class VirtualButton
{
    #region Private Fields

    private bool currentButtonDown = false;
    private bool prevButtonDown = false;
    private bool forceDown = false;

    #endregion

    #region Public Fields

    /// <summary>
    /// The keys registered to the Button.
    /// </summary>
    public List<Key> Keys = [];

    /// <summary>
    /// The controller Buttons registered to the Button.
    /// </summary>
    public List<(Controller, ControllerButton)> ControllerButtons = [];

    /// <summary>
    /// The controller Axes and thresholds registered to the Button.
    /// </summary>
    public List<(Controller, ControllerAxis, bool)> ControllerAxisButtons = [];

    /// <summary>
    /// The mouse buttons registered to the Button.
    /// </summary>
    public List<MouseButton> MouseButtons = [];

    /// <summary>
    /// The mouse wheel registered to the Button.
    /// </summary>
    public List<MouseWheelDirection> MouseWheel = [];

    /// <summary>
    /// Determines if the Button is enabled.  If not enabled all tests return false.
    /// </summary>
    public bool Enabled = true;

    /// <summary>
    /// The time passed since the last button press.
    /// </summary>
    public float LastPressed = float.MaxValue;

    /// <summary>
    /// The time passed since the last button release.
    /// </summary>
    public float LastReleased = float.MaxValue;

    /// <summary>
    /// The threshold for an axis to act as a button press. Defaults to 0.5 or one half of the joystick's total range.
    /// </summary>
    public float AxisButtonThreshold = 0.5f;

    #endregion

    #region Public Properties

    /// <summary>
    /// If the button is currently controlled 
    /// </summary>
    public bool ForcedInput { get; private set; }

    /// <summary>
    /// Check if the button has been pressed.
    /// </summary>
    public bool Pressed => Enabled && currentButtonDown && !prevButtonDown;

    /// <summary>
    /// Check if the button has been released.
    /// </summary>
    public bool Released => Enabled && !currentButtonDown && prevButtonDown;

    /// <summary>
    /// Check if the button is down.
    /// </summary>
    public bool Down => Enabled && currentButtonDown;

    /// <summary>
    /// Check if the button is up.
    /// </summary>
    public bool Up => Enabled && !currentButtonDown;

    /// <summary>
    /// Returns true if this button is using any ControllerButtons from a Controller.
    /// </summary>
    public bool IsUsingControllerButtons => ControllerButtons.Count > 0;

    /// <summary>
    /// Returns true if this button is using any ControllerAxes as buttons.
    /// </summary>
    public bool IsUsingControllerAxisButtons => ControllerAxisButtons.Count > 0;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a Button.
    /// </summary>
    public VirtualButton()
    {
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Reset the Button to report no input.
    /// </summary>
    public void Reset()
    {
        prevButtonDown = false;
        currentButtonDown = false;
    }

    /// <summary>
    /// Clear all registered inputs for the Button.
    /// </summary>
    public void Clear()
    {
        Keys.Clear();
        ControllerButtons.Clear();
        ControllerAxisButtons.Clear();
        MouseButtons.Clear();
        MouseWheel.Clear();
    }

    /// <summary>
    /// Add a keyboard Key to the Button.
    /// </summary>
    /// <param name="keys">The key to add.</param>
    /// <returns>The Button.</returns>
    public VirtualButton AddKey(params Key[] keys)
    {
        foreach (var k in keys)
        {
            Keys.Add(k);
        }
        return this;
    }

    /// <summary>
    /// Add a mouse button to the Button.
    /// </summary>
    /// <param name="mouseButtons">The mouse button to add.</param>
    /// <returns>The Button.</returns>
    public VirtualButton AddMouseButton(params MouseButton[] mouseButtons)
    {
        foreach (var mb in mouseButtons)
        {
            MouseButtons.Add(mb);
        }
        return this;
    }

    /// <summary>
    /// Add the mouse wheel to the Button.
    /// </summary>
    /// <param name="direction">The mouse wheel direction to add.</param>
    /// <returns>The Button.</returns>
    public VirtualButton AddMouseWheel(MouseWheelDirection direction)
    {
        MouseWheel.Add(direction);
        return this;
    }

    /// <summary>
    /// Add a controller button to the Button.
    /// </summary>
    /// <param name="button">The controller button to add.</param>
    /// <param name="controller">The controller.</param>
    /// <returns>The Button.</returns>
    public VirtualButton AddControllerButton(Controller controller, ControllerButton button)
    {
        ControllerButtons.Add((controller, button));
        return this;
    }

    /// <summary>
    /// Add a controller axis to the Button.
    /// </summary>
    /// <param name="axis">The controller axis to add.</param>
    /// <param name="positive">If true, use +ve axis. If false, use -ve axis.</param>
    /// <param name="controller">The controller.</param>
    /// <returns>The Button.</returns>
    public VirtualButton AddAxisButton(Controller controller, ControllerAxis axis, bool positive)
    {
        ControllerAxisButtons.Add((controller, axis, positive));
        return this;
    }

    /// <summary>
    /// Force the state of the button.  This will override player input.
    /// </summary>
    /// <param name="state">The state of the button, true for down, false for up.</param>
    public void ForceState(bool state)
    {
        forceDown = state;
        ForcedInput = true;
    }

    /// <summary>
    /// Release the button's state from forced control.  Restores player input.
    /// </summary>
    public void ReleaseState()
    {
        ForcedInput = false;
    }

    /// <summary>
    /// Update the button status.
    /// </summary>
    public void Update()
    {
        // Fix for buttons that arent being updated.
        if (LastPressed == float.MaxValue) LastPressed = 0;
        if (LastReleased == float.MaxValue) LastReleased = 0;

        prevButtonDown = currentButtonDown;
        currentButtonDown = IsButtonDown();

        LastPressed += Game.Instance.DeltaTime;
        if (Pressed)
        {
            LastPressed = 0;
        }

        LastReleased += Game.Instance.DeltaTime;
        if (Released)
        {
            LastReleased = 0;
        }
    }

    #endregion

    private bool IsButtonDown()
    {
        if (ForcedInput)
        {
            return forceDown;
        }

        foreach (var k in Keys)
        {
            if (InputManager.KeyDown(k))
            {
                return true;
            }
        }

        foreach (var (controller, button) in ControllerButtons)
        {
            if (controller.ButtonDown(button))
            {
                return true;
            }
        }

        foreach (var (controller, axis, positive) in ControllerAxisButtons)
        {
            float val = controller.GetAxis(axis);

            if (positive && val >= AxisButtonThreshold)
            {
                return true;
            }
            if (!positive && val <= -AxisButtonThreshold)
            {
                return true;
            }
        }

        foreach (var mb in MouseButtons)
        {
            if (InputManager.MouseButtonDown(mb))
            {
                return true;
            }
        }

        foreach (var w in MouseWheel)
        {
            if (w == MouseWheelDirection.Down)
            {
                if (InputManager.MouseWheelDelta < 0)
                {
                    return true;
                }
            }
            if (w == MouseWheelDirection.Up)
            {
                if (InputManager.MouseWheelDelta > 0)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
