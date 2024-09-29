using System.Collections.Generic;
using Lutra.Input;

public class LTypeController : VirtualController
{
    public readonly VirtualAxis MovementAxis;
    public readonly VirtualButton ShootButton;
    public readonly VirtualButton StartButton;
    public readonly VirtualButton MuteButton;

    public LTypeController(IEnumerable<Controller> controllers)
    {
        MovementAxis = VirtualAxis.CreateWASD();
        AddAxis("Movement", MovementAxis);

        ShootButton = new VirtualButton().AddKey(Key.Space);
        AddButton("Shoot", ShootButton);

        StartButton = new VirtualButton().AddKey(Key.Enter);
        AddButton("Start", StartButton);

        MuteButton = new VirtualButton().AddKey(Key.M);
        AddButton("Mute", MuteButton);

        foreach (var controller in controllers)
        {
            MovementAxis.AddControllerAxis(controller, ControllerAxis.LeftX, ControllerAxis.LeftY);
            ShootButton.AddControllerButton(controller, ControllerButton.A);
            StartButton.AddControllerButton(controller, ControllerButton.Start);
            MuteButton.AddControllerButton(controller, ControllerButton.Back);
        }
    }
}
