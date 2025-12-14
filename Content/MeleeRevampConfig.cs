using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace MeleeRevamp.Content
{
    public class MeleeRevampConfigClient : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        public static MeleeRevampConfigClient Instance;

        public bool SwordPowerGaugeDisable;

        public bool CameraLockDisable;

        [Range(0f, 1f)]
        [DefaultValue(1f)]
        [Slider]
        public float ShakeIntensity;
    }
}
