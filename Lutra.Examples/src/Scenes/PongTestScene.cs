using System.Numerics;
using ImGuiNET;
using Lutra.Collision;
using Lutra.Graphics;
using Lutra.Input;
using Lutra.Utility;

namespace Lutra.Examples
{
    public class PongTestScene : Scene
    {
        Entity Ball = null;
        Entity LeftPaddle = null;
        Entity RightPaddle = null;

        Text ScoreText = null;
        Text Instruction = null;

        PongController Controller;

        bool AIControlled = true;
        bool DrawColliders = false;
        Vector2 BallVelocity = new Vector2(3.0f, 3.0f);
        int ScoreLeft = 0;
        int ScoreRight = 0;

        public override void Begin() // Pong, the ur-test-game.
        {
            Controller = new PongController();
            InputManager.AddVirtualController(Controller);

            LeftPaddle = new Entity(30, Game.Height / 2);
            var leftPaddleImage = new Image("paddle.png");
            leftPaddleImage.Scale = 0.5f;
            leftPaddleImage.CenterOrigin();
            LeftPaddle.Graphic = leftPaddleImage;
            var leftPaddleCollider = new BoxCollider(25, 50, 1);
            leftPaddleCollider.CenterOrigin();
            LeftPaddle.Collider = leftPaddleCollider;

            RightPaddle = new Entity(Game.Width - 30, Game.Height / 2);
            var rightPaddleImage = new Image("paddle.png");
            rightPaddleImage.Scale = 0.5f;
            rightPaddleImage.CenterOrigin();
            RightPaddle.Graphic = rightPaddleImage;
            var rightPaddleCollider = new BoxCollider(25, 50, 1);
            rightPaddleCollider.CenterOrigin();
            RightPaddle.Collider = rightPaddleCollider;

            Ball = new Entity(Game.Width / 2, Game.Height / 2);
            var ballImage = new Image("Snake/SnakeHead.png");
            ballImage.CenterOrigin();
            Ball.Graphic = ballImage;
            var ballCollider = new CircleCollider(24, 1);
            ballCollider.CenterOrigin();
            Ball.Collider = ballCollider;

            Add(LeftPaddle);
            Add(RightPaddle);
            Add(Ball);

            var testFont = AssetManager.GetFont("monogram-extended.ttf", false);
            Instruction = new Text("Use menu to toggle AI control of paddle.", testFont, 16);
            Instruction.X = 20;
            Instruction.Y = Game.Height - 18;
            AddGraphic(Instruction);

            ScoreText = new Text("0 : 0", testFont, 32);
            ScoreText.X = Game.Width / 2;
            ScoreText.Y = 50;
            ScoreText.CenterOrigin();
            AddGraphic(ScoreText);

            IMGUIDraw += PongControlUI;
        }

        public override void Update()
        {
            // Update Ball
            Ball.X += BallVelocity.X;
            Ball.Graphic.Angle += BallVelocity.X;
            Ball.Y += BallVelocity.Y;

            // Scoring
            if (Ball.X > Game.Width || Ball.X < 0)
            {
                BallVelocity.X *= -1.0f;

                if (Ball.X > Game.Width / 2)
                {
                    AddScore(true);
                }
                else
                {
                    AddScore(false);
                }

                Ball.X = Game.Width / 2;
                Ball.Y = Game.Height / 2;
            }

            // Wall-Bouncing
            if (Ball.Y > Game.Height || Ball.Y < 0)
            {
                BallVelocity.Y *= -1.0f;
            }

            // Update Paddles
            if (AIControlled)
            {
                // Right Side
                if (Ball.X > Game.Width / 2)
                {
                    RightPaddle.Y = Util.Approach(RightPaddle.Y, Ball.Y, 3);
                }
                else // Left Side
                {
                    LeftPaddle.Y = Util.Approach(LeftPaddle.Y, Ball.Y, 3);
                }
            }
            else
            {
                if (Controller.LeftPaddleKeys.Up.Down && LeftPaddle.Y > 50)
                {
                    LeftPaddle.Y -= 3.0f;
                }
                if (Controller.LeftPaddleKeys.Down.Down && LeftPaddle.Y < Game.Height - 50)
                {
                    LeftPaddle.Y += 3.0f;
                }
                if (Controller.RightPaddleKeys.Up.Down && RightPaddle.Y > 50)
                {
                    RightPaddle.Y -= 3.0f;
                }
                if (Controller.RightPaddleKeys.Down.Down && RightPaddle.Y < Game.Height - 50)
                {
                    RightPaddle.Y += 3.0f;
                }
            }

            // Ball-Paddle Collisions
            if (Ball.Collider.Collide(Ball.X, Ball.Y, 1) != null)
            {
                BallVelocity.X *= -1.0f;
            }
        }

        public class PongController : VirtualController
        {
            public VirtualAxis LeftPaddleKeys;
            public VirtualAxis RightPaddleKeys;

            public PongController()
            {
                LeftPaddleKeys = VirtualAxis.CreateWASD();
                RightPaddleKeys = VirtualAxis.CreateArrowKeys();

                AddAxis("LeftPaddleKeys", LeftPaddleKeys);
                AddAxis("RightPaddleKeys", RightPaddleKeys);
            }
        }

        private void AddScore(bool rightLeft)
        {
            if (rightLeft)
            {
                ScoreLeft += 1;
            }
            else
            {
                ScoreRight += 1;
            }

            ScoreText.String = $"{ScoreLeft} : {ScoreRight}";
        }

        private void PongControlUI()
        {
            ImGui.Begin("Pong Controls");

            ImGui.Checkbox("AI Controlled", ref AIControlled);

            if (ImGui.Button("Reset Score"))
            {
                ScoreLeft = 0;
                ScoreRight = 0;
                ScoreText.String = $"{ScoreLeft} : {ScoreRight}";
            }

            if (ImGui.Button("Reset Ball"))
            {
                Ball.X = Game.Width / 2;
                Ball.Y = Game.Height / 2;
            }

            if (ImGui.Button("Ball Speed Up"))
            {
                BallVelocity *= 1.1f;
            }

            if (ImGui.Button("Ball Speed Down"))
            {
                BallVelocity *= 0.9f;
            }

            ImGui.Checkbox("Draw Colliders", ref DrawColliders);

            if (DrawColliders)
            {
                LeftPaddle.Collider.DebugRenderIMGUI(Color.Cyan);
                RightPaddle.Collider.DebugRenderIMGUI(Color.Magenta);
                Ball.Collider.DebugRenderIMGUI(Color.Yellow);
            }

            ImGui.End();
        }
    }
}
