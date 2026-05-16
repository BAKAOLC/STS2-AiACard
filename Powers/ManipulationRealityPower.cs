using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace STS2_AiACard.Powers
{
    public sealed class ManipulationRealityPower : AiACardPowerBase
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Single;

        public override Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
        {
            if (!addedByPlayer || card.Owner?.Creature != Owner || !card.IsUpgradable)
                return Task.CompletedTask;
            CardCmd.Upgrade(card);
            return Task.CompletedTask;
        }
    }
}
