using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Defect
{
    /// <summary>别吵，还在启动</summary>
    public sealed class BootSequence() : ModCardTemplate(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        protected override bool HasEnergyCostX => true;

        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public override bool GainsBlock => true;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new CalculationBaseVar(0m),
            new CalculationExtraVar(3m),
            new CalculatedBlockVar(ValueProp.Move).WithMultiplier(static (card, _) =>
            {
                if (!CombatManager.Instance.IsInProgress || card.CombatState == null)
                    return 0m;
                return card.EnergyCost.CapturedXValue;
            }),
        ];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.BootSequence, Const.Paths.CardPortraits.BootSequence);

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromKeyword(CardKeyword.Retain)];

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            var x = ResolveEnergyXValue();
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.CalculatedBlock.Calculate(cardPlay.Target),
                ValueProp.Move,
                cardPlay);
            if (x <= 0)
                return;

            var prefs = new CardSelectorPrefs(new("gameplay_ui", "CHOOSE_CARD_HEADER"), x);
            var chosen = await CardSelectCmd.FromHand(choiceContext, Owner, prefs, null, this);
            foreach (var c in chosen)
                CardCmd.ApplyKeyword(c, CardKeyword.Retain);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.CalculationExtra.UpgradeValueBy(2m);
        }
    }
}
