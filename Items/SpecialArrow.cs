using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarterMod.Items
{
    public class SpecialArrow : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.FrostburnArrow}";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("FrostfireArrow"); // By default, capitalization in classnames will add spaces to the display name. You can customize the display name here by uncommenting this line.
            Tooltip.SetDefault("Frostfire Arrow for Frostfire Bow.");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 999; // How many items are needed in order to research duplication of this item in Journey mode.
        }

        public override void SetDefaults()
        {
            Item.width = 10; // The width of item hitbox
            Item.height = 28; // The height of item hitbox

            Item.damage = 20; // The damage for projectiles isn't actually 8, it actually is the damage combined with the projectile and the item together
            Item.DamageType = DamageClass.Ranged; // What type of damage does this ammo affect?

            Item.maxStack = 999; // The maximum number of items that can be contained within a single stack
            Item.consumable = true; // This marks the item as consumable, making it automatically be consumed when it's used as ammunition, or something else, if possible
            Item.knockBack = 5f; // Sets the item's knockback. Ammunition's knockback added together with weapon and projectiles.
            Item.value = Item.sellPrice(0, 1, 0, 0); // Item price in copper coins (can be converted with Item.sellPrice/Item.buyPrice)
            Item.rare = ItemRarityID.Yellow; // The color that the item's name will be in-game.
            Item.shoot = ModContent.ProjectileType<Projectiles.SpecialProjectile>(); // The projectile that weapons fire when using this item as ammunition.

            Item.ammo = Item.type; // Important. The first item in an ammo class sets the AmmoID to its type
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(999);
            recipe.AddIngredient(ItemID.IceBlock, 200);
            recipe.AddIngredient(ItemID.FrostburnArrow, 999);
            recipe.AddIngredient(ItemID.IceBlade);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }

    }
}
