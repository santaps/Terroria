﻿using StarterMod.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarterMod.Items
{
    public class CursedFrostfireBow : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.ShadowFlameBow}";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("TutorialSword"); // By default, capitalization in classnames will add spaces to the display name. You can customize the display name here by uncommenting this line.
            Tooltip.SetDefault("Very cursed Frostfire Bow. Use at your own risk. 75% chance to not use ammo.");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1; // How many items are needed in order to research duplication of this item in Journey mode.
        }

        public override void SetDefaults()
        {
            // Common attributes
            Item.width = 12;
            Item.height = 28;
            //Item.scale = 0.75f;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(1, 50, 0, 0); // vendor value

            // Animation properties
            Item.useTime = 15; // The item's use time in ticks (60 ticks == 1 second.)
            Item.useAnimation = 15; // The length of the item's use animation in ticks (60 ticks == 1 second.)
            Item.useStyle = ItemUseStyleID.Shoot; // How you use the item (swinging, holding out, etc.)
            Item.autoReuse = true;

            // Sound properties
            Item.UseSound = SoundID.Item5;

            // Weapon stats
            Item.damage = 65;
            Item.crit = 11;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 5f;
            Item.noMelee = true; // no contact damage with animation

            // Shoot / Bow properties
            Item.shoot = ProjectileID.PurificationPowder; // For some reason, all the guns in the vanilla source have this.
            Item.shootSpeed = 15f; // The speed of the projectile (measured in pixels per frame.)
            Item.useAmmo = AmmoID.Arrow; // The "ammo Id" of the ammo item that this weapon uses. Ammo IDs are magic numbers that usually correspond to the item id of one item that most commonly represent the ammo type.
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            // CanConsumeAmmo allows ammo to be conserved or consumed depending on various conditions.
            // (Its sister hook, CanBeConsumedAsAmmo, is called on the ammo, and has the same function.)
            // This returns true by default; returning false for any reason will prevent ammo consumption.
            // Note that returning true does NOT allow you to force ammo consumption; this currently requires use of IL editing or detours.
            if (player.ItemUsesThisAnimation == 0)
                return Main.rand.NextFloat() >= 0.75f; // 75% chance not to use ammo

            return true;
        }

        // Changes normal wooden arrows into cursed frostfire arrows
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (type == ProjectileID.WoodenArrowFriendly) {
                type = ModContent.ProjectileType<CursedFrostfireProjectile>();
                damage += 25; // Hardcoded for now...
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ShadowFlameBow);
            recipe.AddIngredient(ModContent.ItemType<SpecialBow>());
            recipe.AddIngredient(ItemID.CursedArrow, 999);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }


        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            const int numberProjectiles = 5;

            // Current arrow angles: 0, +5, -10, +15, -20
            for (int i = 0, y = 1; i < numberProjectiles; i++, y *= -1)
            {
                Vector2 newVelocity = velocity.RotatedBy(MathHelper.ToRadians(5*i*y));

                Projectile.NewProjectileDirect(source, position, newVelocity, type, damage, knockback, player.whoAmI);
            }
            return false;
        }

        // What if I wanted multiple projectiles in a even spread? (Vampire Knives)
        // Even Arc style: Multiple Projectile, Even Spread
        /*public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            float numberProjectiles = 3;
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
