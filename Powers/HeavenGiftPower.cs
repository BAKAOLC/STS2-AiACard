using MegaCrit.Sts2.Core.Entities.Powers;

namespace STS2_AiACard.Powers
{
    /// <summary>「天降恩赐」：层数对应该玩家本场战斗结算中的额外金币；金币在战斗奖励阶段领取。持有者死亡时不在死亡清算中移除此能力。</summary>
    public sealed class HeavenGiftPower : AiACardPowerBase
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;

        /// <summary>每次打出「天降恩赐」各为独立实例，层数不在同一图标上合并。</summary>
        public override bool IsInstanced => true;

        public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;
    }
}
