using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Powers
{
    public abstract class AiACardPowerBase : ModPowerTemplate
    {
        public override PowerAssetProfile AssetProfile => Icons(Const.Paths.PlaceholderPortrait);

        protected static PowerAssetProfile Icons(string iconPath, string? bigIconPath = null)
        {
            return new(iconPath, bigIconPath ?? iconPath);
        }
    }
}
