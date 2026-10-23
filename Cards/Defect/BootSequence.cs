using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Defect
{
    /// <summary>别吵，还在启动</summary>
    public sealed class BootSequence() : ModCardTemplate(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        protected override bool HasEnergyCostX => true;

        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Innate];

        public override bool GainsBlock => true;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new BlockVar(7m, ValueProp.Move),
            new DynamicVar("ExtraHits", 0m),
        ];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.BootSequence, Const.Paths.CardPortraits.BootSequence);

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromKeyword(CardKeyword.Retain)];

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            var x = ResolveEnergyXValue() + DynamicVars["ExtraHits"].IntValue;
            for (var i = 0; i < x; i++)
                await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
            if (x <= 0)
                return;

            var prefs = new CardSelectorPrefs(new("cards", "STS2_AI_A_CARD_CARD_BOOT_SEQUENCE.retainPrompt"), 0, x);
            var chosen = await CardSelectCmd.FromHand(choiceContext, Owner, prefs, null, this);
            foreach (var c in chosen)
                CardCmd.ApplyKeyword(c, CardKeyword.Retain);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["ExtraHits"].UpgradeValueBy(1m);
        }
    }
}
