namespace Lutra.Utility.Debugging;

public static class DebugCommands
{
    #region Window Commands

    [DebugCommand(alias: "setscale", help: "Set the display scale of the game.", group: "window")]
    public static void CmdSetScale(float scaleXY)
    {
        Game.Instance.Window.SetScale(scaleXY);
    }

    #endregion
}
