// An easy way to apply RT2D to the game based on the method of yiyang233(Bilibili), mainly used to apply effects to limited objects or regions of the screen
// Refactored and optimized for applying groups of effects.
// TODO: Implement a Command Queue-based profiler for better pass sequence visualization.

using MeleeRevamp.Content.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace MeleeRevamp.Content.Hooks
{
    public class RT2D : ModSystem
    {
        public static RenderTarget2D renderA;
        public static RenderTarget2D renderB;
        public override void Load()
        {
            On_FilterManager.EndCapture += On_FilterManager_EndCapture;
        }
        private void On_FilterManager_EndCapture(On_FilterManager.orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
        {
            if (renderA == null)
                renderA = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            if (renderB == null)
                renderB = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight);

            Save();

            DrawWarp(); 
            ApplyWarp(); // Apply warp

            Save(); // Save the original figure

            DrawBloom();
            ApplyBloom();

            orig.Invoke(self, finalTexture, screenTarget1, screenTarget2, clearColor);
        }
        private void Save()
        {
            Main.instance.GraphicsDevice.SetRenderTarget(Main.screenTargetSwap); // Open and initialize the self created render
            Main.instance.GraphicsDevice.Clear(Color.Transparent);
            Main.spriteBatch.Begin(0, BlendState.AlphaBlend);
            Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White); // Copy what is draw to the screen
            Main.spriteBatch.End();
        }
        private void DrawBloom()
        {
            Main.instance.GraphicsDevice.SetRenderTarget(renderA); 
            Main.instance.GraphicsDevice.Clear(Color.Transparent);
            Main.spriteBatch.Begin(0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < Main.maxProjectiles; i++) 
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is IDrawBloom) 
                    (Main.projectile[i].ModProjectile as IDrawBloom).DrawBloom(); 
            Main.spriteBatch.End();
        }

        private void DrawWarp() // Apply warp to another render
        {
            Main.instance.GraphicsDevice.SetRenderTarget(renderA); // Open and initialize the alternate render provided by tmod API
            Main.instance.GraphicsDevice.Clear(Color.Transparent);
            Main.spriteBatch.Begin(0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < Main.maxProjectiles; i++) // Iterate through all projectiles
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is IDrawWarp) // If the projectile is active and has IDrawWarp interface
                    (Main.projectile[i].ModProjectile as IDrawWarp).DrawWarp(); // Apply drawwarp in the interface
            Main.spriteBatch.End();
        }
        private void ApplyBloom()
        {
            // Uses multiply RT2D to apply multipass bloom
            Effect blurEffect = Filters.Scene["PrecalcGaussBlur"].GetShader().Shader;
            Effect magnifyEffect = Filters.Scene["Magnify"].GetShader().Shader;
            Main.instance.GraphicsDevice.SetRenderTarget(Main.screenTarget);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            Main.spriteBatch.Draw(renderA, Vector2.Zero, Color.White);
            Main.spriteBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(renderA);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            blurEffect.Parameters["uSource"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            blurEffect.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            Main.spriteBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(renderB);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            magnifyEffect.Parameters["uSource"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            magnifyEffect.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(renderA, Vector2.Zero, Color.White);
            Main.spriteBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(Main.screenTarget);
            Main.instance.GraphicsDevice.Clear(Color.Transparent);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            Main.spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
            Main.spriteBatch.Draw(renderB, Vector2.Zero, Color.White);
            for (int i = 0; i < Main.maxProjectiles; i++)
                if (Main.projectile[i].active && Main.projectile[i].ModProjectile is IDrawBloom)
                    (Main.projectile[i].ModProjectile as IDrawBloom).DrawBloom();
            Main.spriteBatch.End();
        }

        private void ApplyWarp()
        {
            Main.instance.GraphicsDevice.SetRenderTarget(Main.screenTarget); // Open the main render
            Main.instance.GraphicsDevice.Clear(Color.Transparent);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            // Set the graph of shader to noise map used in swap
            Effect effect = Filters.Scene["Warp"].GetShader().Shader;
            effect.Parameters["tex"].SetValue(renderA);
            effect.Parameters["intense"].SetValue(0.04f);
            effect.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White); // Draw the content in the previously stored render to the render Main.screenTarget
            Main.spriteBatch.End();
        }
        public override void Unload()
        {
            renderB = null;
            renderA = null;
        }
    }
}
