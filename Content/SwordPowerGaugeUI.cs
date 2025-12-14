using MeleeRevamp.Content.Items.VanillaRevamps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace MeleeRevamp.Content
{
    public class SwordPowerGaugeUI : ModSystem
    {
        //draw all gauges
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            MeleeRevampPlayer ModPlayer = Main.LocalPlayer.GetModPlayer<MeleeRevampPlayer>();
            if (!MeleeRevampConfigClient.Instance.SwordPowerGaugeDisable && Main.LocalPlayer.active && !Main.LocalPlayer.dead && ModPlayer.SwordPowerGaugeMax > 0.0f) // If the client does not disable sword power, the player is active, not dead, and has a max SwordGauge over 0
            {
                int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ruler"));
                LegacyGameInterfaceLayer EnergyGaugeUI = new("Sword Gauge UI",
                    delegate
                    {
                        DrawEnergyGauge(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.Game);
                layers.Insert(index, EnergyGaugeUI);
            }
        }
        public static void DrawEnergyGauge(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            MeleeRevampPlayer ModPlayer = Main.LocalPlayer.GetModPlayer<MeleeRevampPlayer>();

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D backTex = ModContent.Request<Texture2D>("MeleeRevamp/Content/Assets/SwordPowerGaugeBackground").Value;
            Texture2D barTex = ModContent.Request<Texture2D>("MeleeRevamp/Content/Assets/SwordPowerGaugeFill").Value;
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, true);
            Vector2 drawPos = playerCenter + new Vector2(0, -30) - Main.screenPosition;

            spriteBatch.Draw(backTex, drawPos, backTex.Frame(), Color.White, 0f, new Vector2(backTex.Width / 2, backTex.Height / 2), 1f, SpriteEffects.None, 0f);
            float chargePercent = ModPlayer.SwordPowerGauge / ModPlayer.SwordPowerGaugeMax;
            Rectangle barRect = new Rectangle(0, 0, (int)(chargePercent * barTex.Width), barTex.Height);
            spriteBatch.Draw(barTex, drawPos, barRect, Color.White, 0f, new Vector2(barTex.Width / 2, barTex.Height / 2), 1f, SpriteEffects.None, 0f);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, ModPlayer.SwordPowerGauge.ToString() + "/" + ModPlayer.SwordPowerGaugeMax.ToString(), drawPos + new Vector2(-8, -24), Color.White, 0, Vector2.Zero, Vector2.One * 0.75f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
    }
}
