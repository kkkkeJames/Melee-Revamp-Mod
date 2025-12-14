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
    public class BladeOfGrassRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.BladeofGrass;
        }
        public override void SetDefaults(Item item)
        {
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.Shoot;
            item.useTime = item.useAnimation = 20;
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
        public override void RegisterVariables()
        {
            SlashColor = Color.LawnGreen;
            SwordDust3 = DustID.JunglePlants;
            SwordDebuff = BuffID.Poisoned;
            SwordDebuffTime = 7;
            SwordDebuffRate = 4;
            MaxComboCount = 4;
        }
        public override void Appear()
        {
        }
        public override void Initialize()
        {
            base.Initialize();
            RegisterState(new LeftAltCombo1());
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
            if (LeftClick)
            {
                switch (ComboCount)
                {
                    case 0:
                        ((BladeOfGrassSlash)Projectile.ModProjectile).SetState<Wield>(true, 1.7f, 0.7f, -2f, 1.9f, 0.3f, 0f, true, 6f);
                        break;
                    case 1:
                        ((BladeOfGrassSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.6f, 2f, -2f, 0.3f, 0f, true, 6f);
                        break;
                    case 2:
                        if (((BladeOfGrassSlash)Projectile.ModProjectile).ComboTimer <= 60)
                            ((BladeOfGrassSlash)Projectile.ModProjectile).SetState<Stab>(true, 0.3f);
                        else ((BladeOfGrassSlash)Projectile.ModProjectile).SetState<LeftAltCombo1>(true, 0f, false, 0.8f);
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
                base.TriggerAI(projectile, args);
                Projectile proj = projectile.Projectile;
                BladeOfGrassSlash projmod = (BladeOfGrassSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                Main.NewText("Trigger");
            }
            public override void SwitchState(ProjectileStateMachine projectile)
            {
                Projectile proj = projectile.Projectile;
                BladeOfGrassSlash projmod = (BladeOfGrassSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                ((BladeOfGrassSlash)proj.ModProjectile).LeftAltCombo1Count++;
                Main.NewText(Main.mouseLeft);
                if (Main.mouseLeft && ((BladeOfGrassSlash)proj.ModProjectile).LeftAltCombo1Count < 12)
                {
                    projmod.SetState<LeftAltCombo1>(true, 0f, false, 0.8f);
                }
                else
                {
                    ((BladeOfGrassSlash)proj.ModProjectile).LeftAltCombo1Count = 0;
                    base.SwitchState(projectile);
                }
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
                projmod.SetState<Alt2Combo2>(false, 2f, 0.36f, -1.6f, 1.7f, 0f, 0f, true, 6f);
            }
        }
        private class Alt2Combo2 : Wield
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                base.TriggerAI(projectile, args);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 3.6f;
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
                projmod.SetState<Alt3Combo3>(false, 1.7f, 0.9f, -1.6f, 1.2f, 0f, 0f, true, 6f);
            }
        }
        private class Alt3Combo3 : Wield
        {
            public override void TriggerAI(ProjectileStateMachine projectile, params object[] args)
            {
                base.TriggerAI(projectile, args);
                Projectile proj = projectile.Projectile;
                GlobalSwordSlash projmod = (GlobalSwordSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner];
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 2.8f;
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

    public class GrassSlash : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_927";
        public override void SetDefaults()
        {
            Projectile.width = 576;
            Projectile.height = 72;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 15;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
        }
        public override void AI()
        {
            if (Projectile.ai[0] == 0) Projectile.alpha = 255;
            Projectile.ai[0]++;
            if (Projectile.ai[0] <= 5) Projectile.alpha -= 51;
            if (Projectile.ai[0] >= 11) Projectile.alpha += 51;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - new Vector2(Projectile.width / 4, 0).RotatedBy(Projectile.rotation), Projectile.Center + new Vector2(Projectile.width / 4, 0).RotatedBy(Projectile.rotation), Projectile.height, ref point);
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20, 5), DustID.JungleGrass, Main.rand.NextVector2Circular(5, 5));
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Color color;
            List<VertexInfo2> circle = new List<VertexInfo2>();
            for (int i = 0; i <= 80; i++) 
            {
                if (i <= 39)
                    color = Color.Lerp(Color.LawnGreen, Color.White, (float)i / 40);
                else if (i == 40)
                    color = Color.White;
                else color = Color.Lerp(Color.White, Color.LawnGreen, (float)(i - 41) / 40);
                Vector2 pos0 = Projectile.Center - Main.screenPosition;
                circle.Add(new VertexInfo2(pos0 + new Vector2(Projectile.width * (i * 0.0125f - 0.5f) * Projectile.scale, -Projectile.height * 0.5f * Projectile.scale).RotatedBy(Projectile.rotation), new Vector3(0 + i * 0.0125f, 0, 1), color * ((float)(255 - Projectile.alpha) / 255)));
                circle.Add(new VertexInfo2(pos0 + new Vector2(Projectile.width * (i * 0.0125f - 0.5f) * Projectile.scale, Projectile.height * 0.5f * Projectile.scale).RotatedBy(Projectile.rotation), new Vector3(0 + i * 0.0125f, 1, 1), color * ((float)(255 - Projectile.alpha) / 255)));
            }
            #region Vertex Paint
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.Projectile[Projectile.type].Value;
            if (circle.Count >= 3)
            {
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, circle.ToArray(), 0, circle.Count - 2);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            #endregion
            return false;
        }
    }
}
