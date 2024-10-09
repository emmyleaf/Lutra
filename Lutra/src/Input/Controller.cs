using System.Collections.Generic;
using Lutra.Utility.Collections;
using SDL;

namespace Lutra.Input;

/// <summary>
/// Class representing a game controller input from SDL.
/// Each controller is managed and updated by InputManager.
/// </summary>
public class Controller
{
    internal readonly SDL_Gamepad SdlGamepad;
    internal readonly SDL_JoystickID SdlJoystickID;

    internal HashSet<ControllerButton> PrevButtonsDown;
    internal HashSet<ControllerButton> ButtonsDown;
    internal readonly Dictionary<ControllerAxis, float> Axes;
    internal ControllerButton LastButtonDown;

    internal Controller(SDL_Gamepad gamepad, SDL_JoystickID joystickID)
    {
        SdlGamepad = gamepad;
        SdlJoystickID = joystickID;
        PrevButtonsDown = [];
        ButtonsDown = [];
        Axes = [];

        foreach (var axis in Enum.GetValues<ControllerAxis>())
        {
            Axes.Add(axis, 0.0f);
        }
    }

    internal void StartNewFrame()
    {
        PrevButtonsDown = ButtonsDown.Clone();
    }

    internal void ButtonDownEvent(ControllerButton button)
    {
        ButtonsDown.Add(button);
        LastButtonDown = button;
    }

    internal void ButtonUpEvent(ControllerButton button)
    {
        ButtonsDown.Remove(button);
    }

    internal void AxisEvent(ControllerAxis axis, float value)
    {
        Axes[axis] = value;
    }

    #region Public Methods

    public int ID => (int)SdlJoystickID;

    /// <summary>
    /// Check if any controller button was pressed.
    /// </summary>
    /// <returns>True if any button was pressed.</returns>
    public bool AnyButtonPressed()
    {
        foreach (var button in ButtonsDown)
        {
            if (!PrevButtonsDown.Contains(button)) return true;
        }
        return false;
    }

    /// <summary>
    /// Check if a controller button was pressed.
    /// </summary>
    /// <param name="button">The button to check.</param>
    /// <returns>True if the button was pressed.</returns>
    public bool ButtonPressed(ControllerButton button)
    {
        return ButtonsDown.Contains(button) && !PrevButtonsDown.Contains(button);
    }

    /// <summary>
    /// Check if the controller button was released.
    /// </summary>
    /// <param name="button">The button to check.</param>
    /// <returns>True if the button was released.</returns>
    public bool ButtonReleased(ControllerButton button)
    {
        return !ButtonsDown.Contains(button) && PrevButtonsDown.Contains(button);
    }

    /// <summary>
    /// Check if the controller button is down.
    /// </summary>
    /// <param name="button">The button to check.</param>
    /// <returns>True if the button is down.</returns>
    public bool ButtonDown(ControllerButton button)
    {
        return ButtonsDown.Contains(button);
    }

    /// <summary>
    /// Check if the controller button is up.
    /// </summary>
    /// <param name="button">The button to check.</param>
    /// <returns>True if the button is up.</returns>
    public bool ButtonUp(ControllerButton button)
    {
        return !ButtonsDown.Contains(button);
    }

    /// <summary>
    /// Get the value of a controller axis from -1 to 1.
    /// </summary>
    /// <param name="axis">The axis to check.</param>
    /// <returns>The axis value from -1 to 1.</returns>
    public float GetAxis(ControllerAxis axis)
    {
        return Axes[axis];
    }

    #endregion
}
