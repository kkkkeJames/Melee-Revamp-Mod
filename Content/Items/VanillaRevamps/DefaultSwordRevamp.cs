using MeleeRevamp.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.UI.Elements.Base;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MeleeRevamp.Content.Items.VanillaRevamps
{
    public class DefaultBroadswordRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return MeleeRevampSwordsHelper.Broadswords[entity.type];
        }
        public override void SetDefaults(Item item)
        {
            if (item.type == ItemID.Katana) 
                item.useTime = item.useAnimation = 20;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useStyle = ItemUseStyleID.Shoot;
            item.channel = true;
            item.autoReuse = false;
        }
        public override void HoldItem(Item item, Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<DefaultBroadswordSlash>()] < 1)
                Projectile.NewProjectileDirect(player.GetSource_ItemUse(item), player.Center, Vector2.Zero, ModContent.ProjectileType<DefaultBroadswordSlash>(), item.damage, item.knockBack, player.whoAmI, 0, 0, item.type);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
    }
    public class DefaultBroadswordSlash : GlobalSwordSlash
    {
        public override string Texture => "Terraria/Images/Item_" + Projectile.ai[2];
        public static Asset<Texture2D> tex;
        static float ColorDistance(Color c1, Color c2)
        {
            float dr = c1.R - c2.R;
            float dg = c1.G - c2.G;
            float db = c1.B - c2.B;
            return dr * dr + dg * dg + db * db;
        }
        public override void RegisterVariables()
        {
            tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad);
            // A foolish box filter to get the average color from the texture
            Vector3 ColorSum = new Vector3(0, 0, 0);
            int TexturePixels = 0;
            Color[] pixel = new Color[tex.Width() * tex.Height()];
            List<Color> palette = new List<Color>();
            tex.Value.GetData(pixel);
            for (int i = 0; i < pixel.Length; ++i)
            {
                if (pixel[i].A > 0 && ((pixel[i].R != 0) || (pixel[i].G != 0) || (pixel[i].B != 0)))
                {
                    TexturePixels++;
                    ColorSum += new Vector3(pixel[i].R, pixel[i].G, pixel[i].B);
                    bool flag = true;
                    for (int j = 0; j < palette.Count; ++j)
                        if (palette[j] == pixel[i]) flag = false;
                    if (flag)
                        palette.Add(pixel[i]);
                }
            }
            ColorSum /= TexturePixels;
            Color tempColor = new Color(ColorSum.X / 255f, ColorSum.Y / 255f, ColorSum.Z / 255f);
            Color Final = Color.White;
            float dist = float.PositiveInfinity;
            for (int j = 0; j < palette.Count; ++j)
            {
                float tempdist = ColorDistance(tempColor, palette[j]);
                if (tempdist < dist)
                {
                    Final = palette[j];
                    dist = tempdist;
                }
            }
            SlashColor = Final;
            MaxComboCount = 3;
        }
        public override void Appear()
        {
            return;
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
                        ((DefaultBroadswordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -1.9f, 1.9f, 0.2f, 0f, true, 6f);
                        break;
                    case 1:
                        ((DefaultBroadswordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.8f, 1.8f, -1.7f, 0.2f, 0f, true, 6f);
                        break;
                    case 2:
                        ((DefaultBroadswordSlash)Projectile.ModProjectile).SetState<Wield>(true, 2f, 0.7f, -2.5f, 2.3f, 0.4f, 0.2f, true, 6f, true, true, 0f, 1.2f);
                        break;
                }
            }
        }
    }
}
