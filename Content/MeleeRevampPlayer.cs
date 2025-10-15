using Microsoft.Xna.Framework;
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
        public bool LightsBaneImmunity;
        public override void PreUpdate()
        {
            // Detect if the player is in air by detecting if there is a tile below the player
            Point pos = (Player.Bottom / 16).ToPoint();
            PlayerInAir = !Main.tile[pos].HasTile && !Main.tile[pos].HasUnactuatedTile && Main.tile[pos].TileType == 0;
        }
        public override void ResetEffects()
        {
            StickToGround = false;
            LightsBaneImmunity = false;
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
        public override void OnHurt(Player.HurtInfo info)
        {
            if (LightsBaneImmunity)
                info.Cancelled = true;
        }
        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (LightsBaneImmunity)
            {
                r = 0f;
                g = 0f;
                b = 255f;
            }
            base.DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright);
        }
    }
}
