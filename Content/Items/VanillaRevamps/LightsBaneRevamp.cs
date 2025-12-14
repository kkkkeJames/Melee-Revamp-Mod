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
    public class LightsBaneRevamp : GlobalItem
    {
        public bool normalend = false;
        public int phase = 0;
        public int damage;
        public float timer;
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.LightsBane;
        }
        public override void SetDefaults(Item item)
        {
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.Shoot;
            item.useTime = item.useAnimation = 26;
            item.shoot = ProjectileID.None;
            item.channel = true;
            item.autoReuse = false;
        }
        public override void HoldItem(Item item, Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<LightsBaneSlash>()] < 1)
            {
                var proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<LightsBaneSlash>(), item.damage, item.knockBack, player.whoAmI);
            }
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class LightsBaneSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.LightsBane;
        public override void RegisterVariables()
        {
            Player player = Main.player[Projectile.owner];
            SwordDust1 = DustID.Demonite;
            SlashColor = Color.BlueViolet;
            MaxComboCount = 4;
        }
        public override void Appear()
        {
        }
        public override void Initialize()
        {
            base.Initialize();
            //RegisterState(new AltAttackCombo2());
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
                        ((LightsBaneSlash)Projectile.ModProjectile).SetState<Wield>(true, 1.7f, 0.7f, -2.1f, 1.8f, 0.2f, 0f, true, 8f);
                        break;
                    case 1:
                        ((LightsBaneSlash)Projectile.ModProjectile).SetState<Stab>(true, 0.2f);
                        break;
                    case 2:
                        ((LightsBaneSlash)Projectile.ModProjectile).SetState<Wield>(true, 2.2f, 0.5f, -2.6f, 2.4f, 0.2f, 0f, true, 8f);
                        break;
                    case 3:
                        ((LightsBaneSlash)Projectile.ModProjectile).SetState<Wield>(true, 2.2f, 0.5f, -2.6f, 2.4f, 0.2f, 0f, true, 8f);
                        break;
                }
            }
        }
    }
}
/*
        public void AltChargeTrigger(bool quick = false)
        {
            Player player = Main.player[Projectile.owner];
            Timer = 0;
            if (!DrawSword)
            {
                DrawSword = true;
                ApplyDissolve = true;
            }
            IniSet.Set(ArmToSwordOffset, Projectile.rotation, ArmRotation, Projectile.scale);
            TargetSet.Set(new Vector2(-0, 0).RotatedBy(player.direction == 1 ? -(float)Math.PI : 0), player.direction == 1 ? 0 : -(float)Math.PI, player.direction == 1 ? (float)Math.PI * 3 / 2 : -(float)Math.PI * 3 / 2, 1.6f);
            if (!quick) ((LightsBaneSlash)Projectile.ModProjectile).SetState<AltCharge>();
            else ((LightsBaneSlash)Projectile.ModProjectile).SetState<QuickAltCharge>();
        }
        private class AltCharge : ProjectileState
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
                LightsBaneSlash projmod = (LightsBaneSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner]; 
                #endregion
                #region State data
                if (projmod.Timer == 0)
                    Charging = true;
                projmod.Timer++;
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
                if (projmod.Timer <= 60)
                {
                    projmod.TransferToSet(proj, projmod.TargetSet, projmod.Timer / 60f, true, true);
                }
                else projmod.ChargeShader = true;
                #endregion
                if (!Charging)
                {
                    #region Stop charging
                    if (projmod.Timer > 120)
                    {
                        projmod.ChargeShader = false;
                        projmod.TimeMax = player.HeldItem.useTime;
                        projmod.Timer = 0;
                        //projmod.SetState<AltAttack>();
                        ((LightsBaneSlash)proj.ModProjectile).AltAttackTrigger(false, 0, 1f);
                    }
                    #endregion
                }
                else
                {
                    #region Stop charging after 8s
                    if (projmod.Timer > 480)
                    {
                        projmod.ChargeShader = false;
                        projmod.Timer = 0;
                        projmod.TimeMax = player.HeldItem.useTime;
                        //projmod.SetState<AltAttack>();
                        ((LightsBaneSlash)proj.ModProjectile).AltAttackTrigger(false, 0, 1f);
                    }
                    #endregion
                }
            }
        }
        private class QuickAltCharge : ProjectileState
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                throw new NotImplementedException();
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                #region Basic Settings
                Projectile proj = projectile.Projectile;
                LightsBaneSlash projmod = (LightsBaneSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                #endregion
                #region State data
                projmod.Timer++;
                projmod.WieldAttack = false;
                projmod.ShouldDrawArm = true;
                projmod.DrawInverse = player.direction < 0;
                player.itemTime = player.itemAnimation = 2;
                if (projmod.ApplyDissolve) projmod.DissolveRate = projmod.Timer / 15f;
                #endregion
                #region Arm change
                if (projmod.Timer <= 15)
                {
                    projmod.TransferToSet(proj, projmod.TargetSet, projmod.Timer / 15f, true, true);
                }
                else projmod.ChargeShader = true;
                #endregion
                #region Stop charging
                if (projmod.Timer >= 15)
                {
                    projmod.ChargeShader = false;
                    projmod.TimeMax = player.HeldItem.useTime;
                    projmod.Timer = 0;
                    //projmod.SetState<AltAttack>();
                    ((LightsBaneSlash)proj.ModProjectile).AltAttackTrigger(false, 0, 1f);
                }
                #endregion
            }
        }
        public void AltAttackTrigger(bool shouldcountmouse, float SwordPowerGaugeadd, float damscale = 1f)
        {
            Player player = Main.player[Projectile.owner];
            StabDrawArmBefore = ShouldDrawArm;
            Timer = 0;
            IniSet.Set(ArmToSwordOffset, Projectile.rotation, ArmRotation, Projectile.scale);
            MousePos = Main.MouseWorld - player.Center;
            float exrot = MousePos.X > 0 ? 0 : (float)Math.PI;
            if (shouldcountmouse)
            {
                StabStartPosAdd = new Vector2(-SwordRadius / 2, 0).RotatedBy((float)Math.Atan(MousePos.Y / MousePos.X) + exrot);
                StabEndPosAdd = new Vector2(0, 0).RotatedBy((float)Math.Atan(MousePos.Y / MousePos.X) + exrot);
                PrepSet.Set(StabStartPosAdd, (float)Math.Atan(MousePos.Y / MousePos.X) + exrot, (float)Math.Atan(MousePos.Y / MousePos.X) + exrot - (float)Math.PI / 2, Projectile.scale);
                TargetSet.Set(StabStartPosAdd, (float)Math.Atan(MousePos.Y / MousePos.X) + exrot, (float)Math.Atan(MousePos.Y / MousePos.X) + exrot - (float)Math.PI / 2, 1.6f);
            }
            else
            {
                StabStartPosAdd = new Vector2(-SwordRadius / 2, 0).RotatedBy(exrot);
                StabEndPosAdd = new Vector2(0, 0).RotatedBy(exrot);
                PrepSet.Set(StabStartPosAdd, exrot, exrot - (float)Math.PI / 2, Projectile.scale);
                TargetSet.Set(StabStartPosAdd, exrot, exrot - (float)Math.PI / 2, 1.6f);
            }
            ((LightsBaneSlash)Projectile.ModProjectile).SetState<AltAttack>();
            ApplyStuck = true;
            ApplySlashDust = true;
            SwordPowerGaugeAdd = SwordPowerGaugeadd;
            DamageScale = damscale;
        }
        private class AltAttack : Stab
        {
            public override void AI(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                player.velocity = new Vector2(6 * player.direction, 0);
                if (projmod.Timer >= projmod.TimeMax)
                    ((LightsBaneSlash)proj.ModProjectile).AltAttacking = false;
                else ((LightsBaneSlash)proj.ModProjectile).AltAttacking = true;
                base.AI(projectile);
                if (projmod.Timer >= projmod.TimeMax && ((LightsBaneSlash)proj.ModProjectile).AltAttackHit)
                {
                    ((LightsBaneSlash)proj.ModProjectile).AltAttackComboTrigger(false, 1.7f, 0.7f, -1.1f, 2f, 0.3f, 8);
                }
            }
        }
        public void AltAttackComboTrigger(bool shouldcountmouse, float standardscale, float thinscale, float holdrot, float targrot, float SwordPowerGaugeadd, float handlelength = 0, bool applystuck = false, bool applyscreenshake = false, float stoptime = 0, float damscale = 1f, int projtype = 0, float projdamscale = 1f)
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
            ((LightsBaneSlash)Projectile.ModProjectile).SetState<AltAttackCombo1>();
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
        private class AltAttackCombo1 : Wield
        {
            public override void AI(ProjectileStateMachine projectile)
            {
                base.AI(projectile);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                if (projmod.Timer >= projmod.TimeMax)
                {
                    player.GetModPlayer<MeleeRevampPlayer>().LightsBaneImmunity = false;
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile proj = Projectile;
            Player player = Main.player[proj.owner];
            if (((LightsBaneSlash)proj.ModProjectile).AltAttacking)
            {
                ((LightsBaneSlash)proj.ModProjectile).AltAttackHit = true;
                player.GetModPlayer<MeleeRevampPlayer>().LightsBaneImmunity = true;
            }
            
        }
    }
}

 */