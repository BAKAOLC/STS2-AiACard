using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2_AiACard.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Regent
{
    /// <summary>王不喜算术：1 能 3 辉；升级后固有。</summary>
    public sealed class KingDislikesMathCard() : ModCardTemplate(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        public override int CanonicalStarCost => 3;

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromPower<KingDislikesMathPower>()];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.KingDislikesMath, Const.Paths.CardPortraits.KingDislikesMath);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            if (!Owner.Creature.HasPower<KingDislikesMathPower>())
                await PowerCmd.Apply<KingDislikesMathPower>(Owner.Creature, 1, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            AddKeyword(CardKeyword.Innate);
        }
    }
}
