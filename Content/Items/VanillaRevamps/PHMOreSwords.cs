using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MeleeRevamp.Content.Projectiles;

namespace MeleeRevamp.Content.Items.VanillaRevamps
{
    public class CopperBroadSwordRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.CopperBroadsword;
        }
        public override void SetDefaults(Item item)
        {
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.Shoot;
            item.shoot = ModContent.ProjectileType<CopperBroadSwordSlash>();
            item.useTime = item.useAnimation = 33;
            item.channel = true;
            item.autoReuse = false;
        }
        public override void HoldItem(Item item, Player player)
        {
            player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax = 1f;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<CopperBroadSwordSlash>()] < 1)
                Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<CopperBroadSwordSlash>(), item.damage, item.knockBack, player.whoAmI);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class CopperBroadSwordSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.CopperBroadsword;
        public override void RegisterVariables()
        {
            SwordDust1 = DustID.Copper;
            SlashColor = new Color(235, 166, 135);
            MaxComboCount = 3;
        }
        public override void Appear()
        {

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
                        ((CopperBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -1.9f, 1.9f, 0.2f, true, 6f);
                        break;
                    case 1:
                        ((CopperBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.8f, 1.8f, -1.7f, 0.2f, true, 6f);
                        break;
                    case 2:
                        ((CopperBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -2.5f, 2.3f, -0.4f, true, 6f, true, true, 0f, 1.2f);
                        break;
                }
            }
        }
    }
    public class TinBroadSwordRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.TinBroadsword;
        }
        public override void SetDefaults(Item item)
        {
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.Shoot;
            item.shoot = ModContent.ProjectileType<TinBroadSwordSlash>();
            item.useTime = item.useAnimation = 33;
            item.channel = true;
            item.autoReuse = false;
        }
        public override void HoldItem(Item item, Player player)
        {
            player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax = 1f;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<TinBroadSwordSlash>()] < 1)
                Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<TinBroadSwordSlash>(), item.damage, item.knockBack, player.whoAmI);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class TinBroadSwordSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.TinBroadsword;
        public override void RegisterVariables()
        {
            SwordDust1 = DustID.Tin;
            SlashColor = new Color(187, 165, 124);
            MaxComboCount = 3;
        }
        public override void Appear()
        {

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
                        ((TinBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -1.9f, 1.9f, 0.2f, true, 6f);
                        break;
                    case 1:
                        ((TinBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.8f, 1.8f, -1.7f, 0.2f, true, 6f);
                        break;
                    case 2:
                        ((TinBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -2.5f, 2.3f, -0.4f, true, 6f, true, true, 0f, 1.2f);
                        break;
                }
            }
        }
    }
    public class IronBroadSwordRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.IronBroadsword;
        }
        public override void SetDefaults(Item item)
        {
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.Shoot;
            item.shoot = ModContent.ProjectileType<IronBroadSwordSlash>();
            item.useTime = item.useAnimation = 33;
            item.channel = true;
            item.autoReuse = false;
        }
        public override void HoldItem(Item item, Player player)
        {
            player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax = 1f;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<IronBroadSwordSlash>()] < 1)
                Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<IronBroadSwordSlash>(), item.damage, item.knockBack, player.whoAmI);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class IronBroadSwordSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.IronBroadsword;
        public override void RegisterVariables()
        {
            SwordDust1 = DustID.Iron;
            SlashColor = new Color(189, 159, 139);
            MaxComboCount = 3;
        }
        public override void Appear()
        {

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
                        ((IronBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -1.9f, 1.9f, 0.2f, true, 6f);
                        break;
                    case 1:
                        ((IronBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.8f, 1.8f, -1.7f, 0.2f, true, 6f);
                        break;
                    case 2:
                        ((IronBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -2.5f, 2.3f, -0.4f, true, 6f, true, true, 0f, 1.2f);
                        break;
                }
            }
        }
    }
    public class LeadBroadSwordRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.LeadBroadsword;
        }
        public override void SetDefaults(Item item)
        {
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.Shoot;
            item.shoot = ModContent.ProjectileType<LeadBroadSwordSlash>();
            item.useTime = item.useAnimation = 33;
            item.channel = true;
            item.autoReuse = false;
        }
        public override void HoldItem(Item item, Player player)
        {
            player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax = 1f;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<LeadBroadSwordSlash>()] < 1)
                Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<LeadBroadSwordSlash>(), item.damage, item.knockBack, player.whoAmI);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class LeadBroadSwordSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.LeadBroadsword;
        public override void RegisterVariables()
        {
            SwordDust1 = DustID.Lead;
            SlashColor = new Color(104, 140, 150) * 1.5f;
            MaxComboCount = 3;
        }
        public override void Appear()
        {

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
                        ((LeadBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -1.9f, 1.9f, 0.2f, true, 6f);
                        break;
                    case 1:
                        ((LeadBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.8f, 1.8f, -1.7f, 0.2f, true, 6f);
                        break;
                    case 2:
                        ((LeadBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -2.5f, 2.3f, -0.4f, true, 6f, true, true, 0f, 1.2f);
                        break;
                }
            }
        }
    }
    public class SilverBroadSwordRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.SilverBroadsword;
        }
        public override void SetDefaults(Item item)
        {
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.Shoot;
            item.shoot = ModContent.ProjectileType<SilverBroadSwordSlash>();
            item.useTime = item.useAnimation = 33;
            item.channel = true;
            item.autoReuse = false;
        }
        public override void HoldItem(Item item, Player player)
        {
            player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax = 1f;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<SilverBroadSwordSlash>()] < 1)
                Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<SilverBroadSwordSlash>(), item.damage, item.knockBack, player.whoAmI);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class SilverBroadSwordSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.SilverBroadsword;
        public override void RegisterVariables()
        {
            SwordDust1 = DustID.Silver;
            SlashColor = Color.Silver;
            MaxComboCount = 3;
        }
        public override void Appear()
        {

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
                        ((SilverBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -1.9f, 1.9f, 0.2f, true, 6f);
                        break;
                    case 1:
                        ((SilverBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.8f, 1.8f, -1.7f, 0.2f, true, 6f);
                        break;
                    case 2:
                        ((SilverBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -2.5f, 2.3f, -0.4f, true, 6f, true, true, 0f, 1.2f);
                        break;
                }
            }
        }
    }
    public class TungstenBroadSwordRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.TungstenBroadsword;
        }
        public override void SetDefaults(Item item)
        {
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.Shoot;
            item.shoot = ModContent.ProjectileType<TungstenBroadSwordSlash>();
            item.useTime = item.useAnimation = 33;
            item.channel = true;
            item.autoReuse = false;
        }
        public override void HoldItem(Item item, Player player)
        {
            player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax = 1f;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<TungstenBroadSwordSlash>()] < 1)
                Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<TungstenBroadSwordSlash>(), item.damage, item.knockBack, player.whoAmI);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class TungstenBroadSwordSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.TungstenBroadsword;
        public override void RegisterVariables()
        {
            SwordDust1 = DustID.Tungsten;
            SlashColor = Color.LightGreen;
            MaxComboCount = 3;
        }
        public override void Appear()
        {

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
                        ((TungstenBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -1.9f, 1.9f, 0.2f, true, 6f);
                        break;
                    case 1:
                        ((TungstenBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.8f, 1.8f, -1.7f, 0.2f, true, 6f);
                        break;
                    case 2:
                        ((TungstenBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -2.5f, 2.3f, -0.4f, true, 6f, true, true, 0f, 1.2f);
                        break;
                }
            }
        }
    }
    public class GoldBroadSwordRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.GoldBroadsword;
        }
        public override void SetDefaults(Item item)
        {
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.Shoot;
            item.shoot = ModContent.ProjectileType<GoldBroadSwordSlash>();
            item.useTime = item.useAnimation = 33;
            item.channel = true;
            item.autoReuse = false;
        }
        public override void HoldItem(Item item, Player player)
        {
            player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax = 1f;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<GoldBroadSwordSlash>()] < 1)
                Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<GoldBroadSwordSlash>(), item.damage, item.knockBack, player.whoAmI);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class GoldBroadSwordSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.GoldBroadsword;
        public override void RegisterVariables()
        {
            SwordDust1 = DustID.Gold;
            SlashColor = Color.Gold;
            MaxComboCount = 3;
        }
        public override void Appear()
        {

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
                        ((GoldBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -1.9f, 1.9f, 0.2f, true, 6f);
                        break;
                    case 1:
                        ((GoldBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.8f, 1.8f, -1.7f, 0.2f, true, 6f);
                        break;
                    case 2:
                        ((GoldBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -2.5f, 2.3f, -0.4f, true, 6f, true, true, 0f, 1.2f);
                        break;
                }
            }
        }
    }
    public class PlatinumBroadSwordRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.PlatinumBroadsword;
        }
        public override void SetDefaults(Item item)
        {
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.Shoot;
            item.shoot = ModContent.ProjectileType<PlatinumBroadSwordSlash>();
            item.useTime = item.useAnimation = 33;
            item.channel = true;
            item.autoReuse = false;
        }
        public override void HoldItem(Item item, Player player)
        {
            player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax = 1f;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<PlatinumBroadSwordSlash>()] < 1)
                Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<PlatinumBroadSwordSlash>(), item.damage, item.knockBack, player.whoAmI);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class PlatinumBroadSwordSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.PlatinumBroadsword;
        public override void RegisterVariables()
        {
            SwordDust1 = DustID.Platinum;
            SlashColor = Color.Silver * 1.5f;
            MaxComboCount = 3;
        }
        public override void Appear()
        {

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
                        ((PlatinumBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -1.9f, 1.9f, 0.2f, true, 6f);
                        break;
                    case 1:
                        ((PlatinumBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.8f, 1.8f, -1.7f, 0.2f, true, 6f);
                        break;
                    case 2:
                        ((PlatinumBroadSwordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -2.5f, 2.3f, -0.4f, true, 6f, true, true, 0f, 1.2f);
                        break;
                }
            }
        }
    }
}

