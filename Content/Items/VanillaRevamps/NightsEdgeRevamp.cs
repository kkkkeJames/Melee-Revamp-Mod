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
    public class LightsBaneRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.NightsEdge;
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(item, tooltips);
            TooltipLine line1 = new(Mod, "STip1", Language.GetTextValue("Mods.MeleeRevamp.Items.NightsEdge.Tip1")) { OverrideColor = Color.BlueViolet };
            TooltipLine line2 = new(Mod, "STip2", Language.GetTextValue("Mods.MeleeRevamp.Items.NightsEdge.Tip2")) { OverrideColor = Color.BlueViolet };

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
            item.useTime = item.useAnimation = 26;
            item.shoot = ProjectileID.None;
            item.channel = true;
            item.autoReuse = false;
        }
        public override void HoldItem(Item item, Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<NightsEdgeSlash>()] < 1)
            {
                var proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<NightsEdgeSlash>(), item.damage, item.knockBack, player.whoAmI);
            }
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class NightsEdgeSlash : GlobalSwordSlash
    {
        private bool isAltAttack;
        private bool AltAttackHit;
        private int AltAttackLevel = 1;
        private NPC AltTarget;
        public override string Texture => "Terraria/Images/Item_" + ItemID.NightsEdge;
        public override void RegisterVariables()
        {
            Player player = Main.player[Projectile.owner];
            SwordDust1 = DustID.Demonite;
            SlashColor = Color.BlueViolet;
            MaxComboCount = 4;
            ShaderTexture = "MeleeRevamp/Content/Assets/ShaderColor/Demonite";
        }
        public override void Appear()
        {
        }
        public override void Initialize()
        {
            base.Initialize();
            RegisterState(new AltAttackCharge());
            RegisterState(new AltAttack());
            RegisterState(new Alt2Combo1());
            RegisterState(new Alt2Combo2());
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
                        ((NightsEdgeSlash)Projectile.ModProjectile).SetState<Wield>(true, 1.7f, 0.7f, -2.1f, 1.8f, 0.2f, 0f, true, 8f);
                        break;
                    case 1:
                        ((NightsEdgeSlash)Projectile.ModProjectile).SetState<Stab>(true, 0.2f);
                        break;
                    case 2:
                        ((NightsEdgeSlash)Projectile.ModProjectile).SetState<Wield>(true, 2.2f, 0.5f, -2.6f, 2.4f, 0.2f, 0f, true, 8f);
                        break;
                    case 3:
                        ((NightsEdgeSlash)Projectile.ModProjectile).SetState<Wield>(true, 2.2f, 0.5f, -2.6f, 2.4f, 0.2f, 0f, true, 8f);
                        break;
                }
            }
            if (RightClick)
            {
                ((NightsEdgeSlash)Projectile.ModProjectile).SetState<AltAttackCharge>();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            if (isAltAttack && !AltAttackHit)
            {
                AltAttackHit = true;
                AltTarget = target;
            }
        }
        private class AltAttackCharge : ProjectileState
        {
            public bool Charging = true;
            public int Chargetime = 0;
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                Projectile proj = projectile.Projectile;
                NightsEdgeSlash projmod = (NightsEdgeSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                if (!projmod.DrawSword)
                {
                    projmod.DrawSword = true;
                    projmod.ApplyDissolve = true;
                }
                projmod.TargetStruct1.SetStruct(new Vector2(8, 0).RotatedBy(player.direction == 1 ? -(float)Math.PI : 0), player.direction == 1 ? 0 : -(float)Math.PI, player.direction == 1 ? (float)Math.PI * 3 / 2 : -(float)Math.PI * 3 / 2, 1.6f);
                Chargetime = 0;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                #region Basic Settings
                Projectile proj = projectile.Projectile;
                NightsEdgeSlash projmod = (NightsEdgeSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                #endregion
                #region State data
                if (projmod.Timer == 0)
                    Charging = true;
                projmod.Timer++;
                if (projmod.Timer % 30 == 1)
                {
                    player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge -= 0.1f;
                    if (player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge < 0)
                        player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = 0;
                    else Chargetime++;
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
                if ((!Charging && projmod.Timer > 120) || projmod.Timer > 720 || Chargetime >= 12)
                {
                    if (Chargetime >= 12)
                        projmod.AltAttackLevel = 1;
                    else projmod.AltAttackLevel = 0;
                    SwitchState(projectile);
                }
            }

            public override void SwitchState(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                NightsEdgeSlash projmod = (NightsEdgeSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.ChargeShader = false;
                projmod.SetState<AltAttack>(false, 0f);
            }
        }
        private class AltAttack : Lunge
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                base.TriggerAI(projectile, args);
                Projectile proj = projectile.Projectile;
                NightsEdgeSlash projmod = (NightsEdgeSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.isAltAttack = true;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                NightsEdgeSlash projmod = (NightsEdgeSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                player.GetModPlayer<MeleeRevampPlayer>().Invulnerable = true;
                base.AI(projectile);
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                NightsEdgeSlash projmod = (NightsEdgeSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.isAltAttack = false;
                if (projmod.AltAttackHit)
                {
                    projmod.AltAttackHit = false;
                    if (projmod.AltAttackLevel == 1)
                        projmod.SetState<Alt2Combo1>();
                    else base.SwitchState(projectile);
                    projmod.AltAttackLevel = 0;
                }
                else base.SwitchState(projectile);
            }
        }
        public class Alt2Combo1 : ProjectileState
        {
            public Vector2 OrigCenter;
            public Vector2 Center;
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                Projectile proj = projectile.Projectile;
                NightsEdgeSlash projmod = (NightsEdgeSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                OrigCenter = player.Center;
                Center = projmod.AltTarget.Center;
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                NightsEdgeSlash projmod = (NightsEdgeSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                player.GetModPlayer<MeleeRevampPlayer>().Invulnerable = true;
                projmod.CouldHit = false;
                projmod.Timer++;
                player.itemTime = player.itemAnimation = 2;
                player.velocity = Vector2.Zero;
                player.GetModPlayer<MeleeRevampPlayer>().LightsBaneAlpha = MathHelper.Lerp(1, 0, projmod.Timer <= 60 ? projmod.Timer / 60f : 1f);
                projmod.ApplyDissolve = true;
                projmod.DissolveRate = MathHelper.Lerp(1, 0, projmod.Timer <= 60 ? projmod.Timer / 60f : 1f);
                if (projmod.Timer > 60)
                    player.Center = Center;
                else
                {
                    float lerpTime = 0f;
                    if (projmod.Timer > 10 && projmod.Timer <= 50) lerpTime = MeleeRevampMathHelper.expDownLerpHelper(0f, 1f, (projmod.Timer - 10f) / 40f, 2f);
                    if (projmod.Timer > 50) lerpTime = 1f;
                    player.Center = Vector2.Lerp(OrigCenter, Center, lerpTime);
                    player.GetModPlayer<MeleeRevampScreenPlayer>().TimedZoom(new Vector2(2f, 2f), projmod.Timer, 60f);
                }
                if (projmod.Timer > 60 && projmod.Timer % 36 == 1 && projmod.Timer <= 60 + 36 * 6 + 1)
                {
                    float angle = Main.rand.NextFloat(0f, (float)Math.PI * 2);
                    var shootproj = Projectile.NewProjectileDirect(proj.GetSource_FromThis(), Vector2.Zero, new Vector2(30f, 0).RotatedBy(angle), ModContent.ProjectileType<SwordTrail>(), proj.damage, proj.knockBack, player.whoAmI, 40, 0, 1);
                    shootproj.Center = player.Center - new Vector2(300, 0).RotatedBy(angle);
                }
                if (projmod.Timer > 60 + 36 * 7)
                    SwitchState(projectile);
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                NightsEdgeSlash projmod = (NightsEdgeSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.SetState<Alt2Combo2>(false, 0f);
            }
        }
        public class Alt2Combo2 : Lunge
        {
            private int Temptimer = 0;
            public Vector2 OrigCenter;
            private Vector2 Center;
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                Projectile proj = projectile.Projectile;
                NightsEdgeSlash projmod = (NightsEdgeSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                OrigCenter = player.Center;
                Center = projmod.AltTarget.Center - new Vector2(300 * player.direction, 0);
                Temptimer = 0;
                base.TriggerAI(projectile, args);
            }
            public override void AI(ProjectileStateMachine projectile)
            {
                Temptimer++;
                Projectile proj = projectile.Projectile;
                NightsEdgeSlash projmod = (NightsEdgeSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                player.GetModPlayer<MeleeRevampPlayer>().Invulnerable = true;
                player.itemTime = player.itemAnimation = 2;
                if (Temptimer > 120)
                    base.AI(projectile);
                if (Temptimer <= 20)
                {
                    player.GetModPlayer<MeleeRevampPlayer>().LightsBaneAlpha = 0f;
                    player.Center = Vector2.Lerp(OrigCenter, Center, MeleeRevampMathHelper.expDownLerpHelper(0f, 1f, Temptimer / 20f));
                }
                else
                {
                    if (Temptimer <= 60)
                    {
                        player.GetModPlayer<MeleeRevampPlayer>().LightsBaneAlpha = MathHelper.Lerp(0, 1, (Temptimer - 20f) / 40f);
                        projmod.ApplyDissolve = true;
                        projmod.DissolveRate = MathHelper.Lerp(0, 1, Temptimer / 60f);
                    }
                    if (Temptimer <= 144)
                        player.Center = Center;
                    else
                    {
                        if (Temptimer <= 204)
                            player.GetModPlayer<MeleeRevampScreenPlayer>().TimedZoom(new Vector2(2f), Temptimer - 144f, 60f, true);
                        else player.GetModPlayer<MeleeRevampScreenPlayer>().ClearZoomVector();
                    }
                }
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                Player player = Main.player[projectile.Projectile.owner];
                player.GetModPlayer<MeleeRevampPlayer>().Invulnerable = false;
                base.SwitchState(projectile);
            }
        }
    }
}