// TODO : Debug and upgrade screenshake effects
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MeleeRevamp.Content.Core
{
    public class ScreenShake : ModPlayer
    {
        public float ScreenshakeIntensity = 0f;
        public float ScreenshakeAngle = 0f;
        public void ScreenShakeShort(float intensity, float angle = 0f)
        {
            ScreenshakeIntensity = intensity; 
            ScreenshakeAngle = angle;
        }

        public override void ModifyScreenPosition()
        {
            Main.screenPosition += new Vector2(ScreenshakeIntensity, 0).RotatedBy(ScreenshakeAngle);
            if (ScreenshakeIntensity > 0.3f)
                ScreenshakeIntensity *= 0.7f;
            else ScreenshakeIntensity = 0f;
        }

    }
}

