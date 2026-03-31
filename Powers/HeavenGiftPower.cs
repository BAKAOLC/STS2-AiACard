using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace STS2_AiACard.Powers
{
    public sealed class HeavenGiftPower : AiACardPowerBase
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;

        public override Task AfterCombatEnd(CombatRoom room)
        {
            if (Owner.Player == null)
                return Task.CompletedTask;
            return PlayerCmd.GainGold(Amount, Owner.Player);
        }
    }
}
