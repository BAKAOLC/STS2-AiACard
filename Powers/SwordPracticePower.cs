using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace STS2_AiACard.Powers
{
    /// <summary>熟能生巧：本场战斗中后续入场的君王之剑也会在入场时升级。</summary>
    public sealed class SwordPracticePower : AiACardPowerBase
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Single;

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromCard<SovereignBlade>()];

        public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
        {
            if (card.IsDupe || card is not SovereignBlade blade || blade.Owner != Owner.Player)
                return Task.CompletedTask;

            if (!blade.IsUpgradable)
                return Task.CompletedTask;

            CardCmd.Upgrade(blade);
            return Task.CompletedTask;
        }
    }
}
