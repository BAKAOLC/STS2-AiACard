using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace STS2_AiACard.Cards.Colorless
{
    internal static class PairedSwordsHelper
    {
        internal static void BuffSisterDamage<TSister>(Player owner, decimal amount) where TSister : CardModel
        {
            ArgumentNullException.ThrowIfNull(owner.PlayerCombatState);
            foreach (var c in owner.PlayerCombatState.AllCards)
            {
                if (c.IsDupe || c is not TSister)
                    continue;
                c.DynamicVars.Damage.BaseValue += amount;
            }
        }

        /// <summary>
        /// 若姐妹牌已在玩家任一战斗牌堆中（抽牌/手牌/弃牌/消耗等），则置入手牌顶；否则不生成新牌。
        /// </summary>
        internal static async Task MoveSisterToHandIfOwned<TSister>(Player owner, CardModel source)
            where TSister : CardModel
        {
            ArgumentNullException.ThrowIfNull(owner.PlayerCombatState);
            var existing = owner.PlayerCombatState.AllCards.FirstOrDefault(c => !c.IsDupe && c is TSister);
            if (existing == null)
                return;

            await CardPileCmd.Add(existing, PileType.Hand, CardPilePosition.Top, source);
        }
    }
}
