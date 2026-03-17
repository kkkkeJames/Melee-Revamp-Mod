using MeleeRevamp.Content.Core;
using MeleeRevamp.Content.Dusts;
using MeleeRevamp.Content.Projectiles;
using Microsoft.Build.Execution;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Threading;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MeleeRevamp.Content.Items.VanillaRevamps
{
    public class Volcano : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.FieryGreatsword;
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(item, tooltips);
            TooltipLine line1 = new(Mod, "STip1", Language.GetTextValue("Mods.MeleeRevamp.Items.Volcano.Tip1")) { OverrideColor = new Color(0xff, 0xc8, 0x96) };
            TooltipLine line2 = new(Mod, "STip2", Language.GetTextValue("Mods.MeleeRevamp.Items.Volcano.Tip2")) { OverrideColor = new Color(0xff, 0xc8, 0x96) };
            TooltipLine line3 = new(Mod, "STip3", Language.GetTextValue("Mods.MeleeRevamp.Items.Volcano.Tip3")) { OverrideColor = new Color(0xff, 0xc8, 0x96) };

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
        public override void HoldItem(Item item, Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<FieryGreatswordSlash>()] < 1)
            {
                var proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<FieryGreatswordSlash>(), item.damage, item.knockBack, player.whoAmI);
            }
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class FieryGreatswordSlash : GlobalSwordSlash, IDrawBloom
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.FieryGreatsword;
        private int flametimer = 0;
        private bool Ignited = false;
        private int IgniteTimer = 0;
        private int flameHeightMult = 0;
        private float SwordPowerAdd = 0.2f;
        private bool flameShear = false; 
        private int flameShearAngle = 0;
        public override void RegisterVariables()
        {
            Player player = Main.player[Projectile.owner];
            SlashColor = new Color(0xe2, 0x58, 0x22); 
            MaxComboCount = 3;
        }
        public override void Appear()
        {
        }
        public override void Initialize()
        {
            base.Initialize();
            RegisterState(new FlameWield());
            RegisterState(new FlameRecover());
            RegisterState(new AltAttackCharge());
        }
        public override void AIBefore()
        {
            base.AIBefore();
            flametimer++;
            Player player = Main.player[Projectile.owner];
            FieryGreatswordSlash projmod = (FieryGreatswordSlash)Projectile.ModProjectile;
            if (Ignited)
            {
                Projectile.damage = (int)(player.HeldItem.damage * 4f);
                SwordPowerAdd = 0.0f;
                IgniteTimer++;
                if (flameHeightMult < 10 && IgniteTimer % 5 == 0)
                    flameHeightMult++;
                if (IgniteTimer >= 120)
                {
                    IgniteTimer = 0;
                    player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge -= 0.1f;
                    player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = (float)Math.Round(player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge, 1);
                }
                flameShear = false;
            }
            else
            {
                flameHeightMult = 0;
                SwordPowerAdd = 0.2f;
                if (flameHeightMult > 0)
                    flameHeightMult--;
            }
            if (LeftClick)
            {
                switch (ComboCount)
                {
                    case 0:
                        projmod.SetState<FlameWield>(true, 2.4f, 0.8f, -2.1f, 2f, SwordPowerAdd, 0f, true, 6f);
                        break;
                    case 1:
                        projmod.SetState<FlameWield>(true, 2.4f, 0.4f, 1.9f, -1.9f, SwordPowerAdd, 0f, true, 6f);
                        break;
                    case 2:
                        projmod.SetState<FlameWield>(true, 2.4f, 0.9f, -2.2f, 2.3f, SwordPowerAdd, 0f, true, 6f, true, true);
                        break;
                }
            }
            if (RightClick)
            {
                ((FieryGreatswordSlash)Projectile.ModProjectile).SetState<AltAttackCharge>();
                IgniteTimer = 0;
            }
        }
        public override void AIAfter()
        {
            base.AIAfter();
            Player player = Main.player[Projectile.owner];
            FieryGreatswordSlash projmod = (FieryGreatswordSlash)Projectile.ModProjectile;
            if (player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge >= player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax)
            {
                Ignited = true;

            }
            if (Ignited)
            {
                if (player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge <= 0.0f)
                {
                    Ignited = false;
                    IgniteTimer = 0;
                }
                if ((Math.Abs(player.velocity.X) > 0 || flameShear) && flameShearAngle < 30) flameShearAngle += 1;
                if ((player.velocity.X == 0 && !flameShear) && flameShearAngle > 0) flameShearAngle -= 1;
            }
        }
        private class FlameWield : Wield
        {
            public override void AI(ProjectileStateMachine projectile)
            {
                base.AI(projectile);
                Projectile proj = projectile.Projectile;
                FieryGreatswordSlash projmod = (FieryGreatswordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                if (projectile.Timer <= 12 || projectile.Timer > 24 && projectile.Timer <= projmod.TimeMax - 24)
                    projmod.flameShear = true;
                else projmod.flameShear = false;
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                base.SwitchState(projectile);
                Projectile proj = projectile.Projectile;
                FieryGreatswordSlash projmod = (FieryGreatswordSlash)proj.ModProjectile;
                projmod.flameShear = false;
                projmod.SetState<FlameRecover>();
            }
        }
        private class FlameRecover : Recover
        {
            public override void AI(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                FieryGreatswordSlash projmod = (FieryGreatswordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.ShouldDrawArm = true;
                projmod.WieldAttack = false;
                projmod.ChargeShader = false;
                projmod.CouldHit = false;
                projmod.DamageScale = 1f;
                projmod.ShootProjDamScale = 1f;
                projmod.ApplyStuck = false;
                projmod.ApplySlashDust = false;
                projmod.ApplyScreenShake = false;
                projmod.AttackHit = false;
                projmod.SwordPowerGaugeAdd = 0;
                projmod.Timer++;
                #region Switch state
                if (projmod.Timer > projmod.TimeMax / 10 && projmod.Timer <= projmod.TimeMax * 2 / 5)
                {
                    projmod.LerpSwordStruct(proj, projmod.TargetStruct2, (projmod.Timer - (projmod.TimeMax / 10)) / (projmod.TimeMax * 3 / 10), true, true);
                }
                else if (projmod.Timer > projmod.TimeMax * 2 / 5)
                {
                    proj.rotation = player.direction < 0 ? 0.9f * (float)Math.PI : 0.1f * (float)Math.PI;
                    projmod.FlipSwordTexture = player.direction < 0;
                }
                if (!projmod.Ignited)
                {
                    projmod.StartStruct.SetStruct(projmod.ArmToSwordOffset, proj.rotation, projmod.ArmRotation, proj.scale);
                    projmod.TargetStruct2.SetStruct(new Vector2(0, 0), player.direction > 0 ? 0.1f * (float)Math.PI : 0.9f * (float)Math.PI, 0, 1.6f);
                    projmod.Timer = 48;
                    projmod.TimeMax = 240;
                    projmod.SetState<Recover>();
                }
                #endregion
            }
        }
        private class AltAttackCharge : ProjectileState
        {
            public bool Charging = true;
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                Projectile proj = projectile.Projectile;
                FieryGreatswordSlash projmod = (FieryGreatswordSlash)proj.ModProjectile;
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
                FieryGreatswordSlash projmod = (FieryGreatswordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                #endregion
                #region State data
                if (projmod.Timer == 0)
                    Charging = true;
                projmod.Timer++;
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
                FieryGreatswordSlash projmod = (FieryGreatswordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.ChargeShader = false;
                projmod.DrawSword = true;
                projmod.ApplyDissolve = false;
                if (projmod.Timer > 240)
                    projmod.SetState<FlameWield>(false, 3.6f, 0.4f, -3.14f, 3.14f, 0.3f, 0f, false, 6f);
                else projmod.SetState<Recover>();
            }
        }
        public override void PostDraw(Color lightColor)
        {
            FieryGreatswordSlash projmod = (FieryGreatswordSlash)Projectile.ModProjectile;
            if (Ignited)
            {
                if (Main.rand.NextBool(10))
                {
                    int flameWidth = (int)(projmod.SwordRadius * 2 - 22 * Projectile.scale);
                    int randp = Main.rand.Next(flameWidth) - flameWidth / 2;
                    Dust.NewDustPerfect(Projectile.Center + new Vector2(14 * Projectile.scale + randp, 0).RotatedBy(Projectile.rotation), ModContent.DustType<PixelatedGlow>(), new Vector2(0, -5f), 0, new Color(255, 100, 20, 255), Main.rand.NextFloat(0.4f, 0.6f));
                }
            }
            base.PostDraw(lightColor);
        }
        public void DrawBloom()
        {
            FieryGreatswordSlash projmod = (FieryGreatswordSlash)Projectile.ModProjectile;
            Player player = Main.player[Projectile.owner];
            // Draw flame graph when ignited, this will be processed to have a bloom
            // The size of bloom is based on the SwordRadius of projectile, and the height is based on the timer of ignition
            if (flameHeightMult > 0)
            {
                Texture2D noisemap_tex = ModContent.Request<Texture2D>("MeleeRevamp/Content/Assets/Periodic").Value;
                int flameWidth = (int)(projmod.SwordRadius * 2 - 22 * Projectile.scale);
                int flameHeight = 60 * flameHeightMult / 10;
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
                // The effect to make the noise map scroll vertically based on time with visibility of each pixel depending on alpha, color based on coordinates
                Effect effect = Filters.Scene["FlameScroll"].GetShader().Shader;
                effect.Parameters["tex2Scale"].SetValue(new Vector2(1, 1));
                effect.Parameters["timer"].SetValue(flametimer * 0.0045f);
                Main.graphics.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("MeleeRevamp/Content/Assets/Gradient").Value;
                effect.CurrentTechnique.Passes[0].Apply();
                // Draw the noise map. It's a parallelogram, so we use two vertex paints to draw two triangles
                List<VertexInfo2> parallelogram = new List<VertexInfo2>();
                int projdir = !projmod.FlipSwordTexture ? 1 : -1;
                Vector2 drawCenter = Projectile.Center - Main.screenPosition + new Vector2(14 * Projectile.scale, -3.2f * projdir * flameHeightMult).RotatedBy(Projectile.rotation);
                float flameOffset = (float)Math.Tan(MathHelper.ToRadians(projmod.flameShearAngle)) * flameHeight;
                parallelogram.Add(new VertexInfo2(drawCenter + new Vector2(projdir * -flameWidth / 2.0f - flameOffset, projdir * -flameHeight / 2.0f).RotatedBy(Projectile.rotation), new Vector3(0, 0, 1), Color.White));
                parallelogram.Add(new VertexInfo2(drawCenter + new Vector2(projdir * flameWidth / 2.0f - flameOffset, projdir * -flameHeight / 2.0f).RotatedBy(Projectile.rotation), new Vector3(1, 0, 1), Color.White));
                parallelogram.Add(new VertexInfo2(drawCenter + new Vector2(projdir * -flameWidth / 2.0f, projdir * flameHeight / 2.0f).RotatedBy(Projectile.rotation), new Vector3(0, 1, 1), Color.White));
                parallelogram.Add(new VertexInfo2(drawCenter + new Vector2(projdir * flameWidth / 2.0f, projdir * flameHeight / 2.0f).RotatedBy(Projectile.rotation), new Vector3(1, 1, 1), Color.White));
                Main.graphics.GraphicsDevice.Textures[0] = noisemap_tex;
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, parallelogram.ToArray(), 0, 2);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            }
        }
    }
}