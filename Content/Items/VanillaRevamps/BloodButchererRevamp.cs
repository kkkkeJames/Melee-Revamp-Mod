using MeleeRevamp.Content.Core;
using MeleeRevamp.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    public class BloodButchererRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.BloodButcherer;
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(item, tooltips);
            TooltipLine line1 = new(Mod, "STip1", Language.GetTextValue("Mods.MeleeRevamp.Items.BloodButcherer.Tip1")) { OverrideColor = Color.DarkRed * 2f };
            TooltipLine line2 = new(Mod, "STip2", Language.GetTextValue("Mods.MeleeRevamp.Items.BloodButcherer.Tip2")) { OverrideColor = Color.DarkRed * 2f };

            int insertIndex = tooltips.FindLastIndex(t => t.Name != "CreativeSacrificeNeeded");
            if (insertIndex != -1)
            {
                tooltips.Insert(insertIndex, line1);
                tooltips.Insert(insertIndex + 1, line2);
            }
            else
            {
                tooltips.Add(line1);
                tooltips.Add(line2);
            }
        }
        public override void SetDefaults(Item item)
        {
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.Shoot;
            item.useTime = item.useAnimation = 36;
            item.shoot = ProjectileID.None;
            item.channel = true;
            item.autoReuse = false;
        }
        public override void HoldItem(Item item, Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<BloodButchererSlash>()] < 1)
            {
                var proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<BloodButchererSlash>(), item.damage, item.knockBack, player.whoAmI);
            }
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class BloodButchererSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.BloodButcherer;
        public override void RegisterVariables()
        {
            Player player = Main.player[Projectile.owner];
            SwordDust1 = DustID.Blood;
            SlashColor = Color.DarkRed * 2f;
            AlternateAttackCount = 1;
            MaxComboCount = 3;
        }
        public override void Appear()
        {
        }
        public override void Initialize()
        {
            base.Initialize();
            RegisterState(new LeftAltCombo1());
            RegisterState(new AltAttackCharge());
            RegisterState(new AltAttack());
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
                        ((BloodButchererSlash)Projectile.ModProjectile).SetState<Wield>(true, 2.4f, 0.7f, -2.1f, 2.1f, 0.2f, 0f, true, 10f);
                        break;
                    case 1:
                        ((BloodButchererSlash)Projectile.ModProjectile).SetState<Wield>(true, 2.4f, 0.8f, 2f, -1.9f, 0.2f, 0f, true, 10f);
                        break;
                    case 2:
                        ((BloodButchererSlash)Projectile.ModProjectile).SetState<LeftAltCombo1>(true, 2.6f, 0.7f, -2.6f, 2.4f, 0.6f, 0.2f, true, 12f, true, true, 0f, 2f);
                        break;
                }
            }
            if (RightClick)
            {
                ((BloodButchererSlash)Projectile.ModProjectile).SetState<AltAttackCharge>();
            }
        }
        // The last normal attack is a modified version of normal wield, mostly copied from the orig implementation
        private class LeftAltCombo1 : Wield
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                base.TriggerAI(projectile, args);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 6;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                #region Basic settings
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.ShouldDrawArm = true; // Player's arm angle is determined by code
                player.itemAnimation = player.itemTime = 2; // The player is always in using weapon state
                int HoldupTimeMax = 24; // The time player hold up the sword, which is 6f in this case
                int FullHoldupTimeMax = (int)projmod.TimeMax / 2;
                int WieldTimeMax = (int)projmod.TimeMax - HoldupTimeMax; // The time player wield the sword
                proj.localNPCHitCooldown = (int)(player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 4;
                if (projmod.WieldStuckTimer > 0) // Modify stuck frames
                    projmod.WieldStuckTimer--;
                else projmod.Timer++;
                #endregion
                #region Hold up the sword
                if (projmod.Timer <= HoldupTimeMax)
                {
                    projmod.ChargeShader = true;
                    float timer = (float)projmod.Timer / (float)HoldupTimeMax;
                    //projmod.StartStruct.SetCurrentStruct(proj);
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct1, timer, true, true);
                    //projmod.MoveSwordSet(proj, projmod.TargetStruct1, timer);
                    if (projmod.ApplyDissolve) projmod.DissolveRate = timer;
                }
                #endregion
                #region Wield the sword
                else if (projmod.Timer > FullHoldupTimeMax)
                {
                    projmod.ChargeShader = false;
                    projmod.ApplyDissolve = false;
                    projmod.WieldAttack = true;
                    projmod.CouldHit = true;
                    int WieldTimer;
                    WieldTimer = projmod.SlashDrawTimer = projmod.Timer - FullHoldupTimeMax;
                    #region Modify angle, radius, etc.
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct2, (float)(projmod.Timer - FullHoldupTimeMax) / (float)(projmod.TimeMax - FullHoldupTimeMax), false, false, true);
                    proj.scale = MeleeRevampMathHelper.EllipseRadiusHelper(WieldStandardScale, WieldStandardScale * WieldThinScale, projmod.Projectile.rotation - (projmod.ShouldCountMouse ? (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) : 0)); // Change sword's scale
                    projmod.WieldDrawRadius[WieldTimer] = projmod.SwordRadius;
                    #endregion
                }
                #endregion
                #region Switch states
                if (projmod.Timer >= projmod.TimeMax)
                {
                    SwitchState(projectile);
                }
                #endregion
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                base.SwitchState(projectile);
            }
        }
        private class AltAttackCharge : ProjectileState
        {
            public bool Charging = true;
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                Projectile proj = projectile.Projectile;
                BloodButchererSlash projmod = (BloodButchererSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                if (!projmod.DrawSword)
                {
                    projmod.DrawSword = true;
                    projmod.ApplyDissolve = true;
                }
                projmod.TargetStruct1.SetStruct(new Vector2(-12, 0).RotatedBy(player.direction == 1 ? -(float)Math.PI : 0), player.direction == 1 ? -(float)Math.PI : 0, player.direction == 1 ? -(float)Math.PI * 3 / 2 : (float)Math.PI * 3 / 2, 2.4f);
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                #region Basic Settings
                Projectile proj = projectile.Projectile;
                BloodButchererSlash projmod = (BloodButchererSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                #endregion
                #region State data
                if (projmod.Timer == 0)
                    Charging = true;
                projmod.Timer++;
                if (projmod.Timer % 30 == 1)
                {
                    player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge += 0.1f;
                    if (player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge > player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax)
                        player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax;
                    player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = (float)Math.Round(player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge, 1);
                }
                projmod.WieldAttack = false;
                projmod.ShouldDrawArm = true;
                projmod.FlipSwordTexture = player.direction < 0;
                player.itemTime = player.itemAnimation = 2;
                if (Main.mouseRight && !Main.mouseRightRelease)
                    Charging = true;
                else Charging = false;
                if (projmod.ApplyDissolve) projmod.DissolveRate = projmod.Timer / 30f;
                #endregion
                #region Arm change
                if (projmod.Timer <= 120)
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct1, (projmod.Timer - 30) / 90f, true, true);
                else projmod.ChargeShader = true;
                #endregion
                if (!Charging && projmod.Timer > 120 || projmod.Timer > 720)
                {
                    SwitchState(projectile);
                }
            }

            public override void SwitchState(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                BloodButchererSlash projmod = (BloodButchererSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.ChargeShader = false;
                projmod.SetState<AltAttack>(false, 2.4f, 0.4f, 0f, 0f, 0f, 0f, true, 10f);
            }
        }
        private class AltAttack : Wield
        {
            public int startDirection = 1;
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                base.TriggerAI(projectile, args);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.TimeMax = player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge * 600;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                proj.localNPCHitCooldown = 18;
                projmod.ShouldDrawArm = true;
                player.itemAnimation = player.itemTime = 2;
                projmod.Timer++;
                projmod.SlashDrawTimer = projmod.Timer;
                projmod.ApplyDissolve = false;
                projmod.WieldAttack = true;
                projmod.CouldHit = true;
                proj.rotation += 0.18f * startDirection;
                while (proj.rotation > Math.PI * 2) proj.rotation -= (float)Math.PI * 2;
                while (proj.rotation < -Math.PI * 2) proj.rotation += (float)Math.PI * 2;
                projmod.ArmToSwordOffset = new Vector2(-projmod.WieldHandleLength, 0).RotatedBy(proj.rotation);
                proj.scale = MeleeRevampMathHelper.EllipseRadiusHelper(WieldStandardScale, WieldStandardScale * WieldThinScale, projmod.Projectile.rotation); // Change sword's scale
                projmod.ArmRotation = proj.rotation - (float)Math.PI / 2;
                projmod.WieldDrawRadius[projmod.Timer] = projmod.SwordRadius;
                if (proj.rotation > -Math.PI / 2 && proj.rotation < Math.PI / 2) player.direction = 1;
                else player.direction = -1;
                if (projmod.Timer >= projmod.TimeMax)
                {
                    SwitchState(projectile);
                }
                
                if (projmod.Timer % 60 == 1)
                {
                    player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge -= 0.1f;
                    if (player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge < 0)
                        player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = 0;
                    player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = (float)Math.Round(player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge, 1);
                }
            }

            public override void SwitchState(ProjectileStateMachine projectile)
            {
                base.SwitchState(projectile);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.CouldHit = false;
                proj.localNPCHitCooldown = (int)(player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 4;
            }
        }
    }
}