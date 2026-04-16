using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Necrobinder
{
    /// <summary>游梦回魂</summary>
    public sealed class DreamReturn() : ModCardTemplate(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new SummonVar(5m)];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.Static(StaticHoverTip.SummonDynamic, DynamicVars.Summon),
            HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
        ];

        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.DreamReturn, Const.Paths.CardPortraits.DreamReturn);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(Owner.PlayerCombatState);
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            await OstyCmd.Summon(choiceContext, Owner, DynamicVars.Summon.BaseValue, this);
            foreach (var c in Owner.PlayerCombatState.DiscardPile.Cards.ToList()
                         .Where(static x => x.Keywords.Contains(CardKeyword.Ethereal)))
            {
                await CardPileCmd.Add(c, PileType.Hand);
                c.EnergyCost.AddThisTurnOrUntilPlayed(-1, true);
            }
        }

        protected override void OnUpgrade()
        {
            RemoveKeyword(CardKeyword.Exhaust);
        }
    }
}
