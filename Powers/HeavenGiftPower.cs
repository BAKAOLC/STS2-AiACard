using MegaCrit.Sts2.Core.Entities.Powers;

namespace STS2_AiACard.Powers
{
    public sealed class HeavenGiftPower : AiACardPowerBase
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;

        /// <summary>每次打出「天降恩赐」各为独立实例，数值不合并到同一层。</summary>
        public override bool IsInstanced => true;
    }
}
