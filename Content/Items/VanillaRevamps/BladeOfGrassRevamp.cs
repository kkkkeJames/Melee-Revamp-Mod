using MeleeRevamp.Content.Core;
using MeleeRevamp.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MeleeRevamp.Content.Items.VanillaRevamps
{
    public class BladeOfGrassRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.BladeofGrass;
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(item, tooltips);
            TooltipLine line1 = new(Mod, "STip1", Language.GetTextValue("Mods.MeleeRevamp.Items.BladeOfGrass.Tip1")) { OverrideColor = Color.LawnGreen };
            TooltipLine line2 = new(Mod, "STip2", Language.GetTextValue("Mods.MeleeRevamp.Items.BladeOfGrass.Tip2")) { OverrideColor = Color.LawnGreen };
            TooltipLine line3 = new(Mod, "STip3", Language.GetTextValue("Mods.MeleeRevamp.Items.BladeOfGrass.Tip3")) { OverrideColor = Color.LawnGreen };

            int insertIndex = tooltips.FindLastIndex(t => t.Name != "CreativeSacrificeNeeded");
            if (insertIndex != -1)
            {
                tooltips.Insert(insertIndex, line1);
                tooltips.Insert(insertIndex + 1, line2);
                tooltips.Insert(insertIndex + 2, line3);
            }
            else
            {
                tooltips.Add(line1);
                tooltips.Add(line2);
                tooltips.Add(line3);
            }
        }
        public override void SetDefaults(Item item)
        {
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.Shoot;
            item.useTime = item.useAnimation = 24;
            item.shoot = ProjectileID.None;
            item.channel = true;
            item.autoReuse = false;
        }
        public bool mouseright = false;
        public override void HoldItem(Item item, Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<BladeOfGrassSlash>()] < 1)
            {
                var proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<BladeOfGrassSlash>(), item.damage, item.knockBack, player.whoAmI);
            }
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class BladeOfGrassSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.BladeofGrass;
        private int LeafProjNum = 0;
        public override void RegisterVariables()
        {
            SlashColor = Color.LawnGreen;
            SwordDust3 = DustID.JunglePlants;
            SwordDebuff = BuffID.Poisoned;
            SwordDebuffTime = 7;
            SwordDebuffRate = 4;
            MaxComboCount = 4;
            ShaderTexture = "MeleeRevamp/Content/Assets/ShaderColor/Jungle";
        }
        public override void Appear()
        {
        }
        public override void Initialize()
        {
            base.Initialize();
            RegisterState(new LeftAltCombo1());
            RegisterState(new LeftAltCombo2());
            RegisterState(new Alt1Combo1());
            RegisterState(new Alt2Combo1());
            RegisterState(new Alt2Combo2());
            RegisterState(new Alt3Combo1());
            RegisterState(new Alt3Combo2());
            RegisterState(new Alt3Combo3());
            RegisterState(new Alt4Combo1());
        }
        public override void AIBefore()
        {
            base.AIBefore();
            Player player = Main.player[Projectile.owner];
            BladeOfGrassSlash projmod = (BladeOfGrassSlash)Projectile.ModProjectile;
            if (LeftClick)
            {
                LeafProjNum = 0;
                switch (ComboCount)
                {
                    case 0:
                        ((BladeOfGrassSlash)Projectile.ModProjectile).SetState<Wield>(true, 1.7f, 0.7f, -2f, 1.9f, 0.3f, 0f, true, 6f);
                        LeafProjNum = 0;
                        break;
                    case 1:
                        ((BladeOfGrassSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.6f, 2f, -2f, 0.3f, 0f, true, 6f);
                        break;
                    case 2:
                        if (((BladeOfGrassSlash)Projectile.ModProjectile).ComboTimer <= 60)
                            ((BladeOfGrassSlash)Projectile.ModProjectile).SetState<Stab>(true, 0.3f);
                        else {
                            ((BladeOfGrassSlash)Projectile.ModProjectile).SetState<LeftAltCombo1>(true, 0.2f, false, 0.75f);
                            ComboCount = 0;
                        }
                        break;
                    case 3:
                        ((BladeOfGrassSlash)Projectile.ModProjectile).SetState<Wield>(true, 1.7f, 0.8f, -2.2f, 2.1f, 0.3f, 0f, true, 6f);
                        break;
                }
            }
            if (RightClick)
            {
                switch (ComboCount)
                {
                    case 0:
                        ((BladeOfGrassSlash)Projectile.ModProjectile).SetState<Alt1Combo1>(false, 1.7f, 0.9f, -2.2f, 2.2f, 0f, 0.6f, true, 6f);
                        LeafProjNum = 1;
                        break;
                    case 1:
                        ((BladeOfGrassSlash)Projectile.ModProjectile).SetState<Alt2Combo1>(false, 1.7f, 0.7f, 1.5f, -1.7f, 0f, 0.6f, true, 6f);
                        break;
                    case 2:
                        ((BladeOfGrassSlash)Projectile.ModProjectile).SetState<Alt3Combo1>(false, 1.7f, 0.8f, 1.8f, -1.8f, 0f, 0.6f, true, 6f);
                        break;
                    case 3:
                        ((BladeOfGrassSlash)Projectile.ModProjectile).SetState<Alt4Combo1>(false, 1.7f, 0.3f, -1.8f, 1.5f, 0f, 0.6f, true, 6f);
                        break;
                }
            }
        }
        private int LeftAltCombo1Count = 0;
        public class LeftAltCombo1 : Stab
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                if (args.Length < 2)
                    throw new Exception("Not enough arguments for switching to Stab state.");
                bool countMouseAngle = (bool)args[0];
                float SPGaugeAdd = (float)args[1];
                bool stabCombo = args.Length >= 3 ? (bool)args[2] : true;
                float damageScale = args.Length >= 4 ? (float)args[3] : 1f;
                bool applyScreenShake = args.Length >= 5 ? (bool)args[4] : false;
                StabCombo = stabCombo;
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                player.direction = (Main.MouseWorld - player.Center).X >= 0 ? 1 : -1; // Change player direction based on mouse position
                projmod.Timer = 0;
                projmod.StartStruct.SetStruct(projmod.ArmToSwordOffset, proj.rotation, projmod.ArmRotation, proj.scale);
                projmod.MousePos = Main.MouseWorld - player.Center;
                float exrot = projmod.MousePos.X > 0 ? 0 : (float)Math.PI;
                float randrot = Main.rand.NextFloat(-0.2f, 0.2f);
                if (countMouseAngle)
                {
                    StabStartPosAdd = new Vector2(-projmod.SwordRadius / 2, 0).RotatedBy((float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) + exrot);
                    StabEndPosAdd = new Vector2(0, 0).RotatedBy((float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) + exrot);
                    projmod.TargetStruct1.SetStruct(StabStartPosAdd, (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) + exrot + randrot, (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) + exrot + randrot - (float)Math.PI / 2, 1.6f);
                    projmod.TargetStruct2.SetStruct(StabStartPosAdd, (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) + exrot + randrot, (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) + exrot + randrot - (float)Math.PI / 2, 1.6f);
                }
                else
                {
                    StabStartPosAdd = new Vector2(-projmod.SwordRadius / 2, 0).RotatedBy(exrot);
                    StabEndPosAdd = new Vector2(0, 0).RotatedBy(exrot);
                    projmod.TargetStruct1.SetStruct(StabStartPosAdd, exrot + randrot, exrot + randrot - (float)Math.PI / 2, 1.6f);
                    projmod.TargetStruct2.SetStruct(StabStartPosAdd, exrot + randrot, exrot + randrot - (float)Math.PI / 2, 1.6f);
                }
                projmod.ApplyStuck = true;
                projmod.ApplySlashDust = true;
                projmod.ApplyScreenShake = applyScreenShake;
                projmod.SwordPowerGaugeAdd = SPGaugeAdd;
                projmod.DamageScale = damageScale;
                projmod.isCombo = false;
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 1.2f;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                #region Basic settings
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                player.itemAnimation = player.itemTime = 2;
                int HoldupTimeMax = (int)(projmod.TimeMax / 2); // Notice: hold up time = item.usetime / 2
                int StabTimeMax = (int)(projmod.TimeMax / 2); // Stab time = item.usetime / 2
                projmod.Timer++;
                #endregion
                #region Hold up the sword
                if (projmod.Timer <= HoldupTimeMax)
                {
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct1, (float)projmod.Timer / HoldupTimeMax, true, true);
                }
                #endregion
                #region Stab
                else if (projmod.Timer <= HoldupTimeMax + StabTimeMax)
                {
                    projmod.CouldHit = true;
                    projmod.SlashDrawTimer = projmod.Timer - HoldupTimeMax;
                    projmod.SlashDrawTimeMax = StabTimeMax;
                    if (projmod.Timer == HoldupTimeMax + 1)
                    {
                        projmod.StartStruct.SetStruct(projmod.ArmToSwordOffset, proj.rotation, projmod.ArmRotation, proj.scale);
                        projmod.TargetStruct2.SetStruct(StabEndPosAdd, proj.rotation, projmod.ArmRotation, proj.scale);
                    }
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct2, (projmod.Timer - HoldupTimeMax) / (float)StabTimeMax, true, true);
                }
                #endregion
                #region Switch state
                if (projmod.Timer >= projmod.TimeMax)
                {
                    SwitchState(projectile);
                }
                #endregion
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.AttackHit = false;
                ((BladeOfGrassSlash)proj.ModProjectile).LeftAltCombo1Count++;
                if (Main.mouseLeft && ((BladeOfGrassSlash)proj.ModProjectile).LeftAltCombo1Count < 8)
                {
                    projmod.SetState<LeftAltCombo1>(true, 0.2f, false, 0.75f);
                }
                else
                {
                    ((BladeOfGrassSlash)proj.ModProjectile).LeftAltCombo1Count = 0;
                    projmod.SetState<LeftAltCombo2>(true, 0.2f, false, 1f, true);
                }
            }
        }
        public class LeftAltCombo2 : Stab
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                base.TriggerAI(projectile, args);
                Projectile proj = projectile.Projectile;
                BladeOfGrassSlash projmod = (BladeOfGrassSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                float exrot = projmod.MousePos.X > 0 ? 0 : (float)Math.PI;
                StabEndPosAdd = new Vector2(projmod.SwordRadius / 2, 0).RotatedBy((float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) + exrot);
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 9f;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                #region Basic settings
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                player.itemAnimation = player.itemTime = 2;
                int HoldupTimeMax = (int)(projmod.TimeMax / 9 * 4);
                int StabTimeMax = (int)(projmod.TimeMax / 9);
                int RecoverTimeMax = (int)projmod.TimeMax - HoldupTimeMax - StabTimeMax;
                proj.localNPCHitCooldown = (int)(player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 4;
                projmod.Timer++;
                #endregion
                #region Hold up the sword
                if (projmod.Timer <= HoldupTimeMax)
                {
                    projmod.CouldHit = false;
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct1, (float)projmod.Timer / HoldupTimeMax, true, true);
                    //projmod.MoveSwordSet(proj, projmod.TargetStruct1, (float)projmod.Timer / HoldupTimeMax);
                }
                #endregion
                #region Stab
                else if (projmod.Timer <= HoldupTimeMax + StabTimeMax)
                {
                    projmod.CouldHit = true;
                    projmod.SlashDrawTimer = projmod.Timer - HoldupTimeMax;
                    projmod.SlashDrawTimeMax = StabTimeMax;
                    if (projmod.Timer == HoldupTimeMax + 1)
                    {
                        projmod.StartStruct.SetStruct(projmod.ArmToSwordOffset, proj.rotation, projmod.ArmRotation, proj.scale);
                        projmod.TargetStruct2.SetStruct(StabEndPosAdd, proj.rotation, projmod.ArmRotation, proj.scale);
                    }
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct2, (projmod.Timer - HoldupTimeMax) / (float)StabTimeMax, true, true);
                }
                #endregion
                #region Recover
                else
                {
                    projmod.CouldHit = false;
                    projmod.SlashDrawTimer = projmod.Timer - HoldupTimeMax - StabTimeMax;
                    projmod.SlashDrawTimeMax = RecoverTimeMax;
                    if (projmod.Timer == HoldupTimeMax + StabTimeMax + 1)
                    {
                        projmod.StartStruct.SetStruct(projmod.ArmToSwordOffset, proj.rotation, projmod.ArmRotation, proj.scale);
                        projmod.TargetStruct2.SetStruct(StabStartPosAdd, proj.rotation, projmod.ArmRotation, proj.scale);
                    }
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct2, (projmod.Timer - HoldupTimeMax - StabTimeMax) / (float)RecoverTimeMax, true, true);
                }
                #endregion
                #region Switch state
                if (projmod.Timer >= projmod.TimeMax)
                {
                    SwitchState(projectile);
                }
                #endregion
            }
        }
        private class Alt1Combo1 : Wield
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                base.TriggerAI(projectile, args);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 5.2f;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                base.AI(projectile);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                int HoldupTimeMax = 24;
                #region Shoot Projectile (If the projectile shot is set in the weapon item)
                if (!projmod.FullGaugeCost) ((BladeOfGrassSlash)proj.ModProjectile).LeafProjNum = 0;
                else if (projmod.Timer == HoldupTimeMax + 1)
                {
                    for (int i = 0; i < ((BladeOfGrassSlash)proj.ModProjectile).LeafProjNum; i++)
                        Projectile.NewProjectileDirect(proj.GetSource_FromThis(), player.Center, new Vector2(player.direction * 16, 0).RotateRandom(0.5f), ProjectileID.BladeOfGrass, (int)(proj.damage * 0.75f), proj.knockBack, player.whoAmI, -player.direction * Main.rand.NextFloat(0.08f, 0.16f));
                }
                #endregion
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                base.SwitchState(projectile);
            }
        }

        private class Alt2Combo1 : Wield
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                base.TriggerAI(projectile, args);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 3.6f;
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.SetState<Alt2Combo2>(projmod.FullGaugeCost, false, 2f, 0.36f, -1.6f, 1.7f, 0f, 0f, true, 6f);
            }
        }
        private class Alt2Combo2 : Wield
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                bool tempflag = (bool)args[0];
                base.TriggerAI(projectile, args.Skip(1).ToArray());
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.FullGaugeCost = tempflag;
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 3.6f;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                base.AI(projectile);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                int HoldupTimeMax = 24;
                #region Shoot Projectile (If the projectile shot is set in the weapon item)
                if (!projmod.FullGaugeCost) ((BladeOfGrassSlash)proj.ModProjectile).LeafProjNum = 0;
                else if (projmod.Timer == HoldupTimeMax + 1)
                {
                    ((BladeOfGrassSlash)proj.ModProjectile).LeafProjNum++;
                    for (int i = 0; i < ((BladeOfGrassSlash)proj.ModProjectile).LeafProjNum; i++)
                        Projectile.NewProjectileDirect(proj.GetSource_FromThis(), player.Center, new Vector2(player.direction * 16, 0).RotateRandom(0.5f), ProjectileID.BladeOfGrass, (int)(proj.damage * 0.75f), proj.knockBack, player.whoAmI, -player.direction * Main.rand.NextFloat(0.08f, 0.16f));
                }
                #endregion
            }
        }
        private class Alt3Combo1 : Wield
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                base.TriggerAI(projectile, args);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 3.2f;
                player.velocity.Y -= 10f;
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.SetState<Alt3Combo2>(false, 1.7f, 0.9f, -2.4f, 2.4f, 0f, 0f, true, 6f);
            }
        }
        private class Alt3Combo2 : Wield
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                base.TriggerAI(projectile, args);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 2.8f;
                player.velocity.Y += 15f;
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.SetState<Alt3Combo3>(projmod.FullGaugeCost, false, 1.7f, 0.9f, -1.6f, 1.2f, 0f, 0f, true, 6f);
            }
        }
        private class Alt3Combo3 : Wield
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                bool tempflag = (bool)args[0];
                base.TriggerAI(projectile, args.Skip(1).ToArray());
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.FullGaugeCost = tempflag;
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 2.8f;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                base.AI(projectile);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                int HoldupTimeMax = 24;
                #region Shoot Projectile (If the projectile shot is set in the weapon item)
                if (!projmod.FullGaugeCost) ((BladeOfGrassSlash)proj.ModProjectile).LeafProjNum = 0;
                else if (projmod.Timer == HoldupTimeMax + 1)
                {
                    ((BladeOfGrassSlash)proj.ModProjectile).LeafProjNum++;
                    for (int i = 0; i < ((BladeOfGrassSlash)proj.ModProjectile).LeafProjNum; i++)
                        Projectile.NewProjectileDirect(proj.GetSource_FromThis(), player.Center, new Vector2(player.direction * 16, 0).RotateRandom(0.5f), ProjectileID.BladeOfGrass, (int)(proj.damage * 0.75f), proj.knockBack, player.whoAmI, -player.direction * Main.rand.NextFloat(0.08f, 0.16f));
                }
                #endregion
            }
        }
        private class Alt4Combo1 : Wield
        {
            public int startDirection = 1;
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                base.TriggerAI(projectile, args);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 4.8f;
                startDirection = player.direction;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                //base.AI(projectile);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                proj.localNPCHitCooldown = 18;
                projmod.ShouldDrawArm = true;
                player.itemAnimation = player.itemTime = 2;
                projmod.Timer++;
                projmod.SlashDrawTimer = projmod.Timer;
                int HoldupTimeMax = 24;
                int SwingTimeMax = (int)((player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 4);
                int StopTimeMax = (int)((player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 0.8f);
                if (projmod.Timer <= HoldupTimeMax)
                {
                    float timer = (float)projmod.Timer / (float)HoldupTimeMax;
                    //projmod.MoveSwordSet(proj, projmod.PrepSet, timer);
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct1, timer, true, true);
                    if (projmod.ApplyDissolve) projmod.DissolveRate = timer;
                }
                else
                {
                    projmod.ApplyDissolve = false;
                    projmod.WieldAttack = true;
                    projmod.CouldHit = projmod.Timer >= SwingTimeMax ? false : true;
                    proj.rotation += 0.18f * (projmod.Timer >= SwingTimeMax ? MathHelper.Lerp(1, 0, (float)(projmod.Timer - SwingTimeMax) / StopTimeMax) : 1) * startDirection;
                    while (proj.rotation > Math.PI * 2) proj.rotation -= (float)Math.PI * 2;
                    while (proj.rotation < -Math.PI * 2) proj.rotation += (float)Math.PI * 2;
                    projmod.ArmToSwordOffset = new Vector2(-projmod.WieldHandleLength, 0).RotatedBy(proj.rotation);
                    proj.scale = MeleeRevampMathHelper.EllipseRadiusHelper(WieldStandardScale, WieldStandardScale * WieldThinScale, projmod.Projectile.rotation); // Change sword's scale
                    projmod.ArmRotation = proj.rotation - (float)Math.PI / 2;
                    projmod.WieldDrawRadius[projmod.Timer] = projmod.SwordRadius;
                    if (proj.rotation > -Math.PI / 2 && proj.rotation < Math.PI / 2) player.direction = 1;
                    else player.direction = -1;
                    #region Shoot Projectile (If the projectile shot is set in the weapon item)
                    if (!projmod.FullGaugeCost) ((BladeOfGrassSlash)proj.ModProjectile).LeafProjNum = 0;
                    else if (projmod.Timer == HoldupTimeMax + 1)
                    {
                        ((BladeOfGrassSlash)proj.ModProjectile).LeafProjNum++;
                        for (int i = 0; i < ((BladeOfGrassSlash)proj.ModProjectile).LeafProjNum; i++)
                            Projectile.NewProjectileDirect(proj.GetSource_FromThis(), player.Center, new Vector2(player.direction * 16, 0).RotateRandom(0.5f), ProjectileID.BladeOfGrass, (int)(proj.damage * 0.75f), proj.knockBack, player.whoAmI, -player.direction * Main.rand.NextFloat(0.08f, 0.16f));
                    }
                    #endregion
                    if (projmod.Timer >= projmod.TimeMax)
                    {
                        SwitchState(projectile);
                    }
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
