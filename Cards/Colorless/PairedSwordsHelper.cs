using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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

        internal static async Task MoveSisterToHandOrCreate<TSister>(PlayerChoiceContext ctx, Player owner, CardModel source)
            where TSister : CardModel, new()
        {
            ArgumentNullException.ThrowIfNull(owner.PlayerCombatState);
            ArgumentNullException.ThrowIfNull(owner.Creature.CombatState);
            var existing = owner.PlayerCombatState.AllCards.FirstOrDefault(c => !c.IsDupe && c is TSister);
            if (existing != null)
            {
                await CardPileCmd.Add(existing, PileType.Hand, CardPilePosition.Top, source);
                return;
            }

            var created = owner.Creature.CombatState.CreateCard(new TSister(), owner);
            await CardPileCmd.AddGeneratedCardToCombat(created, PileType.Hand, true);
        }
    }
}
