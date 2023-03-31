using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarterMod.Projectiles
{
    public class CursedFrostfireProjectile : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.FrostburnArrow}";
        public int bounce = 3;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cursed Frostfire Arrow"); // Name of the projectile. It can be appear in chat

            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = false; // Make the cultist resistant to this projectile, as it's resistant to all homing projectiles.
        }

        public override void SetDefaults()
        {
            Projectile.width = 8; // The width of projectile hitbox
            Projectile.height = 8; // The height of projectile hitbox
            Projectile.light = 2f; // How much light emit around the projectile

            Projectile.aiStyle = 1; // The ai style of the projectile (0 means custom AI). For more please reference the source code of Terraria
            AIType = ProjectileID.WoodenArrowFriendly;

            Projectile.DamageType = DamageClass.Ranged; // What type of damage does this projectile affect?
            Projectile.friendly = true; // Can the projectile deal damage to enemies?
            Projectile.hostile = false; // Can the projectile deal damage to the player?

            Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
            Projectile.tileCollide = true; // Can the projectile collide with tiles?
            Projectile.timeLeft = 600; // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
            Projectile.extraUpdates = 1; // Set to above 0 if you want the projectile to update multiple time in a frame

            Projectile.penetrate = 5; // How many monsters the projectile can penetrate.
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Projectile can bounce twice from walls
            bounce--;
            if (bounce <= 0)
            {
                Projectile.Kill();
            }
            else
            {
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
                SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

                // If the projectile hits the left or right side of the tile, reverse the X velocity
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                {
                    Projectile.velocity.X = -oldVelocity.X;
                }

                // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                {
                    Projectile.velocity.Y = -oldVelocity.Y;
                }
            }
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Frostburn, 300, false);
            target.AddBuff(BuffID.OnFire, 300, false);
            target.AddBuff(BuffID.CursedInferno, 300, false);
        }


    }
}
