using Lutra.Input;

public class LTypeController : VirtualController
{
    public readonly VirtualAxis MovementAxis;
    public readonly VirtualButton ShootButton;
    public readonly VirtualButton StartButton;
    public readonly VirtualButton MuteButton;

    public LTypeController()
    {
        MovementAxis = VirtualAxis.CreateWASD()
            .AddControllerAxis(ControllerAxis.LeftX, ControllerAxis.LeftY, 0);
        AddAxis("Movement", MovementAxis);

        ShootButton = new VirtualButton()
            .AddKey(Key.Space)
            .AddControllerButton(ControllerButton.A, 0);
        AddButton("Shoot", ShootButton);

        StartButton = new VirtualButton()
            .AddKey(Key.Enter)
            .AddControllerButton(ControllerButton.Start, 0);
        AddButton("Start", StartButton);

        MuteButton = new VirtualButton()
            .AddKey(Key.M)
            .AddControllerButton(ControllerButton.Back, 0);
        AddButton("Mute", MuteButton);
    }
}
