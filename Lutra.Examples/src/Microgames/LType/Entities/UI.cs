using Lutra.Graphics;
using Lutra.Utility;

namespace Lutra.Examples.Microgames.LType;

public class UI : Entity
{
    private const string CONTROLS_STRING = "Controls:\nMovement: WASD/Left Stick\nShoot: Space/A button\nStart: Enter/Start button\nMute: M/Back button";

    private RichText titleText;
    private RichText controlsText;
    private RichText deathText;
    private Text scoreLabelText;
    private Text scoreDisplayText;

    public void EndTitleScreen()
    {
        titleText.Visible = false;
        controlsText.Visible = false;
        scoreLabelText.Visible = true;
        scoreDisplayText.Visible = true;
    }

    public void StartDeathScren(int score)
    {
        deathText.String = $"{{waveAmpY:4}}{{waveRate:2}}Your ship was destroyed!{{waveAmpY:0}}\nScore: {score}\nPress Enter/Start button to try again.";
        deathText.Visible = true;
        scoreLabelText.Visible = false;
        scoreDisplayText.Visible = false;
    }

    public void EndDeathScreen()
    {
        deathText.Visible = false;
        scoreLabelText.Visible = true;
        scoreDisplayText.Visible = true;
    }

    public void UpdateScoreText(int score)
    {
        scoreDisplayText.SetString(score.ToString());
        scoreDisplayText.Transform.OriginX = scoreDisplayText.Width;
    }

    public override void Added()
    {
        var font = AssetManager.GetFont("monogram-extended.ttf", false);

        titleText = new RichText("{shadow:2}L-Type Engram Alpha", font, 48)
        {
            Layer = 0,
            X = Scene.Game.Width / 2,
            Y = Scene.Game.Height / 4,
            ScrollX = 0f,
            ScrollY = 0f,
            DefaultCharColor0 = Color.FromString("dfe0e8"),
            DefaultCharColor1 = Color.FromString("dfe0e8"),
            DefaultCharColor2 = Color.FromString("a3a7c2"),
            DefaultCharColor3 = Color.FromString("a3a7c2"),
            LineHeight = 0.8f
        };
        titleText.Refresh();
        titleText.CenterTextOrigin();

        controlsText = new RichText(CONTROLS_STRING, font, 32)
        {
            Layer = 0,
            X = Scene.Game.Width / 2,
            Y = Scene.Game.Height * (3f / 4f),
            ScrollX = 0f,
            ScrollY = 0f,
            DefaultShadowX = 1,
            DefaultShadowY = 1,
            Color = Color.FromString("a3a7c2"),
            TextAlign = TextAlign.Center,
            LineHeight = 0.8f
        };
        controlsText.Refresh();
        controlsText.CenterTextOrigin();

        deathText = new RichText($"Your ship was destroyed!\n\nPress Enter/Start button to try again.", font, 32)
        {
            Layer = -30,
            X = Scene.Game.Width / 2,
            Y = Scene.Game.Height / 2,
            ScrollX = 0f,
            ScrollY = 0f,
            DefaultShadowX = 1,
            DefaultShadowY = 1,
            Color = Color.FromString("dfe0e8"),
            TextAlign = TextAlign.Center
        };
        deathText.Refresh();
        deathText.CenterTextOrigin();

        scoreLabelText = new Text("SCORE", font, 16)
        {
            Layer = -30,
            X = Scene.Game.Width - 12,
            Y = 20,
            ScrollX = 0f,
            ScrollY = 0f,
            Color = Color.FromString("dfe0e8")
        };
        scoreLabelText.Transform.OriginX = scoreLabelText.Width;

        scoreDisplayText = new Text("0", font, 32)
        {
            Layer = -30,
            X = Scene.Game.Width - 11,
            Y = 40,
            ScrollX = 0f,
            ScrollY = 0f,
            Color = Color.FromString("dfe0e8")
        };
        scoreDisplayText.Transform.OriginX = scoreDisplayText.Width;

        // Don't show these yet
        deathText.Visible = false;
        scoreLabelText.Visible = false;
        scoreDisplayText.Visible = false;

        AddGraphic(titleText);
        AddGraphic(controlsText);
        AddGraphic(deathText);
        AddGraphic(scoreLabelText);
        AddGraphic(scoreDisplayText);
    }
}
