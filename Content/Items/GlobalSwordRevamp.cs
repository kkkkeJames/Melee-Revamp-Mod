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
using Terraria.Localization;
using Terraria.ModLoader;

namespace MeleeRevamp.Content.Items.VanillaRevamps
{
    public class GlobalSwordRevamp : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return MeleeRevampSwordsHelper.Broadswords[entity.type] || MeleeRevampSwordsHelper.RevampedBroadswords[entity.type];
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(item, tooltips);
            float SPMvalue = MeleeRevampSwordsHelper.Broadswords[item.type] ? MeleeRevampSwordsHelper.SwordGaugeDefaultMax : MeleeRevampSwordsHelper.SwordGaugeMaxValues[item.type];
            TooltipLine line1 = new(Mod, "Tip1", Language.GetTextValue("Mods.MeleeRevamp.GenericTooltips.Broadswords.Tip1", SPMvalue)) { OverrideColor = Color.Gold };
            TooltipLine line2 = new(Mod, "Tip2", Language.GetTextValue("Mods.MeleeRevamp.GenericTooltips.Broadswords.Tip2")) { OverrideColor = Color.Gold };
            TooltipLine line3 = new(Mod, "Tip3", Language.GetTextValue("Mods.MeleeRevamp.GenericTooltips.Broadswords.Tip3")) { OverrideColor = Color.Gold };

            int insertIndex = tooltips.FindIndex(t => t.Name == "Tooltip0");
            if (insertIndex != -1)
            {
                tooltips.Insert(insertIndex + 1, line1);
                tooltips.Insert(insertIndex + 2, line2);
                tooltips.Insert(insertIndex + 3, line3);
            }
            else
            {
                tooltips.Add(line1);
                tooltips.Add(line2);
                tooltips.Add(line3);
            }
        }
        public override bool CanUseItem(Item item, Player player)
        {
            return false;
        }
        public override void HoldItem(Item item, Player player)
        {
            if (MeleeRevampSwordsHelper.Broadswords[item.type])
                player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax = MeleeRevampSwordsHelper.SwordGaugeDefaultMax;
            else player.GetModPlayer<MeleeRevampPlayer>().SwordPowerGaugeMax = MeleeRevampSwordsHelper.SwordGaugeMaxValues[item.type];
        }
    }
}
