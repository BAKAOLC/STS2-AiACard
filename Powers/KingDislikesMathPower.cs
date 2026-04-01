using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace STS2_AiACard.Powers
{
    /// <summary>王不喜算术：能量不足可用辉星代付（2 辉星/1 能量）；辉星不足可用能量代付（1 能量/2 辉星，向上取整）。</summary>
    public sealed class KingDislikesMathPower : AiACardPowerBase
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Single;

        public override bool ShouldPayExcessEnergyCostWithStars(Player player) =>
            Owner.Player == player;
    }
}
