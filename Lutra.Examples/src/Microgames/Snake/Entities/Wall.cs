using Lutra.Collision;
using Lutra.Graphics;

namespace Lutra.Examples.Microgames.Snake.Entities
{
    public class Wall : Entity
    {
        public Wall(int tileX, int tileY) : base(tileX * SnakeScene.GridSize, tileY * SnakeScene.GridSize)
        {
            AddGraphic(new Image("Snake/Wall.png"));
            Graphic.Color = Color.Grey;
            Graphic.CenterOrigin();

            AddCollider(new BoxCollider(SnakeScene.GridSize, SnakeScene.GridSize, 1));
            Collider.CenterOrigin();
        }
    }
}
