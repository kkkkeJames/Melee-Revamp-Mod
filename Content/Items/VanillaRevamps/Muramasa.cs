using MeleeRevamp.Content.Core;
using MeleeRevamp.Content.Projectiles;
using Microsoft.Build.Execution;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MeleeRevamp.Content.Items.VanillaRevamps
{
    public class Muramasa : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.Muramasa;
        }
        public override void HoldItem(Item item, Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<MuramasaSlash>()] < 1)
            {
                var proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<MuramasaSlash>(), item.damage, item.knockBack, player.whoAmI);
            }
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class MuramasaSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Muramasa;
        public override void RegisterVariables()
        {
            Player player = Main.player[Projectile.owner];
            SlashColor = new Color(25, 30, 123);
            MaxComboCount = 5;
            ShaderTexture = "MeleeRevamp/Content/Assets/ShaderColor/Sapphire";
        }
        public override void Appear()
        {
        }
        public override void Initialize()
        {
            base.Initialize();
            RegisterState(new LeftAltCombo1());
            RegisterState(new LeftAltCombo2());
        }
        public override void AIBefore()
        {
            base.AIBefore();
            Player player = Main.player[Projectile.owner];
            MuramasaSlash projmod = (MuramasaSlash)Projectile.ModProjectile;
            if (LeftClick)
            {
                switch (ComboCount)
                {
                    case 0:
                        projmod.SetState<Wield>(true, 1.8f, 0.7f, -2f, 1.2f, 0.3f, 0f, true, 6f);
                        break;
                    case 1:
                        projmod.SetState<Wield>(true, 2.2f, 0.3f, 1.9f, -1.7f, 0f, 0f, true, 6f);
                        break;
                    case 2:
                        projmod.SetState<LeftAltCombo1>(true, 0f);
                        break;
                    case 3:
                        projmod.SetState<Wield>(true, 2f, 0.8f, 1.9f, -0.5f, 0.3f, 0f, true, 6f, false, false);
                        break;
                    case 4:
                        projmod.SetState<LeftAltCombo2>(false, 2f, 0.8f, -0.8f, 3f, 0.3f, 0f, true, 6f, false, true);
                        break;
                }
            }
        }
        private class LeftAltCombo1 : Stab
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                base.TriggerAI(projectile, args);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 6;
            }
        }
        private class LeftAltCombo2 : Wield
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                base.TriggerAI(projectile, args);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 5;
            }
        }
    }
}