using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;

namespace STS2_AiACard.Powers
{
    /// <summary>熟能生巧：仅占位能力，实际升级君王之剑在打出能力牌时处理。</summary>
    public sealed class SwordPracticePower : AiACardPowerBase
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Single;

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromCard<SovereignBlade>()];
    }
}
