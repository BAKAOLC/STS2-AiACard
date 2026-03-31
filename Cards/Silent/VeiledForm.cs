using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_AiACard.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Silent
{
    /// <summary>隐秘形态</summary>
    public sealed class VeiledForm() : ModCardTemplate(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => [];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips
        {
            get
            {
                if (IsUpgraded)
                {
                    return
                    [
                        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
                        HoverTipFactory.FromPower<DrawCardsNextTurnPower>(),
                    ];
                }

                return [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];
            }
        }

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.PlaceholderPortrait, Const.Paths.PlaceholderPortrait);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            foreach (var c in PileType.Hand.GetPile(Owner).Cards.ToList())
                if (c != this)
                    await CardCmd.Exhaust(choiceContext, c);

            await PowerCmd.Apply<VeiledFormPower>(Owner.Creature, 1, Owner.Creature, this);
            await PowerCmd.Apply<VeiledFormExtraTurnPower>(Owner.Creature, 1, Owner.Creature, this);
            if (IsUpgraded)
                await PowerCmd.Apply<DrawCardsNextTurnPower>(Owner.Creature, 2, Owner.Creature, this);
        }
    }
}
