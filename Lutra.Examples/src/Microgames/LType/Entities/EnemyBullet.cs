using System.Numerics;
using Lutra.Graphics;

namespace Lutra.Examples.Microgames.LType;

public class EnemyBullet : Entity
{
    private static readonly Color Color = Color.FromString("ad2f45");
    public ImageSet Sprite;

    public EnemyBullet(Vector2 position)
    {
        Position = position;

        Sprite = new ImageSet("LType/Bullets.png", 16, 16)
        {
            Layer = -20,
            Frame = 0,
            Color = Color,
            FlippedX = true
        };
        Sprite.CenterOrigin();
        AddGraphic(Sprite);
    }

    public override void Update()
    {
        if (!Scene.MainCamera.Bounds.Contains(Position))
        {
            RemoveSelf();
        }

        X -= Scene.Game.DeltaTime * 60f;
    }
}
