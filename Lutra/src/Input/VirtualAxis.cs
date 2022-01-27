using System.Collections.Generic;
using System.Numerics;
using Lutra.Utility;

namespace Lutra.Input;

/// <summary>
/// Class that represents a virtual axis of input. Interprets both X and Y from -1 to 1.
/// Can use multiple sources of input like keyboard, mouse buttons, or controller axes and buttons.
/// The axis input can also be controlled in code.
/// </summary>
public class VirtualAxis
{
    #region Static Methods

    /// <summary>
    /// Create a new Axis that uses the arrow keys for movement.
    /// </summary>
    /// <returns>A new Axis.</returns>
    public static VirtualAxis CreateArrowKeys()
    {
        return new VirtualAxis(Key.Up, Key.Right, Key.Down, Key.Left);
    }

    /// <summary>
    /// Create a new Axis that uses WASD for movement.
    /// </summary>
    /// <returns>A new Axis.</returns>
    public static VirtualAxis CreateWASD()
    {
        return new VirtualAxis(Key.W, Key.D, Key.S, Key.A);
    }

    #endregion

    #region Private Fields

    /// <summary>
    /// The Keys to use.
    /// </summary>
    private Dictionary<Direction, List<Key>> Keys = new();

    /// <summary>
    /// The controller Buttons registered to the Button.
    /// </summary>
    private Dictionary<Direction, List<(int, ControllerButton)>> ControllerButtons = new();

    /// <summary>
    /// The X axes to use.
    /// </summary>
    private List<(int, ControllerAxis)> AxesX = new();

    /// <summary>
    /// The Y axes to use.
    /// </summary>
    private List<(int, ControllerAxis)> AxesY = new();

    #endregion

    #region Public Fields

    /// <summary>
    /// Determines if the axis is currently enabled. If false, X and Y will report 0.
    /// </summary>
    public bool Enabled = true;

    /// <summary>
    /// The range that must be exceeded by controller axes in order for their input to register.
    /// </summary>
    public float DeadZone = 0.15f;

    /// <summary>
    /// Determines if the DeadZone will be treated as 0 for controller axes.
    /// If true, remaps the positive and negative ranges DeadZone - 1 to 0 - 1.
    /// </summary>
    public bool RemapRange = true;

    /// <summary>
    /// Determines if raw data coming from the controller axes should be rounded to 2 digits.
    /// </summary>
    public bool RoundInput = true;

    /// <summary>
    /// Determines if input has any effect on the axis. When set to true the axis will remain at
    /// the X and Y it was at when locked.
    /// </summary>
    public bool Locked = false;

    #endregion

    #region Public Properties

    /// <summary>
    /// The current Vector2 position of the axis.
    /// </summary>
    public Vector2 Position => new Vector2(X, Y);

    /// <summary>
    /// The X position of the axis from -1 to 1.
    /// </summary>
    public float X { get; private set; }

    /// <summary>
    /// The Y position of the axis from -1 to 1.
    /// </summary>
    public float Y { get; private set; }

    /// <summary>
    /// The previous X position of the axis.
    /// </summary>
    public float LastX { get; private set; }

    /// <summary>
    /// The previous Y position of the axis.
    /// </summary>
    public float LastY { get; private set; }

    /// <summary>
    /// Check if the axis is currently forced.
    /// </summary>
    public bool ForcedInput { get; private set; }

    /// <summary>
    /// The the up Button for the Axis.
    /// </summary>
    public VirtualButton Up { get; private set; }

    /// <summary>
    /// Gets the right Button for the Axis.
    /// </summary>
    public VirtualButton Right { get; private set; }

    /// <summary>
    /// Gets the down Button for the Axis.
    /// </summary>
    public VirtualButton Down { get; private set; }

    /// <summary>
    /// The the left Button for the Axis.
    /// </summary>
    public VirtualButton Left { get; private set; }

    /// <summary>
    /// Check if the axis has any means of input currently registered to it.
    /// </summary>
    public bool HasInput =>
        ForcedInput
        || Keys.Count > 0
        || ControllerButtons.Count > 0
        || AxesX.Count > 0
        || AxesY.Count > 0;

    /// <summary>
    /// Check of the axis is completely neutral.
    /// </summary>
    public bool Neutral => X == 0 && Y == 0;

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new Axis.
    /// </summary>
    public VirtualAxis()
    {
        foreach (Direction dir in InputHelper.CardinalDirections)
        {
            Keys[dir] = new();
            ControllerButtons.Add(dir, new());
        }

        // Create buttons for Axis.
        Up = new VirtualButton();
        Right = new VirtualButton();
        Down = new VirtualButton();
        Left = new VirtualButton();
    }

    /// <summary>
    /// Create a new Axis using Keys.
    /// </summary>
    /// <param name="up">The Key for Up.</param>
    /// <param name="right">The Key for Right.</param>
    /// <param name="down">The Key for Down.</param>
    /// <param name="left">The Key for Left.</param>
    public VirtualAxis(Key up, Key right, Key down, Key left)
        : this()
    {
        AddKeys(up, right, down, left);
    }

    /// <summary>
    /// Create a new Axis using a controller axis.
    /// </summary>
    /// <param name="x">The ControllerAxis to use for X.</param>
    /// <param name="y">The ControllerAxis to use for Y.</param>
    /// <param name="controller">The controller id to use.</param>
    public VirtualAxis(ControllerAxis x, ControllerAxis y, int controller)
        : this()
    {
        AddControllerAxis(x, y, controller);
    }

    /// <summary>
    /// Create a new Axis using ControllerButtons.
    /// </summary>
    /// <param name="up">The ControllerButton for Up.</param>
    /// <param name="right">The ControllerButton for Right.</param>
    /// <param name="down">The ControllerButton for Down.</param>
    /// <param name="left">The ControllerButton for Left.</param>
    /// <param name="controller">The controller id to use.</param>
    public VirtualAxis(ControllerButton up, ControllerButton right, ControllerButton down, ControllerButton left, int controller)
        : this()
    {
        AddButton(up, Direction.Up, controller);
        AddButton(right, Direction.Right, controller);
        AddButton(down, Direction.Down, controller);
        AddButton(left, Direction.Left, controller);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Reset the Axis to report no input.
    /// </summary>
    public void Reset()
    {
        X = 0;
        Y = 0;
        LastX = 0;
        LastY = 0;
        Up.Reset();
        Right.Reset();
        Down.Reset();
        Left.Reset();
    }

    /// <summary>
    /// Clear all registered inputs for the Axis.
    /// </summary>
    public void Clear()
    {
        foreach (var dirList in Keys.Values) dirList.Clear();
        foreach (var dirList in ControllerButtons.Values) dirList.Clear();
        AxesX.Clear();
        AxesY.Clear();
    }

    /// <summary>
    /// Add a Controller axis.
    /// </summary>
    /// <param name="x">The x axis of the controller.</param>
    /// <param name="y">The y axis of the controller.</param>
    /// <param name="controller">The controller id.</param>
    /// <returns>The Axis.</returns>
    public VirtualAxis AddControllerAxis(ControllerAxis x, ControllerAxis y, int controller)
    {
        AxesX.Add((controller, x));
        AxesY.Add((controller, y));
        return this;
    }

    /// <summary>
    /// Add a Controller button.
    /// </summary>
    /// <param name="button">The controller button id.</param>
    /// <param name="direction">The direction this button should effect.</param>
    /// <param name="controller">The controller id.</param>
    /// <returns>The Axis.</returns>
    public VirtualAxis AddButton(ControllerButton button, Direction direction, int controller)
    {
        ControllerButtons[direction].Add((controller, button));
        return this;
    }

    /// <summary>
    /// Add a key.
    /// </summary>
    /// <param name="key">The keyboard key.</param>
    /// <param name="direction">The direction this key should effect.</param>
    /// <returns>The Axis.</returns>
    public VirtualAxis AddKey(Key key, Direction direction)
    {
        Keys[direction].Add(key);
        return this;
    }

    /// <summary>
    /// Add keys.
    /// </summary>
    /// <param name="up">The Key for Up.</param>
    /// <param name="right">The Key for Right.</param>
    /// <param name="down">The Key for Down.</param>
    /// <param name="left">The Key for Left.</param>
    /// <returns>The Axis.</returns>
    public VirtualAxis AddKeys(Key up, Key right, Key down, Key left)
    {
        AddKey(up, Direction.Up);
        AddKey(right, Direction.Right);
        AddKey(down, Direction.Down);
        AddKey(left, Direction.Left);
        return this;
    }

    /// <summary>
    /// Force the axis state.
    /// </summary>
    /// <param name="x">The forced x state.</param>
    /// <param name="y">The forced y state.</param>
    public void ForceState(float x, float y)
    {
        ForcedInput = true;
        X = x;
        Y = y;
    }

    /// <summary>
    /// Force the axis state.
    /// </summary>
    /// <param name="xy">The forced x and y state.</param>
    public void ForceState(Vector2 xy)
    {
        ForceState(xy.X, xy.Y);
    }

    /// <summary>
    /// Force the axis x state.
    /// </summary>
    /// <param name="x">The forced x state.</param>
    public void ForceStateX(float x)
    {
        ForceState(x, Y);
    }

    /// <summary>
    /// Force the axis y state.
    /// </summary>
    /// <param name="y">The forced y state.</param>
    public void ForceStateY(float y)
    {
        ForceState(X, y);
    }

    /// <summary>
    /// Relinquish control of the axis back to input.
    /// </summary>
    public void ReleaseState()
    {
        ForcedInput = false;
    }

    /// <summary>
    /// Update the VirtualAxis.
    /// </summary>
    public void Update()
    {
        LastX = X;
        LastY = Y;

        if (Locked) return;

        if (!Enabled)
        {
            X = 0;
            Y = 0;
            return;
        }

        if (ForcedInput)
        {
            return;
        }

        X = 0;
        Y = 0;

        foreach (var (id, axis) in AxesX)
        {
            if (InputManager.Controllers.TryGetValue(id, out var controller))
            {
                float value = controller.GetAxis(axis);
                if (value >= DeadZone || value <= -DeadZone)
                {
                    X += MapAxisValue(value);
                }
            }
        }

        foreach (var (id, axis) in AxesY)
        {
            if (InputManager.Controllers.TryGetValue(id, out var controller))
            {
                float value = controller.GetAxis(axis);
                if (value >= DeadZone || value <= -DeadZone)
                {
                    Y += MapAxisValue(value);
                }
            }
        }

        foreach (Key k in Keys[Direction.Up])
        {
            if (InputManager.KeyDown(k))
            {
                Y -= 1;
            }
        }
        foreach (Key k in Keys[Direction.Down])
        {
            if (InputManager.KeyDown(k))
            {
                Y += 1;
            }
        }
        foreach (Key k in Keys[Direction.Left])
        {
            if (InputManager.KeyDown(k))
            {
                X -= 1;
            }
        }
        foreach (Key k in Keys[Direction.Right])
        {
            if (InputManager.KeyDown(k))
            {
                X += 1;
            }
        }

        foreach (var (id, button) in ControllerButtons[Direction.Up])
        {
            if (InputManager.Controllers.TryGetValue(id, out var controller) && controller.ButtonDown(button))
            {
                Y -= 1;
            }
        }
        foreach (var (id, button) in ControllerButtons[Direction.Down])
        {
            if (InputManager.Controllers.TryGetValue(id, out var controller) && controller.ButtonDown(button))
            {
                Y += 1;
            }
        }
        foreach (var (id, button) in ControllerButtons[Direction.Left])
        {
            if (InputManager.Controllers.TryGetValue(id, out var controller) && controller.ButtonDown(button))
            {
                X -= 1;
            }
        }
        foreach (var (id, button) in ControllerButtons[Direction.Right])
        {
            if (InputManager.Controllers.TryGetValue(id, out var controller) && controller.ButtonDown(button))
            {
                X += 1;
            }
        }

        X = Util.Clamp(X, -1, 1);
        Y = Util.Clamp(Y, -1, 1);

        if (RoundInput)
        {
            X = MathF.Round(X, 2);
            Y = MathF.Round(Y, 2);
        }

        // Update the buttons. This makes it easy to read the Axis as buttons for Up, Right, Left, Down.
        Up.Update();
        Right.Update();
        Down.Update();
        Left.Update();

        Up.ForceState(Y < -0.5f);
        Right.ForceState(X > 0.5f);
        Down.ForceState(Y > 0.5f);
        Left.ForceState(X < -0.5f);
    }

    public override string ToString()
    {
        return "[VirtualAxis X: " + X.ToString() + " Y: " + Y.ToString() + "]";
    }

    #endregion

    private float MapAxisValue(float value)
    {
        if (RemapRange)
        {
            if (value > 0)
            {
                return Util.ScaleClamp(value, DeadZone, 1, 0, 1);
            }
            else
            {
                return Util.ScaleClamp(value, -1, -DeadZone, -1, 0);
            }
        }

        return value;
    }
}
