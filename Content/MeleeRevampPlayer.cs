using Terraria;
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
        public override void ResetEffects()
        {
            StickToGround = false;
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
    }
}
