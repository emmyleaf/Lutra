using System.Collections.Generic;
using Lutra.Utility;

namespace Lutra.Input;

/// <summary>
/// Component representing a group of Button and Axis classes. 
/// The controller updates all buttons and axes once it is added to InputManager.
/// </summary>
public class VirtualController
{
    // TODO: Just have InputManager create these, so we don't have to add them manually?
    #region Private Fields

    private readonly Dictionary<string, VirtualButton> buttons = [];
    private readonly Dictionary<string, VirtualAxis> axes = [];

    #endregion

    #region Public Fields

    /// <summary>
    /// Determines if the controller is enabled. If not, all buttons return false, and all axes return 0, 0.
    /// </summary>
    public bool Enabled = true;

    #endregion

    public VirtualButton GetButton(Enum name) => buttons.GetValueOrDefault(Util.EnumValueToString(name));
    public VirtualButton GetButton(string name) => buttons.GetValueOrDefault(name);

    public VirtualAxis GetAxis(Enum name) => axes.GetValueOrDefault(Util.EnumValueToString(name));
    public VirtualAxis GetAxis(string name) => axes.GetValueOrDefault(name);

    public void AddButton(Enum name, VirtualButton b = null)
    {
        AddButton(Util.EnumValueToString(name), b);
    }

    public void AddButton(string name, VirtualButton b = null)
    {
        buttons.Add(name, b ?? new VirtualButton());
    }

    public void AddAxis(Enum name, VirtualAxis a = null)
    {
        AddAxis(Util.EnumValueToString(name), a);
    }

    public void AddAxis(string name, VirtualAxis a = null)
    {
        axes.Add(name, a ?? new VirtualAxis());
    }

    public void Clear()
    {
        buttons.Clear();
        axes.Clear();
    }

    internal void Update()
    {
        foreach (var button in buttons.Values)
        {
            button.Enabled = Enabled;
            button.Update();
        }

        foreach (var axis in axes.Values)
        {
            axis.Enabled = Enabled;
            axis.Update();
        }
    }
}
