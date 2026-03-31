using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;

namespace STS2_AiACard.Powers
{
    /// <summary>
    ///     隐藏计数：Amount 含 1 点基准；本场每枚冰霜球成功入队时 +1。凛冬打击的命中段数等于本能力的 Amount。
    /// </summary>
    public sealed class WinterFrostGenTrackerPower : AiACardPowerBase
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;

        protected override bool IsVisibleInternal => false;

        public override async Task AfterOrbChanneled(PlayerChoiceContext choiceContext, Player player, OrbModel orb)
        {
            if (player.Creature != Owner || orb is not FrostOrb)
                return;
            await PowerCmd.ModifyAmount(this, 1, Owner, null);
        }
    }
}
