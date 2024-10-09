using Lutra.Collision;
using Lutra.Graphics;

namespace Lutra.Examples.Microgames.Snake.Entities
{
    public class Fish : Entity
    {
        public Fish(int tileX, int tileY) : base(tileX * SnakeScene.GridSize, tileY * SnakeScene.GridSize)
        {
            AddGraphic(new Image("Snake/Fish.png"));
            Graphic.CenterOrigin();

            AddCollider(new BoxCollider(SnakeScene.GridSize, SnakeScene.GridSize, 2));
            Collider.CenterOrigin();
        }
    }
}
