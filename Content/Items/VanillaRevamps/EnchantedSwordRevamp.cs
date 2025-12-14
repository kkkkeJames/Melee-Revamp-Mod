using MeleeRevamp.Content.Core;
using MeleeRevamp.Content.Projectiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MeleeRevamp.Content.Items.VanillaRevamps
{
    public class EnchantedSwordRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.EnchantedSword;
        }
        public override void SetDefaults(Item item)
        {
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.Shoot;
            item.useTime = item.useAnimation = 30;
            item.channel = true;
            item.autoReuse = false;
        }
        public override void HoldItem(Item item, Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<EnchantedSwordSlash>()] < 1)
                Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<EnchantedSwordSlash>(), item.damage, item.knockBack, player.whoAmI);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class EnchantedSwordSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.EnchantedSword;
        public override void RegisterVariables()
        {
            SwordDust1 = DustID.Gold;
            SlashColor = new Color(33, 166, 255);
            MaxComboCount = 3;
        }
        public override void Appear()
        {
        }
        public override void Initialize()
        {
            base.Initialize();
        }
        public override void AIBefore()
        {
            base.AIBefore();
            Player player = Main.player[Projectile.owner];
            if (LeftClick)
            {
                switch (ComboCount)
                {
                    case 0:
                        ((EnchantedSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -1.9f, 1.9f, 0.8f, 0.4f, true, 6f);
                        break;
                    case 1:
                        ((EnchantedSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.8f, 1.8f, -1.7f, 0.8f, 0.4f, true, 6f);
                        break;
                    case 2:
                        ((EnchantedSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -2.5f, 2.3f, 0.8f, 0.4f, true, 6f, true, true, 0f, 1.2f);
                        break;
                }
            }
        }
    }
}