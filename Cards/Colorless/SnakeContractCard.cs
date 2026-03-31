using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Colorless
{
    /// <summary>异蛇契约：获得原版「混乱」能力；升级后额外抽 3 张牌。</summary>
    public sealed class SnakeContractCard() : ModCardTemplate(0, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromPower<ConfusedPower>()];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.PlaceholderPortrait, Const.Paths.PlaceholderPortrait);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            if (!Owner.Creature.HasPower<ConfusedPower>())
                await PowerCmd.Apply<ConfusedPower>(Owner.Creature, 1, Owner.Creature, this);
            if (IsUpgraded)
                await CardPileCmd.Draw(choiceContext, 3, Owner);
        }
    }
}
