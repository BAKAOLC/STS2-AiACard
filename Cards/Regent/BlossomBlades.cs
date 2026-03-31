using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Regent
{
    /// <summary>剑花纷飞</summary>
    public sealed class BlossomBlades() : ModCardTemplate(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<SovereignBlade>(),
            HoverTipFactory.FromCard<RegentDaggerCard>(),
        ];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.PlaceholderPortrait, Const.Paths.PlaceholderPortrait);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(CombatState);
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            var prefs = new CardSelectorPrefs(new("gameplay_ui", "CHOOSE_CARD_HEADER"), 1);
            var blade = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs,
                static c => c is SovereignBlade, this)).FirstOrDefault() as SovereignBlade;
            if (blade == null)
                return;

            var dagger = CombatState.CreateCard<RegentDaggerCard>(Owner);
            dagger.ConfigureHitsFromSovereignBaseDamage(blade.DynamicVars.Damage.BaseValue);
            await CardCmd.Transform(blade, dagger);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
