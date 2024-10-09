using System;
using Lutra.Examples.Microgames.Snake.Entities;
using Lutra.Graphics;
using Lutra.Input;
using Lutra.Utility;

namespace Lutra.Examples.Microgames.Snake
{
    public enum SnakeGameState
    {
        NewGame,
        Playing,
        GameOver
    }

    public class SnakeScene : Scene
    {
        public static readonly int GridSize = 32;
        private SnakeGameState GameState;

        private readonly Entity StartNewGameHUD = new();
        private readonly Entity PlayingHUD = new();
        private RichText PlayingScore;
        private readonly Entity GameOverHUD = new();
        private RichText GameOverScore;

        private int CurrentScore = 0;

        private Entities.Snake Snake;
        private Entities.Fish LastFishSpawned;

        public override void Begin()
        {
            base.Begin();
            BuildMap();

            var textFont = AssetManager.GetFont("monogram-extended.ttf", false);

            var newGameTextA = new RichText("Snake!", textFont, 32);
            newGameTextA.CenterTextOrigin();
            newGameTextA.X = Game.Instance.Width / 2;
            newGameTextA.Y = 40;
            var newGameTextB = new RichText("Press Space To Start\nArrow Keys to Move", textFont, 16);
            newGameTextB.CenterTextOrigin();
            newGameTextB.X = Game.Instance.Width / 2;
            newGameTextB.Y = 66;
            StartNewGameHUD.AddGraphic(newGameTextA);
            StartNewGameHUD.AddGraphic(newGameTextB);
            StartNewGameHUD.X = 0;
            StartNewGameHUD.Y = 0;
            Add(StartNewGameHUD);

            PlayingScore = new RichText("Score: 0", textFont, 32);
            PlayingScore.X = 0;
            PlayingScore.Y = Game.Instance.Height - 40;
            PlayingScore.Layer = -20;
            PlayingHUD.AddGraphic(PlayingScore);
            Add(PlayingHUD);
            PlayingHUD.Visible = false;

            Add(GameOverHUD);
            GameOverScore = new RichText("Oops! Your Score: 0\nPress Space to Try Again", textFont, 32);
            GameOverScore.X = Game.Instance.Width / 2;
            GameOverScore.Y = Game.Instance.Height / 2;
            GameOverScore.Layer = -20;
            GameOverHUD.AddGraphic(GameOverScore);
            GameOverHUD.Visible = false;
        }

        public override void Update()
        {
            base.Update();

            switch (GameState)
            {
                case SnakeGameState.NewGame:
                    {
                        if (InputManager.KeyPressed(Key.Space))
                        {
                            CurrentScore = 0;
                            PlayingScore.String = $"Score: {CurrentScore}";
                            GameState = SnakeGameState.Playing;
                            StartNewGameHUD.Visible = false;
                            PlayingHUD.Visible = true;

                            Snake = new Entities.Snake(10, 5);
                            Add(Snake);

                            LastFishSpawned = SpawnFish();
                        }
                        break;
                    }
                case SnakeGameState.Playing:
                    {
                        if (Snake.TouchingAFish())
                        {
                            LastFishSpawned.RemoveSelf();
                            CurrentScore++;
                            PlayingScore.DefaultShakeY = 10;
                            PlayingScore.String = $"Score: {CurrentScore}";
                            PlayingScore.Refresh();
                            LastFishSpawned = SpawnFish();
                            Snake.AddBodySegment();
                        }

                        if (PlayingScore.DefaultShakeY > 0)
                        {
                            PlayingScore.DefaultShakeY--;
                            PlayingScore.Refresh();
                        }

                        if (!Snake.Alive)
                        {
                            // Snake died? Game over!
                            GameState = SnakeGameState.GameOver;
                            PlayingHUD.Visible = false;
                            GameOverHUD.Visible = true;
                            GameOverScore.String = $"Oops! Your Score: {CurrentScore}\nPress Space to Try Again!";
                            GameOverScore.CenterTextOrigin();

                            if (LastFishSpawned != null)
                            {
                                LastFishSpawned.RemoveSelf();
                                LastFishSpawned = null;
                            }
                        }
                        break;
                    }
                case SnakeGameState.GameOver:
                    {
                        if (InputManager.KeyPressed(Key.Space))
                        {

                            Snake.RemoveSelf();
                            Snake = null;
                            GameState = SnakeGameState.NewGame;
                            GameOverHUD.Visible = false;
                            StartNewGameHUD.Visible = true;
                        }

                        break;
                    }
            }
        }

        private Fish SpawnFish()
        {
            var fish = new Fish(Rand.Int(1, Game.Width / GridSize), Rand.Int(1, Game.Height / GridSize));
            Add(fish);

            return fish;
        }

        private void BuildMap()
        {
            for (int x = 0; x < (int)MathF.Ceiling(Game.Width / GridSize) + 1; x++)
            {
                for (int y = 0; y < (int)MathF.Ceiling(Game.Height / GridSize) + 1; y++)
                {
                    if (x == 0 || y == 0 || x == (int)MathF.Ceiling(Game.Width / GridSize) || y == (int)MathF.Ceiling(Game.Height / GridSize))
                    {
                        var wall = new Wall(x, y);
                        Add(wall);
                    }
                }
            }
        }
    }
}
