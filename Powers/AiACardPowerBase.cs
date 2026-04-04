using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Powers
{
    public abstract class AiACardPowerBase : ModPowerTemplate
    {
        public override PowerAssetProfile AssetProfile =>
            this switch
            {
                BlossomBladesPower => Icons(Const.Paths.PowerIcons.BlossomBlades),
                HeavenGiftPower => Icons(Const.Paths.PowerIcons.HeavenGift),
                KingDislikesMathPower => Icons(Const.Paths.PowerIcons.KingDislikesMath),
                ManipulationRealityPower => Icons(Const.Paths.PowerIcons.ManipulationReality),
                PlagueResonanceDexterityDownPower => Icons(Const.Paths.PowerIcons.PlagueResonance),
                PostCombatHealPower => Icons(Const.Paths.PowerIcons.PostCombatHeal),
                RequiemEnergyPower => Icons(Const.Paths.PowerIcons.RequiemEnergy),
                SnakeContractEntropyPower => Icons(Const.Paths.PowerIcons.SnakeContractEntropy),
                SnakeEloPower => Icons(Const.Paths.PowerIcons.SnakeElo),
                SwordPracticePower => Icons(Const.Paths.PowerIcons.SwordPractice),
                VeiledFormPower => Icons(Const.Paths.PowerIcons.VeiledForm),
                VeiledFormExtraTurnPower => Icons(Const.Paths.PowerIcons.VeiledForm),
                _ => Icons(Const.Paths.PlaceholderPortrait),
            };

        protected static PowerAssetProfile Icons(string iconPath, string? bigIconPath = null)
        {
            return new(iconPath, bigIconPath ?? iconPath);
        }
    }
}
