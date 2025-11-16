/*
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

namespace MeleeRevamp.Content.Items.VanillaRevamps
{
    public class BloodButchererRevamp : GlobalItem
    {
        public bool normalend = false;
        public int phase = 0;
        public int damage;
        public float timer;
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
            item.shoot = ModContent.ProjectileType<BloodButchererSlash>();
            item.autoReuse = false;
        }
        public override void HoldItem(Item item, Player player)
        {
            player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax = 2.4f;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<BloodButchererSlash>()] < 1)
            {
                var proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<BloodButchererSlash>(), item.damage, item.knockBack, player.whoAmI);
            }
            if (normalend == true) 
            {
                timer++; 
                if (timer >= 60)
                {
                    normalend = false;
                }
            }
            else timer = 0;
        }
        public override bool AltFunctionUse(Item item, Player player)
        {
            return true;
        }
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 0)
            {
                timer = 0;
                if (normalend)
                {
                    phase++; 
                    if (phase == 3)
                        phase = 0;  
                }
                else
                {
                    phase = 0; 
                }
                normalend = true;
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.type == ModContent.ProjectileType<BloodButchererSlash>() && proj.owner == player.whoAmI && proj != null)
                    {
                        switch (phase)
                        {
                            case 0:
                                ((BloodButchererSlash)proj.ModProjectile).WieldTrigger(true, 2.4f * item.scale, 0.7f, -1.9f, 1.9f, 0.3f, 10);
                                break;
                            case 1:
                                ((BloodButchererSlash)proj.ModProjectile).WieldTrigger(true, 2.4f * item.scale, 0.8f, 1.8f, -1.7f, 0.3f, 10);
                                break;
                            case 2:
                                ((BloodButchererSlash)proj.ModProjectile).SpecialTrigger(true, 2.6f * item.scale, 0.7f, -2.5f, 2.3f, -0.6f, 12, true, true, 0, 2f);
                                break;
                        }
                    }
                }
            }
            else if (player.altFunctionUse == 2)
            {
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.type == ModContent.ProjectileType<BloodButchererSlash>() && proj.owner == player.whoAmI && proj != null)
                    {
                        ((BloodButchererSlash)proj.ModProjectile).AlternateAttackTrigger();
                    }
                }
            }
            return false;
        }
    }
    public class BloodButchererSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.BloodButcherer;
        public BloodButchererSlash()
        {

        }
        public override void Appear()
        {
        }
        public override void Initialize()
        {
            base.Initialize();
            RegisterState(new Special());
            RegisterState(new AlternateAttack());
            RegisterState(new AlternateAttack2());
        }
        public override void RegisterVariables()
        {
            Player player = Main.player[Projectile.owner];
            SwordDust1 = DustID.Blood;
            SlashColor = Color.DarkRed * 2f;
            AlternateAttackCount = 1;
        }
        public void SpecialTrigger(bool shouldcountmouse, float standardscale, float thinscale, float holdrot, float targrot, float SwordPowerGaugeadd, float handlelength = 0, bool applystuck = false, bool applyscreenshake = false, float stoptime = 0, float damscale = 1f, int projtype = 0, float projdamscale = 1f)
        {
            Player player = Main.player[Projectile.owner];
            WieldDrawArmBefore = ShouldDrawArm;
            Timer = 0; // Reset timer
            IniSet.Set(ArmToSwordOffset, Projectile.rotation, ArmRotation, Projectile.scale); // Get iniset
            ShouldCountMouse = shouldcountmouse; // Should this wield count mouse angle into consideration
            if (shouldcountmouse) MousePos = Main.MouseWorld - player.Center; // If yes, get mouse rotation
            WieldHoldRot = holdrot; // Get the final angle of hold
            WieldFinalRot = targrot; // Get the final angle of wielding
            DrawInverse = player.direction < 0 ? true : false; // Change direction of projectile draw based on player direction
            if (targrot < holdrot) DrawInverse = !DrawInverse;
            if (player.direction < 0)
            {
                WieldHoldRot = (float)Math.PI - WieldHoldRot; // If player's direction is left, change hold rot and final rot based on them
                WieldFinalRot = (float)Math.PI - WieldFinalRot;
            }
            float scl = MeleeRevampMathHelper.EllipseRadiusHelper(standardscale, standardscale * thinscale, WieldHoldRot);
            if (shouldcountmouse) // Take mouse into consideration
            {
                WieldHoldRot += (float)Math.Atan(MousePos.Y / MousePos.X);
                WieldFinalRot += (float)Math.Atan(MousePos.Y / MousePos.X);
            }
            WieldStandardScale = standardscale; // Change radius issue
            WieldThinScale = thinscale;
            WieldHandleLength = handlelength;
            PrepSet.Set(new Vector2(-WieldHandleLength, 0).RotatedBy(WieldHoldRot), WieldHoldRot, WieldHoldRot - (float)Math.PI / 2f, scl);
            // Calculate the final rotation by all those inputs with the ellipse radius helper
            int targscaleflag = ShouldCountMouse ? 1 : 0;
            float targscale = MeleeRevampMathHelper.EllipseRadiusHelper(WieldStandardScale, WieldStandardScale * WieldThinScale, WieldFinalRot - (float)Math.Atan(MousePos.Y / MousePos.X) * targscaleflag);
            TargetSet.Set(Vector2.Zero, WieldFinalRot, WieldFinalRot - (float)Math.PI / 2, targscale); // Set the targetset
            ((GlobalSwordSlash)Projectile.ModProjectile).SetState<Special>();
            if (!DrawSword)
            {
                DrawSword = true;
                ApplyDissolve = true;
            }
            DamageScale = damscale;
            ShootProj = projtype;
            ApplyStuck = applystuck;
            ApplySlashDust = true;
            ApplyScreenShake = applyscreenshake;
            SwordPowerGaugeAdd = SwordPowerGaugeadd;
        }
        // The last normal attack is a modified version of normal wield, mostly copied from the orig implementation
        private class Special : ProjectileState
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                throw new NotImplementedException();
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                #region Basic settings
                Projectile proj = projectile.Projectile;
                BloodButchererSlash projmod = (BloodButchererSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner]; 
                projmod.ShouldDrawArm = true; 
                player.itemAnimation = player.itemTime = 2; 
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 16 / 3; 
                int HoldupTimeMax = (int)(projmod.TimeMax * 3 / 4); 
                int WieldTimeMax = (int)projmod.TimeMax - HoldupTimeMax; 
                if (projmod.WieldStuckTimer > 0) 
                    projmod.WieldStuckTimer--; 
                else projmod.Timer++; 
                #endregion
                #region Charge for 1/2 time
                if (projmod.Timer <= HoldupTimeMax / 2) 
                {
                    projmod.WieldAttack = false; 
                    float timer = (float)projmod.Timer / (float)HoldupTimeMax;
                    projmod.MoveSwordSet(proj, projmod.PrepSet, timer);
                    if (projmod.ApplyDissolve) projmod.DissolveRate = timer;

                }
                #endregion
                #region Wield 
                else if (projmod.Timer > HoldupTimeMax)
                {
                    projmod.ApplyDissolve = false;
                    projmod.WieldAttack = true;
                    projmod.CouldHit = true;
                    int WieldTimer;
                    WieldTimer = projmod.SlashDrawTimer = projmod.Timer - HoldupTimeMax;
                    #region Modify angle, radius, etc.
                    projmod.RotSetTargetLogistic(proj, projmod.TargetSet, projmod.Timer - HoldupTimeMax, (float)(projmod.TimeMax - HoldupTimeMax)); // Use logistics function to determine rotation of projectile
                    projmod.ArmToSwordOffset = new Vector2(-projmod.WieldHandleLength, 0).RotatedBy(proj.rotation); // Modify the player's arm rotation
                    proj.scale = MeleeRevampMathHelper.EllipseRadiusHelper(projmod.WieldStandardScale, projmod.WieldStandardScale * projmod.WieldThinScale, projmod.Projectile.rotation - (projmod.ShouldCountMouse ? (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) : 0)); // Change sword's scale
                    projmod.ArmRotation = proj.rotation - (float)Math.PI / 2; // The arm moves with the sword
                    projmod.WieldDrawRadius[WieldTimer] = projmod.SwordRadius;
                    #endregion
                    #region If there are any projectile that it should shoot
                    if (projmod.ShootProj != 0 && projmod.Timer == HoldupTimeMax + WieldTimeMax / 4)
                    {
                        Projectile shootproj = Projectile.NewProjectileDirect(proj.GetSource_FromThis(), player.Center, Vector2.Zero, projmod.ShootProj, (int)(proj.damage * projmod.ShootProjDamScale), proj.knockBack, Main.myPlayer);
                        shootproj.direction = player.direction;
                    }
                    #endregion
                    #region (Not Implemented) Deflect projectiles
                    if (projmod.Timer >= -12 + HoldupTimeMax + WieldTimeMax / 4 && projmod.Timer <= 12 + HoldupTimeMax + WieldTimeMax / 4)
                    {
                        foreach (Projectile counterproj in Main.projectile)
                        {
                            Rectangle projhitbox = new Rectangle((int)(proj.Center.X + new Vector2(projmod.SwordRadius, 0).RotatedBy(proj.rotation).X) - 10, (int)(proj.Center.Y + new Vector2(projmod.SwordRadius, 0).RotatedBy(proj.rotation).Y) - 10, 20, 20);
                            Rectangle counterhitbox = counterproj.Hitbox;
                        }
                    }
                    #endregion
                    #region Switch states
                    if (projmod.Timer >= projmod.TimeMax)
                    {
                        projmod.IniSet.Set(projmod.ArmToSwordOffset, proj.rotation, projmod.ArmRotation, proj.scale);
                        projmod.TargetSet.Set(new Vector2(0, 0), player.direction > 0 ? 0.1f * (float)Math.PI : 0.9f * (float)Math.PI, 0, 1.6f);
                        projmod.Timer = 0;
                        projmod.TimeMax = 240;
                        projmod.SetState<Recover>();
                        return;
                    }
                    #endregion
                }
                #endregion
            }
        }
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