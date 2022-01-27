using System;
using System.Collections.Generic;
using System.Numerics;
using Lutra.Collision;
using Lutra.Graphics;

namespace Lutra.Examples.Microgames.LType;

public class PlayerShip : Entity
{
    private static readonly Vector2 weaponPosition = new Vector2(20, 5);
    private Image shipSprite;
    private Weapon currentWeapon = Weapon.Gauss;
    private float lastShotTimer = float.MaxValue;

    public readonly List<Bullet> ActiveBullets = new();
    public bool PlayerDead;
    public Action OnDeath;

    public PlayerShip()
    {
        shipSprite = new Image("LType/Ship.png");
        shipSprite.Layer = -10;
        shipSprite.CenterOrigin();
        AddGraphic(shipSprite);

        Collider = new PixelCollider("LType/Ship.png");
        Collider.CenterOrigin();
    }

    public override void Update()
    {
        if (!PlayerDead)
        {
            HandleMovement();
            CheckCollisions();
            HandleShooting();
        }
    }

    private void HandleMovement()
    {
        X += Scene.Game.DeltaTime * 60f;

        var movementAxis = GetScene<LTypeScene>().Controller.MovementAxis;

        var deltaX = 2f * movementAxis.X * Scene.Game.DeltaTime * 60f;
        var deltaY = 2f * movementAxis.Y * Scene.Game.DeltaTime * 60f;
        var camera = Scene.MainCamera;

        if (movementAxis.X < 0 && X - camera.Left < 50) deltaX = 0;
        if (movementAxis.X > 0 && camera.Right - X < 50) deltaX = 0;
        if (movementAxis.Y < 0 && Y - camera.Top < 40) deltaY = 0;
        if (movementAxis.Y > 0 && camera.Bottom - Y < 40) deltaY = 0;

        X += deltaX;
        Y += deltaY;
    }

    private void CheckCollisions()
    {
        var enemyCollisons = Collider.CollideEntities(X, Y, ColliderTags.Enemy);

        if (enemyCollisons.Count > 0)
        {
            OnDeath();
            PlayerDead = true;
        }
    }

    private void HandleShooting()
    {
        lastShotTimer += Scene.Game.DeltaTime;

        var shootButton = GetScene<LTypeScene>().Controller.ShootButton;
        var weaponData = WeaponData.Get(currentWeapon);

        if (shootButton.Down && lastShotTimer > weaponData.FiringSpeed)
        {
            lastShotTimer = 0f;
            var bullet = new Bullet(Position + weaponPosition, Vector2.UnitX, currentWeapon);
            ActiveBullets.Add(bullet);
            bullet.OnRemoved += () => ActiveBullets.Remove(bullet);
            Scene.Add(bullet);
        }
    }
}
