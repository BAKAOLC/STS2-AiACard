using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2_AiACard.Cards.Necrobinder;

namespace STS2_AiACard.Powers
{
    public sealed class RequiemEnergyPower : AiACardPowerBase
    {
        private const int TriggerEnergyThreshold = 3;
        private const int EnergyGainPerStack = 1;

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new EnergyVar(TriggerEnergyThreshold), new EnergyVar("RequiemGain", EnergyGainPerStack)];

        public override Task AfterApplied(Creature? applier, CardModel? cardSource)
        {
            SyncLocVarsFromAmount();
            return Task.CompletedTask;
        }

        public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
            decimal amount, Creature? applier, CardModel? cardSource)
        {
            if (power == this)
                SyncLocVarsFromAmount();

            return Task.CompletedTask;
        }

        public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner?.Creature != Owner)
                return Task.CompletedTask;

            var ec = cardPlay.Card.EnergyCost;
            var spent = ec.CostsX ? ec.CapturedXValue : ec.GetWithModifiers(CostModifiers.All);
            var threshold = DynamicVars.Energy.IntValue;
            if (spent < threshold || Owner.Player == null)
                return Task.CompletedTask;

            var triggeringStacks = cardPlay.Card is RequiemCard ? Amount - 1 : Amount;
            if (triggeringStacks <= 0)
                return Task.CompletedTask;

            return PlayerCmd.GainEnergy(EnergyGainPerStack * triggeringStacks, Owner.Player);
        }

        private void SyncLocVarsFromAmount()
        {
            DynamicVars["RequiemGain"].BaseValue = EnergyGainPerStack * Amount;
            InvokeDisplayAmountChanged();
        }
    }
}
