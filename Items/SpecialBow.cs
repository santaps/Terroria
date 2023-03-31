using StarterMod.Projectiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarterMod.Items
{
    public class SpecialBow : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.IceBow}";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("FrostfireBow"); // By default, capitalization in classnames will add spaces to the display name. You can customize the display name here by uncommenting this line.
            Tooltip.SetDefault("Extra icy magic bow. 50% chance not to use ammo.");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1; // How many items are needed in order to research duplication of this item in Journey mode.
        }

        public override void SetDefaults()
        {
            // Common attributes
            Item.width = 12;
            Item.height = 28;
            //Item.scale = 0.75f;
            Item.rare = ItemRarityID.Cyan;
            Item.value = 1250125; // vendor value

            // Animation properties
            Item.useTime = 20; // The item's use time in ticks (60 ticks == 1 second.)
            Item.useAnimation = 20; // The length of the item's use animation in ticks (60 ticks == 1 second.)
            Item.useStyle = ItemUseStyleID.Shoot; // How you use the item (swinging, holding out, etc.)
            Item.autoReuse = true;

            // Sound properties
            Item.UseSound = SoundID.Item5;

            // Weapon stats
            Item.damage = 50;
            Item.crit = 6;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 5f;
            Item.noMelee = true; // no contact damage with animation

            // Shoot / Bow properties
            Item.shoot = ProjectileID.PurificationPowder; // For some reason, all the guns in the vanilla source have this.
            Item.shootSpeed = 10f; // The speed of the projectile (measured in pixels per frame.)
            Item.useAmmo = AmmoID.Arrow; // The "ammo Id" of the ammo item that this weapon uses. Ammo IDs are magic numbers that usually correspond to the item id of one item that most commonly represent the ammo type.
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            // CanConsumeAmmo allows ammo to be conserved or consumed depending on various conditions.
            // (Its sister hook, CanBeConsumedAsAmmo, is called on the ammo, and has the same function.)
            // This returns true by default; returning false for any reason will prevent ammo consumption.
            // Note that returning true does NOT allow you to force ammo consumption; this currently requires use of IL editing or detours.
            if (player.ItemUsesThisAnimation == 0)
                return Main.rand.NextFloat() >= 0.5f; // 50% chance not to use ammo
 
            return true;
        }

        // Changes normal wooden arrows into frostburn arrows
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) 
        {
            if (type == ProjectileID.WoodenArrowFriendly) {
                type = ModContent.ProjectileType<SpecialProjectile>();
                damage += 20; // Hardcode to test for now
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            const int numberProjectiles = 3;
            const float angle = 5f;
            int angleOffset = numberProjectiles / 2;
            float startingAngleOffset = -angle * angleOffset;
            //Vector2 offset = new Vector2(velocity.X, velocity.Y);
            //Vector2 offset = velocity * 3;
            //position += offset;

            // Even spread with odd number of arrows
            for (int i = 0; i < numberProjectiles; i++)
            {
                Vector2 newVelocity = velocity.RotatedBy(MathHelper.ToRadians(startingAngleOffset + angle * i));

                Projectile.NewProjectileDirect(source, position, newVelocity, type, damage, knockback, player.whoAmI);
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IceBlock, 100);
            recipe.AddIngredient(ItemID.IceBow, 1);
            recipe.AddIngredient(ItemID.FrostburnArrow, 999);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }


        // This method lets you adjust position of the gun in the player's hands. Play with these values until it looks good with your graphics.
        /*public override Vector2? HoldoutOffset()
        {
            return new Vector2(2f, -2f);
        }*/

        // What if I wanted multiple projectiles in a even spread? (Vampire Knives)
        // Even Arc style: Multiple Projectile, Even Spread
        /*public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            float numberProjectiles = 3 + Main.rand.Next(3); // 3, 4, or 5 shots
            float rotation = MathHelper.ToRadians(45);

            position += Vector2.Normalize(velocity) * 45f;

            for (int i = 0; i < numberProjectiles; i++) {
                Vector2 perturbedSpeed = velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1))) * .2f; // Watch out for dividing by 0 if there is only 1 projectile.
                Projectile.NewProjectile(source, position, perturbedSpeed, type, damage, knockback, player.whoAmI);
            }

            return false; // return false to stop vanilla from calling Projectile.NewProjectile.
        }*/
    }
}
