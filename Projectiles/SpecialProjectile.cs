using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarterMod.Projectiles
{
    public class SpecialProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Special Arrow"); // Name of the projectile. It can be appear in chat

            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = false; // Make the cultist resistant to this projectile, as it's resistant to all homing projectiles.
        }

        public override void SetDefaults()
        {
            Projectile.width = 8; // The width of projectile hitbox
            Projectile.height = 8; // The height of projectile hitbox
            Projectile.light = 1f; // How much light emit around the projectile

            Projectile.aiStyle = 1; // The ai style of the projectile (0 means custom AI). For more please reference the source code of Terraria
            //AIType = 0;

            Projectile.DamageType = DamageClass.Ranged; // What type of damage does this projectile affect?
            Projectile.friendly = true; // Can the projectile deal damage to enemies?
            Projectile.hostile = false; // Can the projectile deal damage to the player?

            Projectile.ignoreWater = false; // Does the projectile's speed be influenced by water?
            Projectile.tileCollide = true; // Can the projectile collide with tiles?
            Projectile.timeLeft = 600; // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
        }


    }
}
