using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace STS2_AiACard.Powers
{
    /// <summary>
    ///     隐藏：仅用于触发一次额外回合，在 <see cref="AfterTakingExtraTurn" /> 后移除。
    /// </summary>
    public sealed class VeiledFormExtraTurnPower : AiACardPowerBase
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;

        protected override bool IsVisibleInternal => false;

        public override bool ShouldTakeExtraTurn(Player player)
        {
            return player.Creature == Owner && Amount > 0;
        }

        public override async Task AfterTakingExtraTurn(Player player)
        {
            if (player.Creature != Owner)
                return;
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), this, -1m, Owner, null);
        }
    }
}
