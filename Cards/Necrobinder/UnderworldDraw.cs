using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Necrobinder
{
    /// <summary>冥域抽取</summary>
    public sealed class UnderworldDraw() : ModCardTemplate(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        protected override bool HasEnergyCostX => true;

        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.PlaceholderPortrait, Const.Paths.PlaceholderPortrait);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            var exhaust = PileType.Exhaust.GetPile(Owner).Cards.ToList();
            if (exhaust.Count == 0)
                return;

            var x = ResolveEnergyXValue();
            var take = Math.Min(x, exhaust.Count);
            var prefs = new CardSelectorPrefs(new("gameplay_ui", "CHOOSE_CARD_HEADER"), take);
            var picked = await CardSelectCmd.FromSimpleGrid(choiceContext, exhaust, Owner, prefs);
            var dest = IsUpgraded ? PileType.Hand : PileType.Discard;
            foreach (var c in picked)
                await CardPileCmd.Add(c, dest);
        }
    }
}
