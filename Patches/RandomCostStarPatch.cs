using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_AiACard.Powers;
using STS2RitsuLib.Patching.Models;

namespace STS2_AiACard.Patches
{
    public sealed class RandomCostStarPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.random_cost.stars";
        public static string Description => "Use the same bounded star costs for Confused and Slither";
        public static ModPatchTarget[] GetTargets() =>
        [
            new(typeof(ConfusedPower), nameof(ConfusedPower.AfterCardDrawn),
                [typeof(PlayerChoiceContext), typeof(CardModel), typeof(bool)]),
            new(typeof(Slither), nameof(Slither.AfterCardDrawn),
                [typeof(PlayerChoiceContext), typeof(CardModel), typeof(bool)])
        ];
        public static void Postfix(AbstractModel __instance, CardModel card)
        {
            var applies = __instance switch
            {
                ConfusedPower power => card.Owner?.Creature == power.Owner,
                Slither slither => slither.Card == card && card.Pile?.Type == PileType.Hand,
                _ => false
            };
            if (applies && card.Owner != null)
                SnakeContractEntropyPower.RandomizeStarCost(card, card.Owner);
        }
    }
}
