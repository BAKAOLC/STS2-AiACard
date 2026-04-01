using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Necrobinder
{
    /// <summary>冥域抽取</summary>
    public sealed class UnderworldDraw() : ModCardTemplate(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        protected override bool HasEnergyCostX => true;

        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.PlaceholderPortrait, Const.Paths.PlaceholderPortrait);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            var x = ResolveEnergyXValue();
            if (x <= 0)
                return;

            var exhaust = PileType.Exhaust.GetPile(Owner).Cards.ToList();
            if (exhaust.Count == 0)
                return;

            var take = Math.Min(x, exhaust.Count);
            if (take <= 0)
                return;

            var prefs = new CardSelectorPrefs(new("gameplay_ui", "CHOOSE_CARD_HEADER"), take, take);
            var picked = await CardSelectCmd.FromSimpleGrid(choiceContext, exhaust, Owner, prefs);
            foreach (var c in picked)
                await CardPileCmd.Add(c, PileType.Discard);
        }

        protected override void OnUpgrade()
        {
            RemoveKeyword(CardKeyword.Exhaust);
        }
    }
}
