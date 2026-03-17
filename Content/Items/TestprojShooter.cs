using MeleeRevamp.Content.Projectiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MeleeRevamp.Content.Items
{
    public class TestprojShooter : ModItem
    {
        public override string Texture => "Terraria/Images/Extra_195";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.useTime = Item.useAnimation = 30;
            Item.shoot = ModContent.ProjectileType<SwordTrail>();
            Item.autoReuse = false;
            Item.stack = 1;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noUseGraphic = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var shootproj = Projectile.NewProjectileDirect(source, player.Center + new Vector2(player.direction * 20, 0), new Vector2(player.direction * 30f, 0), type, 0, 0, player.whoAmI);
            return false;
        }
    }
}
