using System;
using System.Numerics;
using Lutra.Collision;
using Lutra.Graphics;

namespace Lutra.Examples.Microgames.LType;

public class Enemy : Entity
{
    private const float ALIVE_AFTER_HIT = 1f;

    private static readonly Vector2 weaponPosition = new Vector2(-10, 0);
    private Image enemySprite;
    private float aliveTimer = 0f;
    private float lastShotTimer = float.MaxValue;

    public float FiringSpeed = 2f;
    public int Health;
    public int PointsValue = 100;

    public Enemy(int startingHealth)
    {
        Health = startingHealth;

        enemySprite = new Image("LType/Enemy.png");
        enemySprite.Layer = -8;
        enemySprite.CenterOrigin();
        AddGraphic(enemySprite);

        Collider = new BoxCollider(20, 10, ColliderTags.Enemy);
        Collider.CenterOrigin();
    }

    public void DealDamage(int damage)
    {
        Health -= damage;
    }

    public override void Update()
    {
        aliveTimer += Scene.Game.DeltaTime;
        lastShotTimer += Scene.Game.DeltaTime;

        if (X + 10 < Scene.MainCamera.Left)
        {
            RemoveSelf();
        }

        if (Health <= 0)
        {
            GetScene<LTypeScene>().Score += PointsValue;
            RemoveSelf();
        }

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        Y += 0.2f * MathF.Sin(aliveTimer);
        X += 0.4f * MathF.Sin(2f * aliveTimer) - 0.1f;
    }

    private void HandleShooting()
    {
        if (lastShotTimer > FiringSpeed)
        {
            lastShotTimer = 0f;
            var bullet = new EnemyBullet(Position + weaponPosition);
            Scene.Add(bullet);
        }
    }
}
