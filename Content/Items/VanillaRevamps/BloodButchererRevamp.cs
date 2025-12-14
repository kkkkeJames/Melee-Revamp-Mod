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
using Terraria.ModLoader;
using static MeleeRevamp.Content.Items.VanillaRevamps.BladeOfGrassSlash;

namespace MeleeRevamp.Content.Items.VanillaRevamps
{
    public class BloodButchererRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.BloodButcherer;
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
                    #region Switch states
                    if (projmod.Timer >= projmod.TimeMax)
                    {
                        SwitchState(projectile);
                    }
                    #endregion
                }
                #endregion
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                base.SwitchState(projectile);
            }
        }
    }
}
/*
        public void AlternateAttackTrigger()
        {
            Player player = Main.player[Projectile.owner];
            Timer = 0;
            if (!DrawSword)
            {
                DrawSword = true;
                ApplyDissolve = true;
            }
            IniSet.Set(ArmToSwordOffset, Projectile.rotation, ArmRotation, Projectile.scale); 
            TargetSet.Set(new Vector2(-12, 0).RotatedBy(player.direction == 1 ? -(float)Math.PI : 0), player.direction == 1 ? -(float)Math.PI : 0, player.direction == 1 ? -(float)Math.PI * 3 / 2 : (float)Math.PI * 3 / 2, 2.4f);
            ((BloodButchererSlash)Projectile.ModProjectile).SetState<AlternateAttack>();
        }
        private class AlternateAttack : ProjectileState
        {
            public bool Charging = true;
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                throw new NotImplementedException();
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
                projmod.DrawInverse = player.direction < 0;
                player.itemTime = player.itemAnimation = 2;
                if (Main.mouseRight && !Main.mouseRightRelease)
                    Charging = true;
                else Charging = false;
                if (projmod.ApplyDissolve) projmod.DissolveRate = projmod.Timer / 30f;
                #endregion
                #region Arm change
                if (projmod.Timer <= 120)
                {
                    projmod.TransferToSet(proj, projmod.TargetSet, (projmod.Timer - 30) / 90f, true, true);
                }
                else projmod.ChargeShader = true;
                #endregion
                if (!Charging)
                {
                    #region Stop charging
                    if (projmod.Timer > 120)
                    {
                        projmod.ChargeShader = false;
                        projmod.TimeMax = player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge * 600; 
                        projmod.Timer = 0; 
                        projmod.SetState<AlternateAttack2>(); 
                    }
                    #endregion
                }
                else
                {
                    #region Stop charging after 12s
                    if (projmod.Timer > 720)
                    {
                        projmod.ChargeShader = false;
                        projmod.Timer = 0; 
                        projmod.TimeMax = player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge * 600;
                        projmod.SetState<AlternateAttack2>(); 
                    }
                    #endregion
                }
            }
        }
        private class AlternateAttack2 : ProjectileState
        {
            public bool Charging = true;
            public int ChargeDirection = 1;
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                throw new NotImplementedException();
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                #region Basic setting
                Projectile proj = projectile.Projectile;
                BloodButchererSlash projmod = (BloodButchererSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                player.GetModPlayer<BBSpecialPlayer>().BBSpecial = true;
                proj.localNPCHitCooldown = 8;
                projmod.WieldStandardScale = 2.4f;
                projmod.WieldThinScale = 0.4f;
                projmod.WieldHandleLength = 12;
                projmod.DamageScale = 0.2f;
                projmod.ShouldDrawArm = true; 
                player.itemAnimation = player.itemTime = 2; 
                if (projmod.Timer == 0) ChargeDirection = player.direction;
                projmod.Timer++; 
                projmod.SlashDrawTimer = projmod.Timer;
                #endregion
                #region Spin attack
                projmod.WieldAttack = true;
                projmod.CouldHit = true;
                if (projmod.Timer % 60 == 1)
                {
                    player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge -= 0.1f;
                    if (player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge < 0)
                        player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = 0;
                    player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = (float)Math.Round(player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge, 1);
                }
                #region Angle change
                if (projmod.TimeMax - projmod.Timer > 20) proj.rotation += 0.2f * ChargeDirection;
                else proj.rotation += MathHelper.Lerp(0, 0.2f, (projmod.TimeMax - projmod.Timer) / 20f) * ChargeDirection;
                while (proj.rotation > Math.PI * 2) proj.rotation -= (float)Math.PI * 2;
                while (proj.rotation < -Math.PI * 2) proj.rotation += (float)Math.PI * 2;
                projmod.ArmToSwordOffset = new Vector2(-projmod.WieldHandleLength, 0).RotatedBy(proj.rotation);
                proj.scale = MeleeRevampMathHelper.EllipseRadiusHelper(projmod.WieldStandardScale, projmod.WieldStandardScale * projmod.WieldThinScale, projmod.Projectile.rotation);
                projmod.ArmRotation = projmod.ArmRotation = proj.rotation - (float)Math.PI / 2;
                projmod.WieldDrawRadius[projmod.Timer] = projmod.SwordRadius; 
                if (proj.rotation > -Math.PI / 2 && proj.rotation < Math.PI / 2) player.direction = 1;
                else player.direction = -1;
                #endregion
                #region State machine
                if (projmod.Timer >= projmod.TimeMax) 
                {
                    player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = 0;
                    player.GetModPlayer<BBSpecialPlayer>().BBSpecial = false;
                    proj.localNPCHitCooldown = (int)(player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 4;
                    projmod.IniSet.Set(projmod.ArmToSwordOffset, proj.rotation, projmod.ArmRotation, proj.scale); 
                    projmod.TargetSet.Set(new Vector2(0, 0), player.direction > 0 ? 0.1f * (float)Math.PI : 0.9f * (float)Math.PI, 0, 1.6f);
                    projmod.Timer = 0;
                    projmod.TimeMax = 240; 
                    projmod.SetState<Recover>(); 
                    return;
                }
                #endregion
                #endregion
            }
        }
    }
    public class BBSpecialPlayer : ModPlayer
    {
        public bool BBSpecial = false;
        public override void OnHurt(Player.HurtInfo info)
        {
            if (BBSpecial) info.Damage = (int)(info.Damage * 0.75f);
            base.OnHurt(info);
        }
    }
}

 */