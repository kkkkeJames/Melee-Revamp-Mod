using Microsoft.Xna.Framework;
using System.Reflection.Metadata.Ecma335;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MeleeRevamp.Content
{
    public class MeleeRevampPlayer : ModPlayer
    {
        #region Sword Power Variables
        public float SwordPowerGaugeMax = 0; // Current max sword gauge
        public float SwordPowerGauge = 0; // Current sword gauge
        #endregion
        public bool StickToGround;
        public bool PlayerInAir; // If the player is in air
        public float LightsBaneAlpha;
        public bool Invulnerable;
        public override void PreUpdate()
        {
            // Detect if the player is in air by detecting if there is a tile below the player
            Point pos = (Player.Bottom / 16).ToPoint();
            PlayerInAir = !Main.tile[pos].HasTile && !Main.tile[pos].HasUnactuatedTile && Main.tile[pos].TileType == 0;
        }
        public override void OnRespawn()
        {
            Invulnerable = false;
            base.OnRespawn();
        }
        public override void ResetEffects()
        {
            StickToGround = false;
            LightsBaneAlpha = 1f;
        }
        public override void SetControls()
        {
            if (StickToGround)
            {
                Player.controlJump = false;
                Player.controlDown = false;
                Player.controlHook = false;
                Player.stairFall = false;
            }
        }
        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            a *= LightsBaneAlpha;
            r *= LightsBaneAlpha;
            g *= LightsBaneAlpha;
            b *= LightsBaneAlpha;
            base.DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright);
        }
        public override bool CanBeHitByProjectile(Projectile proj)
        {
            if (Invulnerable) return false;
            else return base.CanBeHitByProjectile(proj);
        }
        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
        {
            if (Invulnerable) return false;
            else return base.CanBeHitByNPC(npc, ref cooldownSlot);
        }
    }
    public class MeleeRevampScreenPlayer : ModPlayer
    {
        public Vector2 ZoomVector = Vector2.Zero;
        public float ZoomTime = 0;
        public float ZoomTimeMax = 0; 
        public bool ZoomReverse = false;
        public void TimedZoom(Vector2 zoomVector, float zoomTime, float zoomTimeMax, bool zoomReverse = false)
        {
            ZoomVector = zoomVector;
            ZoomTime= zoomTime;
            ZoomTimeMax = zoomTimeMax;
            ZoomReverse = zoomReverse;
        }
        public void ClearZoomVector()
        {
            ZoomVector = Vector2.Zero;
            ZoomTime = 0;
            ZoomTimeMax = 0;
            ZoomReverse = false;
        }
    }
}
