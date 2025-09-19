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
        public bool special = false;
        public bool normalend = false;
        public int phase = 0;
        public int damage;
        public float timer;
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
            item.shoot = ModContent.ProjectileType<BladeOfGrassSlash>();
            item.channel = true;
            item.autoReuse = false;
        }
        public bool mouseright = false;
        public override void HoldItem(Item item, Player player)
        {
            player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax = 2.4f;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<BladeOfGrassSlash>()] < 1)
            {
                var proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<BladeOfGrassSlash>(), item.damage, item.knockBack, player.whoAmI);
            }
            if (normalend == true) //在一斩结束后开始计算后摇
            {
                timer++; //计时器增加后摇时间
                if (timer >= 60)
                {
                    normalend = false; //六十f是极限时间，过去了就是重新开始
                }
            }
            else timer = 0;

            if (Main.mouseRightRelease && mouseright)
            {
                mouseright = false;
                special = !special;
            }
            if (Main.mouseRight)
                mouseright = true;
        }
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            timer = 0;
            if (normalend)
            {
                phase++; //在60f的攻击后摇之内再次攻击的话，段数指示器加一，表示这一打是下一打
                if (phase == 4)
                    phase = 0; //如果第四段打完则重置到第一段
            }
            else
            {
                phase = 0; //60f后摇接受后攻击，则直接返回第一段
            }
            normalend = true;
            if (!special)
            {
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.type == ModContent.ProjectileType<BladeOfGrassSlash>() && proj.owner == player.whoAmI && proj != null)
                    {
                        switch (phase)
                        {
                            case 0:
                                ((BladeOfGrassSlash)proj.ModProjectile).WieldTrigger(true, 1.7f * item.scale, 0.7f, -2f, 1.9f, 0.2f, 6);
                                break;
                            case 1:
                                ((BladeOfGrassSlash)proj.ModProjectile).WieldTrigger(true, 2f * item.scale, 0.6f, 2f, -2f, 0.2f, 6);
                                break;
                            case 2:
                                ((BladeOfGrassSlash)proj.ModProjectile).StabTrigger(true, 0.2f);
                                break;
                            case 3:
                                ((BladeOfGrassSlash)proj.ModProjectile).WieldTrigger(true, 1.7f * item.scale, 0.9f, -2.2f, 2.1f, 0.2f, 6);
                                break;
                        }
                    }
                }
            }
            else
            {
                if (player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge >= 1.6f || player.GetModPlayer<BladeOfGrassPlayer>().Special)
                {
                    player.GetModPlayer<BladeOfGrassPlayer>().Special = false;
                    player.GetModPlayer<BladeOfGrassPlayer>().SpecialTimer = 0;
                    phase = 0;
                    foreach (Projectile proj in Main.projectile)
                    {
                        if (proj.type == ModContent.ProjectileType<BladeOfGrassSlash>() && proj.owner == player.whoAmI && proj != null)
                        {
                            ((BladeOfGrassSlash)proj.ModProjectile).SpecialWieldTrigger(false, 2f * item.scale, 0.6f, 2f, -2f, 0f, 6);
                        }
                    }
                }
            }
            return false;
        }
    }
    public class BladeOfGrassSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.BladeofGrass;
        public BladeOfGrassSlash()
        {

        }
        public override void Appear()
        {
        }
        public override void Initialize()
        {
            base.Initialize();
            RegisterState(new SpecialWield());
        }
        public override void RegisterVariables()
        {
            Player player = Main.player[Projectile.owner];
            SlashColor = Color.LawnGreen;
            SwordDust3 = DustID.JunglePlants;
            SwordDebuff = BuffID.Poisoned;
            SwordDebuffTime = 7;
            SwordDebuffRate = 4;
        }
        public void SpecialWieldTrigger(bool shouldcountmouse, float standardscale, float thinscale, float holdrot, float targrot, float SwordPowerGaugeadd, float handlelength = 0, float stoptime = 0, float damscale = 1f, int projtype = 0, float projdamscale = 1f)
        {
            Player player = Main.player[Projectile.owner];
            WieldDrawArmBefore = ShouldDrawArm;
            Timer = 0; //重置计时器
            IniSet.Set(ArmToSwordOffset, Projectile.rotation, ArmRotation, Projectile.scale); //获取iniset
            ShouldCountMouse = shouldcountmouse; //获取此次挥剑角度是否由鼠标角度决定，也即是否此时需要获取鼠标位置
            if (shouldcountmouse) MousePos = Main.MouseWorld - player.Center; //若需要，获取此时的鼠标位置
            WieldHoldRot = holdrot; //获取举剑的目标角度
            WieldFinalRot = targrot; //获取挥剑的目标角度
            DrawInverse = player.direction < 0 ? true : false; //改变射弹绘制方向
            if (targrot < holdrot) DrawInverse = !DrawInverse; //如果从下向上挥则改变射弹方向
            if (player.direction < 0) //如果玩家为负方向
            {
                WieldHoldRot = (float)Math.PI - WieldHoldRot; //改变两个角度使得角度变成正向
                WieldFinalRot = (float)Math.PI - WieldFinalRot;
            }
            float scl = MeleeRevampMathHelper.EllipseRadiusHelper(standardscale, standardscale * thinscale, WieldHoldRot);
            if (shouldcountmouse) //加入鼠标角度的影响
            {
                WieldHoldRot += (float)Math.Atan(MousePos.Y / MousePos.X);
                WieldFinalRot += (float)Math.Atan(MousePos.Y / MousePos.X);
            }
            WieldStandardScale = standardscale; //解决半径问题
            WieldThinScale = thinscale;
            WieldHandleLength = handlelength;
            TargetSet.Set(new Vector2(-WieldHandleLength, 0).RotatedBy(WieldHoldRot), WieldHoldRot, WieldHoldRot - (float)Math.PI / 2f, scl);
            ((GlobalSwordSlash)Projectile.ModProjectile).SetState<SpecialWield>();
            DamageScale = damscale;
            ShootProj = projtype;
            SwordPowerGaugeAdd = SwordPowerGaugeadd;
        }
        private class SpecialWield : ProjectileState
        {
            public Vector2 slashpos1;
            public Vector2 slashpos2;
            public override void AI(ProjectileStateMachine projectile)
            {
                #region 基础设置
                Projectile proj = projectile.Projectile;
                BladeOfGrassSlash projmod = (BladeOfGrassSlash)proj.ModProjectile;
                Player player = Main.player[proj.owner]; //基础设置
                projmod.ShouldDrawArm = true; //自定义手臂角度
                player.itemAnimation = player.itemTime = 2; //使得玩家始终处于使用武器的状态
                projmod.TimeMax = (player.HeldItem.useTime / player.GetAttackSpeed(DamageClass.Melee)) * 2; //状态总时长，但是是一般时长的1/2
                int HoldupTimeMax = (int)(projmod.TimeMax / 3); //举剑的时长等于使用武器时间的1/3
                int WieldTimeMax = (int)projmod.TimeMax - HoldupTimeMax; //挥剑的时长等于使用武器时间的2/3
                if (projmod.WieldStuckTimer > 0) //如果卡肉
                    projmod.WieldStuckTimer--; //在计时器减少之前减少卡肉计时器
                else projmod.Timer++; //然后再减少射弹计时器
                #endregion
                #region 总时长的前1/3：举剑
                if (projmod.Timer <= HoldupTimeMax) //如果总时长小于举剑的时长
                {
                    projmod.WieldAttack = false;
                    if (!projmod.WieldDrawArmBefore)
                    {
                        if (projmod.Timer <= HoldupTimeMax / 3f)
                        {
                            projmod.DrawBehindPlayer = true;
                            projmod.ArmRotation = MathHelper.Lerp(0, (float)Math.PI, (float)(projmod.Timer / (HoldupTimeMax / 3f)));
                        }
                        else
                        {
                            projmod.TransferToSet(proj, projmod.TargetSet, (float)(projmod.Timer - HoldupTimeMax / 3f) / (2f * HoldupTimeMax / 3f), true, true);
                        }
                    }
                    else
                    {
                        projmod.TransferToSet(proj, projmod.TargetSet, (float)projmod.Timer / HoldupTimeMax, true, true);
                    }
                }
                #endregion
                #region 后2/3：挥剑
                else
                {
                    player.velocity.X = 60 * player.direction;
                    if (projmod.Timer == HoldupTimeMax + WieldTimeMax / 3) slashpos1 = player.Center;
                    if (projmod.Timer == HoldupTimeMax + WieldTimeMax * 2 / 3) slashpos2 = player.Center;
                    projmod.WieldAttack = true;
                    projmod.CouldHit = true;
                    int WieldTimer;
                    WieldTimer = projmod.SlashDrawTimer = projmod.Timer - HoldupTimeMax;
                    #region 记录初始数据
                    if (WieldTimer == 1) //当时间到了刚开始计算挥剑的位置
                    {
                        Projectile grassproj = Projectile.NewProjectileDirect(proj.GetSource_FromAI(), player.Center, Vector2.Zero, ModContent.ProjectileType<FlyingGrass>(), 0, 0, player.whoAmI);
                        projmod.IniSet.Set(projmod.ArmToSwordOffset, proj.rotation, projmod.ArmRotation, proj.scale); //重新记录Ini类原始数据
                        #region 一部分需要计算得出的数据
                        //通过挥剑标准倍率、挥剑缩小倍率、挥剑初始角度、挥剑最终角度、挥剑角度是否考虑鼠标和鼠标位置等计算得出最终的倍率大小
                        int targscaleflag = projmod.ShouldCountMouse ? 1 : 0;
                        float targscale = MeleeRevampMathHelper.EllipseRadiusHelper(projmod.WieldStandardScale, projmod.WieldStandardScale * projmod.WieldThinScale, projmod.WieldFinalRot - (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) * targscaleflag);
                        projmod.TargetSet.Set(Vector2.Zero, projmod.WieldFinalRot, projmod.WieldFinalRot - (float)Math.PI / 2, targscale);//重新记录targ数据
                        #endregion
                    }
                    #endregion
                    #region 角度、位置、大小变化，不是平滑变化
                    projmod.RotSetTargetLogistic(proj, projmod.TargetSet, projmod.Timer - HoldupTimeMax, (float)(projmod.TimeMax - HoldupTimeMax)); //角度变化
                    projmod.ArmToSwordOffset = new Vector2(-projmod.WieldHandleLength, 0).RotatedBy(proj.rotation);
                    proj.scale = MeleeRevampMathHelper.EllipseRadiusHelper(projmod.WieldStandardScale, projmod.WieldStandardScale * projmod.WieldThinScale, projmod.Projectile.rotation - (projmod.ShouldCountMouse ? (float)Math.Atan(projmod.MousePos.Y / projmod.MousePos.X) : 0));
                    projmod.ArmRotation = projmod.ArmRotation = proj.rotation - (float)Math.PI / 2;
                    projmod.WieldDrawRadius[WieldTimer] = projmod.SwordRadius; //像oldpos一样记录绘制半径
                    #endregion
                    #region 挥剑到正中间位置（也就是挥剑的1/4时间点）时发射弹幕
                    if (projmod.ShootProj != 0 && projmod.Timer == HoldupTimeMax + WieldTimeMax / 4)
                    {
                        Projectile shootproj = Projectile.NewProjectileDirect(proj.GetSource_FromThis(), player.Center, Vector2.Zero, projmod.ShootProj, (int)(proj.damage * projmod.ShootProjDamScale), proj.knockBack, Main.myPlayer);
                        shootproj.direction = player.direction;
                    }
                    #endregion
                    #region 状态机的处理
                    if (projmod.Timer >= projmod.TimeMax) //结束
                    {
                        #region 斩切射弹
                        Projectile slashproj = Projectile.NewProjectileDirect(proj.GetSource_FromAI(), slashpos1, Vector2.Zero, ModContent.ProjectileType<GrassSlash>(), proj.damage, proj.knockBack, player.whoAmI);
                        Projectile slashproj2 = Projectile.NewProjectileDirect(proj.GetSource_FromAI(), slashpos1, Vector2.Zero, ModContent.ProjectileType<GrassSlash>(), proj.damage, proj.knockBack, player.whoAmI);
                        Projectile slashproj3 = Projectile.NewProjectileDirect(proj.GetSource_FromAI(), slashpos2, Vector2.Zero, ModContent.ProjectileType<GrassSlash>(), proj.damage, proj.knockBack, player.whoAmI);
                        Projectile slashproj4 = Projectile.NewProjectileDirect(proj.GetSource_FromAI(), slashpos2, Vector2.Zero, ModContent.ProjectileType<GrassSlash>(), proj.damage, proj.knockBack, player.whoAmI);
                        slashproj.rotation = slashproj3.rotation = (float)Math.PI / 4;
                        slashproj2.rotation = slashproj4.rotation = (float)Math.PI * 3 / 4;
                        player.GetModPlayer<ScreenShake>().ScreenShakeShort(36, 0);
                        #endregion
                        player.velocity.X = 0;
                        player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGauge = 0;
                        projmod.IniSet.Set(projmod.ArmToSwordOffset, proj.rotation, projmod.ArmRotation, proj.scale); //设置iniset
                        projmod.TargetSet.Set(new Vector2(-8 * player.direction, -16), 0.5f * (float)Math.PI, player.direction * (float)Math.PI, 1.2f);
                        projmod.Timer = 0; //复原计时器
                        projmod.TimeMax = 240; //设置最大时间
                        projmod.SetState<Recover>(); //设置AI
                        return;
                    }
                    #endregion
                }
                #endregion
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
            //前两个参数没必要动，第三个是头，第四个是尾，第五个是宽度，第六个别动，这样从左下角到右上角规定哪里有碰撞伤害
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
            for (int i = 0; i <= 80; i++) //这里代表每一帧的所有拖尾，所以依旧是每一f循环完毕
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
            #region 顶点绘制配件
            //顶点绘制的固定一套语句，但是additive和alphablend有不同的适配情况
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
    public class FlyingGrass : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_976";
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 18;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 210;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(0, 7);
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.HeldItem.type != ItemID.BladeofGrass) Projectile.Kill();
            if (Projectile.Hitbox.Intersects(player.Hitbox) && Projectile.ai[0] >= 30)
            {
                player.GetModPlayer<BladeOfGrassPlayer>().Special = true;
                player.GetModPlayer<BladeOfGrassPlayer>().SpecialTimer = 6;
                Projectile.Kill();
            }
            Projectile.ai[0]++;
            Projectile.velocity.X = (float)Math.Sin(Projectile.ai[0] / 15) * 4;
            Projectile.velocity.Y = 0.2f;
            if (Projectile.ai[0] > 200)
                Projectile.alpha += 25;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D MainTex = ModContent.Request<Texture2D>(Texture).Value;
            int frameheight = MainTex.Height / 7;
            Rectangle Mainframe = new Rectangle(0, Projectile.frame * frameheight, Projectile.width, Projectile.height);
            Main.EntitySpriteDraw(MainTex, Projectile.Center - Main.screenPosition, Mainframe, Color.White * ((255 - Projectile.alpha) / 255f), 0f, Projectile.Size / 2, Projectile.scale, effects, default);
            return false;
        }
    }
    public class BladeOfGrassPlayer : ModPlayer
    {
        public bool Special = false;
        public int SpecialTimer = 0;
        public override void PostUpdateMiscEffects()
        {
            if (SpecialTimer > 0) SpecialTimer--;
            if (SpecialTimer <= 0) Special = false;
        }
    }
}
