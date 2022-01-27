using Lutra.Examples;
using Lutra.Utility;

// As an example, make sure we preload the test font we use in this project
AssetManager.PreloadAssets += () =>
{
    AssetManager.GetFont("monogram-extended.ttf", false);
};

// GameOptions allows us to set some of the initial state of the Game.
Game game = new Game(new GameOptions()
{
    Title = "Lutra Examples",
    Width = 640,
    Height = 360,
    ScaleXY = 2f,
    TargetFrameRate = 60.0
});

game.Start(new LaunchMenuScene());
