using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace STS2_AiACard.Powers
{
    /// <summary>隐秘形态（可见）：每回合少抽，层数随打出次数叠加。</summary>
    public sealed class VeiledFormPower : AiACardPowerBase
    {
        public override PowerType Type => PowerType.Debuff;

        public override PowerStackType StackType => PowerStackType.Counter;

        public override decimal ModifyHandDraw(Player player, decimal count)
        {
            if (player.Creature != Owner)
                return count;
            return Math.Max(0m, count - Amount);
        }
    }
}
