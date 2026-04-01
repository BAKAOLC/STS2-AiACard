using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2_AiACard.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Colorless
{
    /// <summary>异蛇契约：金卡；获得「异蛇熵契」；打出后立即随机化当前手牌耗能/辉星；之后抽上的牌同上。</summary>
    public sealed class SnakeContractCard() : ModCardTemplate(0, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromPower<SnakeContractEntropyPower>()];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.PlaceholderPortrait, Const.Paths.PlaceholderPortrait);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            if (!Owner.Creature.HasPower<SnakeContractEntropyPower>())
                await PowerCmd.Apply<SnakeContractEntropyPower>(Owner.Creature, 1, Owner.Creature, this);

            ArgumentNullException.ThrowIfNull(Owner.PlayerCombatState);
            foreach (var card in Owner.PlayerCombatState.Hand.Cards.Where(c => c.Owner == Owner))
                SnakeContractEntropyPower.ApplyRandomCosts(card, Owner);

            if (IsUpgraded)
                await CardPileCmd.Draw(choiceContext, 3, Owner);
        }
    }
}
