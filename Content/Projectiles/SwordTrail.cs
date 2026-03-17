using MeleeRevamp.Content.Core;
using MeleeRevamp.Content.Particles;
using Microsoft.Build.Execution;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core;
using ReLogic.Content;
using SDL2;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace MeleeRevamp.Content.Projectiles
{
    public class SwordTrail : ModProjectile, IDrawWarp
    {
        public override string Texture => "MeleeRevamp/Content/Assets/ShaderColor/Demonite";
        public static Asset<Texture2D> WarpTexture;
        public float CurveTimeleft;
        public float upScale, downScale;
        public float headScale, bottomScale;
        private Vector2[] PosCache = new Vector2[30];
        private Vector2 OrigCenter;
        public override void Load()
        {
            WarpTexture = ModContent.Request<Texture2D>("MeleeRevamp/Content/Assets/WarpTex");
        }
        public override void Unload()
        {
            WarpTexture = null;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.friendly = false;
            Projectile.timeLeft = 60;
            Projectile.scale = 1;
            Projectile.extraUpdates = 3;
            Projectile.height = 60;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            Projectile.timeLeft = Projectile.localNPCHitCooldown = (int)Projectile.ai[0];
            PosCache = new Vector2[(int)Projectile.ai[0] / 2];
            CurveTimeleft = Main.rand.Next((int)(Projectile.ai[0] / 4 * 3) - 3, (int)(Projectile.ai[0] / 4 * 3) + 3);
            upScale = Main.rand.NextFloat(0.9f, 1f);
            downScale = Main.rand.NextFloat(0.9f, 1f);
            bottomScale = Main.rand.NextFloat(0.2f, 0.3f);
            headScale = Main.rand.NextFloat(0.6f, 0.8f);
            OrigCenter = Projectile.Center;
            if (Projectile.ai[1] == 1)
            {
                headScale = Main.rand.NextFloat(0.2f, 0.3f);
                bottomScale = Main.rand.NextFloat(0.6f, 0.8f);
            }
            if (Projectile.ai[2] == 1)
                Projectile.friendly = true;
        }
        public override void AI()
        {
            base.AI();
            if (Projectile.timeLeft > Projectile.ai[0] / 2)
            {
                for (int i = PosCache.Length - 1; i > 0; i--)
                {
                    PosCache[i] = PosCache[i - 1];
                }
                PosCache[0] = Projectile.Center;
            }
            if (Projectile.timeLeft <= Projectile.ai[0] / 2 + 1)
                Projectile.velocity = Vector2.Zero;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), OrigCenter, Projectile.Center, Projectile.height, ref point);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            Player player = Main.player[Projectile.owner];
            if (Projectile.ai[2] == 1 && !MeleeRevampConfigClient.Instance.CameraLockDisable)
            {
                PunchCameraModifier camPunch = new(player.Center, new Vector2(0f, -1f), 10f * MeleeRevampConfigClient.Instance.ShakeIntensity, 3f, 10, 1000f);
                Main.instance.CameraModifiers.Add(camPunch);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            List<VertexInfo2> slash = new List<VertexInfo2>();
            int iTimer = 1;
            while (iTimer < PosCache.Length && !(PosCache[iTimer] == Vector2.Zero))
            {
                Vector2 normal = PosCache[iTimer - 1] - PosCache[iTimer];
                normal = Vector2.Normalize(new Vector2(-normal.Y, normal.X));
                Vector2 pos = PosCache[iTimer] - Main.screenPosition;
                float lerptimer = 1f;
                if (Projectile.timeLeft > CurveTimeleft)
                {
                    lerptimer = MeleeRevampMathHelper.expDownLerpHelper(headScale, 1, (float)(iTimer - 1) / (Projectile.ai[0] - 1 - CurveTimeleft), 1.5f);
                }
                else
                {
                    if (iTimer < (Projectile.ai[0] - CurveTimeleft))
                        lerptimer = MeleeRevampMathHelper.expDownLerpHelper(headScale, 1, (float)(iTimer - 1) / (Projectile.ai[0] - 1 - CurveTimeleft), 1.5f);
                    else lerptimer = MeleeRevampMathHelper.expUpLerpHelper(1, bottomScale, (float)(iTimer - (Projectile.ai[0] - 1 - CurveTimeleft)) / (CurveTimeleft - (Projectile.ai[0] / 2)), 3);
                }
                if (Projectile.timeLeft <= CurveTimeleft)
                {
                    if (Projectile.timeLeft > (Projectile.ai[0] / 2))
                        lerptimer *= MathHelper.SmoothStep(0.8f, 1f, (Projectile.timeLeft - (Projectile.ai[0] / 2)) / (CurveTimeleft - (Projectile.ai[0] / 2)));
                    else lerptimer *= MathHelper.Lerp(0f, 0.8f, Projectile.timeLeft / (Projectile.ai[0] / 2));
                }
                float height = lerptimer * Projectile.height / 2;
                float timer = (float)iTimer / PosCache.Length;
                slash.Add(new VertexInfo2(pos - normal * height * upScale, new Vector3(MathHelper.Lerp(0, 1, timer), 0, 1), Color.White));
                slash.Add(new VertexInfo2(pos + normal * height * downScale, new Vector3(MathHelper.Lerp(0, 1, timer), 1, 1), Color.White));
                iTimer++;
            }
            #region Set up vertex paint
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Effect effect = Filters.Scene["TimeTrail"].GetShader().Shader;
            effect.Parameters["timer"].SetValue((float)Projectile.timeLeft * 0.04f);
            Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("MeleeRevamp/Content/Assets/Trail", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Main.graphics.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
            effect.CurrentTechnique.Passes[0].Apply();
            if (slash.Count >= 3)
            {
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, slash.ToArray(), 0, slash.Count - 2);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            #endregion
            return false;
        }
        public void DrawWarp()
        {
            Color color = Color.White;
            PreDraw(ref color);
        }
    }
}
