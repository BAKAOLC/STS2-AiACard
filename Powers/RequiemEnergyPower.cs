using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace STS2_AiACard.Powers
{
    public sealed class RequiemEnergyPower : AiACardPowerBase
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new EnergyVar(2), new EnergyVar("RequiemGain", 1)];

        public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner?.Creature != Owner)
                return Task.CompletedTask;

            var ec = cardPlay.Card.EnergyCost;
            var spent = ec.CostsX ? ec.CapturedXValue : ec.GetWithModifiers(CostModifiers.All);
            var threshold = DynamicVars.Energy.IntValue;
            if (spent >= threshold && Owner.Player != null)
                return PlayerCmd.GainEnergy(DynamicVars["RequiemGain"].IntValue, Owner.Player);

            return Task.CompletedTask;
        }
    }
}
