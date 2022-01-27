using Lutra.Audio.OpenAL;
using Lutra.Graphics;
using Lutra.Input;
using Lutra.Utility;

namespace Lutra.Examples.Microgames.LType;

// L-Type Engram α
public class LTypeScene : Scene
{
    public const float DEFAULT_VOLUME = 0.5f;

    public LTypeController Controller;
    public UI UI;
    public bool GameStarted;
    public bool GameMuted;
    public int Score;

    private Image background;
    private PlayerShip playerShip;
    private Music musicA;
    private float spawnTimer = 0f;

    public LTypeScene()
    {
        ManagesOwnCamera = true;
    }

    public override void Begin()
    {
        Music.GlobalVolume = DEFAULT_VOLUME;
        Sound.GlobalVolume = DEFAULT_VOLUME;

        UI = new UI();
        Add(UI);

        background = new Image("LType/BG.png") { RepeatX = true, RepeatFlipX = true };
        background.Layer = 10;
        AddGraphic(background);

        playerShip = new PlayerShip() { X = 100, Y = Game.Height / 2f };
        playerShip.OnDeath += () => UI.StartDeathScren(Score);
        Add(playerShip);

        musicA = new Music(AssetManager.LoadStream("LType/engramloopA.ogg"), true);
        musicA.Play();

        Controller = new LTypeController();
        InputManager.AddVirtualController(Controller);
    }

    public override void Update()
    {
        if (!playerShip.PlayerDead)
        {
            MainCamera.X += Game.DeltaTime * 60f;
        }

        HandleStart();
        HandleMute();
        HandleDebugDisplay();

        UI.UpdateScoreText(Score);

        if (GameStarted && !playerShip.PlayerDead)
        {
            var currentMusicPitch = musicA.Pitch;
            if (currentMusicPitch != 1f)
            {
                musicA.Pitch = Util.Approach(currentMusicPitch, 1f, Game.DeltaTime);
            }

            if (spawnTimer <= 0f)
            {
                var enemy = new Enemy(30)
                {
                    X = MainCamera.Right + 20,
                    Y = Rand.Float(50f, MainCamera.Bottom - 50f)
                };
                Add(enemy);
                spawnTimer += Rand.Float(2f, 4f);
            }
            spawnTimer -= Game.DeltaTime;
        }

        if (playerShip.PlayerDead)
        {
            musicA.Pitch = Util.Approach(musicA.Pitch, 0.37f, Game.DeltaTime);
        }
    }

    public override void End()
    {
        musicA.Dispose();
    }

    private void HandleStart()
    {
        if (!GameStarted && Controller.StartButton.Pressed)
        {
            GameStarted = true;
            UI.EndTitleScreen();

            foreach (var bullet in playerShip.ActiveBullets.ToArray())
            {
                bullet.RemoveSelf();
            }
        }

        if (playerShip.PlayerDead && Controller.StartButton.Pressed)
        {
            playerShip.PlayerDead = false;
            UI.EndDeathScreen();
        }
    }

    private void HandleMute()
    {
        if (Controller.MuteButton.Pressed)
        {
            if (GameMuted)
            {
                Music.GlobalVolume = DEFAULT_VOLUME;
                Sound.GlobalVolume = DEFAULT_VOLUME;
                musicA.Play();
                GameMuted = false;
            }
            else
            {
                musicA.Stop();
                Music.GlobalVolume = 0f;
                Sound.GlobalVolume = 0f;
                GameMuted = true;
            }
        }
    }

    private void HandleDebugDisplay()
    {
        if (InputManager.KeyPressed(Key.F1))
        {
            if (IMGUIDraw == DEBUG_Render)
            {
                IMGUIDraw = delegate { };
            }
            else
            {
                IMGUIDraw = DEBUG_Render;
            }
        }
    }

    private void DEBUG_Render()
    {
        foreach (var entity in Entities)
        {
            foreach (var collider in entity.Colliders)
            {
                collider.DebugRenderIMGUI(Color.Cyan);
            }
        }
    }
}
