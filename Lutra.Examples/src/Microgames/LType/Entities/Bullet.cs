using System.Numerics;
using Lutra.Collision;
using Lutra.Graphics;

namespace Lutra.Examples.Microgames.LType;

public class Bullet : Entity
{
    private const float ALIVE_AFTER_HIT = 1f;

    public ImageSet Sprite;
    public Vector2 Direction;
    public WeaponData WeaponData;

    private Collider hitCollider;
    private Collider rangeCollider;

    private float hitTimer = float.MaxValue;

    public Bullet(Vector2 startPosition, Vector2 direction, Weapon weaponType)
    {
        Position = startPosition;
        Direction = direction;
        WeaponData = WeaponData.Get(weaponType);

        Sprite = new ImageSet("LType/Bullets.png", 16, 16)
        {
            Layer = -20,
            Frame = (int)weaponType
        };
        Sprite.CenterOrigin();
        AddGraphic(Sprite);

        hitCollider = new BoxCollider(5, 5, ColliderTags.PlayerBullet);
        hitCollider.CenterOrigin();
        hitCollider.OriginX -= 5;
        AddCollider(hitCollider);

        rangeCollider = new CircleCollider(WeaponData.Range);
        rangeCollider.CenterOrigin();
        AddCollider(rangeCollider);
    }

    public override void Update()
    {
        if (!Scene.MainCamera.Bounds.Contains(Position))
        {
            RemoveSelf();
        }

        hitTimer -= Scene.Game.DeltaTime;
        if (hitTimer < 0) RemoveSelf();

        if (hitTimer > ALIVE_AFTER_HIT)
        {
            var magnitude = 4f * Scene.Game.DeltaTime * 60f;
            Position += Direction * magnitude;

            CheckCollisions();
        }
    }

    private void CheckCollisions()
    {
        if (hitCollider.Overlap(X, Y, ColliderTags.Enemy))
        {
            // Explosion frames are on the second row
            Sprite.Frame += Sprite.Columns;
            hitTimer = ALIVE_AFTER_HIT;

            var enemyHits = rangeCollider.CollideEntities<Enemy>(X, Y, ColliderTags.Enemy);

            foreach (var enemy in enemyHits)
            {
                enemy.DealDamage(WeaponData.Damage);
            }
        }
    }
}
