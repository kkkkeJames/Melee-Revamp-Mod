using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;

namespace MeleeRevamp.Content.Dusts
{
    public class PixelatedGlow : ModDust
    {
        public override string Texture => "MeleeRevamp/Content/Assets/GlowDust";
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.95f;

            dust.alpha += 5;

            dust.alpha = (int)(dust.alpha * 1.01f);
            dust.scale *= 0.965f;

            if (dust.alpha >= 255)
                dust.active = false;

            return false;
        }
        public override bool PreDraw(Dust dust)
        {
            float lerper = 1f - dust.alpha / 255f;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, dust.color * lerper, dust.rotation, tex.Size() / 2f, dust.scale * lerper, 0f, 0f);
            float glowScale = dust.scale * 0.5f;
            Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * lerper, dust.rotation, tex.Size() / 2f, glowScale * lerper, 0f, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            return false;
        }
    }
}
