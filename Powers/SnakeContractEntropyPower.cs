using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2_AiACard.Powers
{
    /// <summary>
    /// 异蛇契约：与「混乱」类似，但额外将带固定辉星消耗的牌在本场战斗内的辉星消耗随机为 0～4。
    /// </summary>
    public sealed class SnakeContractEntropyPower : AiACardPowerBase
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Single;

        public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card.Owner != Owner.Player)
                return Task.CompletedTask;
            ApplyRandomCosts(card, Owner.Player);
            return Task.CompletedTask;
        }

        /// <summary>随机化能量（0～3，与混乱一致）及固定辉星消耗（0～4，不含辉星 X）。</summary>
        public static void ApplyRandomCosts(CardModel card, Player player)
        {
            var rng = player.RunState.Rng.CombatEnergyCosts;
            var changed = false;

            if (card.EnergyCost.Canonical >= 0)
            {
                card.EnergyCost.SetThisCombat(rng.NextInt(4));
                changed = true;
            }

            if (!card.HasStarCostX && card.CanonicalStarCost > 0)
            {
                card.SetStarCostThisCombat(rng.NextInt(5));
                changed = true;
            }

            if (changed)
                NCard.FindOnTable(card)?.PlayRandomizeCostAnim();
        }
    }
}
