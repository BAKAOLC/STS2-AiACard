using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using STS2_AiACard.Powers;
using STS2RitsuLib.Patching.Models;

namespace STS2_AiACard.Patches
{
    /// <summary>王不喜算术：辉星不足时用能量垫付；与 <see cref="KingDislikesMathPower" /> 及原版能量→辉星逻辑配合。</summary>
    internal static class KingDislikesMathResourceLogic
    {
        internal static bool HasKingMath(Player? player)
        {
            return player?.Creature.GetPower<KingDislikesMathPower>() != null;
        }

        internal static bool HasEnoughWithStarToEnergySubstitution(PlayerCombatState pcs, CardModel card)
        {
            var e = Math.Max(0, card.EnergyCost.GetWithModifiers(CostModifiers.All));
            var s = Math.Max(0, card.GetStarCostWithModifiers());
            var E = pcs.Energy;
            var S = pcs.Stars;

            if (e > E && card.CombatState != null &&
                Hook.ShouldPayExcessEnergyCostWithStars(card.CombatState, card.Owner))
            {
                s += (e - E) * 2;
                e = E;
            }

            if (s > S)
            {
                var sd = s - S;
                e += (sd + 1) / 2;
                s = S;
            }

            return e <= E && s <= S;
        }

        internal static void ApplyHasEnoughPostfix(
            PlayerCombatState __instance,
            CardModel card,
            ref UnplayableReason reason,
            ref bool __result)
        {
            if (__result)
                return;
            if (!HasKingMath(card.Owner))
                return;
            var resMask = UnplayableReason.EnergyCostTooHigh | UnplayableReason.StarCostTooHigh;
            if ((reason & resMask) == 0)
                return;
            if ((reason & ~resMask) != 0)
                return;
            if (!HasEnoughWithStarToEnergySubstitution(__instance, card))
                return;
            reason = UnplayableReason.None;
            __result = true;
        }

        internal static bool SpendResourcesPrefix(CardModel __instance, ref Task<(int, int)> __result)
        {
            if (!HasKingMath(__instance.Owner) || __instance.CombatState == null)
                return true;

            __result = SpendResourcesWithStarSubstitution(__instance);
            return false;
        }

        private static async Task<(int, int)> SpendResourcesWithStarSubstitution(CardModel card)
        {
            var pcs = card.Owner.PlayerCombatState;
            var combatState = card.CombatState;
            ArgumentNullException.ThrowIfNull(pcs);
            ArgumentNullException.ThrowIfNull(combatState);

            var energy = pcs.Energy;
            var energyToSpend = card.EnergyCost.GetAmountToSpend();
            var starsToSpend = Math.Max(0, card.GetStarCostWithModifiers());

            if (energyToSpend > energy && Hook.ShouldPayExcessEnergyCostWithStars(combatState, card.Owner))
            {
                starsToSpend += (energyToSpend - energy) * 2;
                energyToSpend = energy;
            }

            if (starsToSpend > pcs.Stars)
            {
                var starShort = starsToSpend - pcs.Stars;
                energyToSpend += (starShort + 1) / 2;
                starsToSpend = pcs.Stars;
            }

            var spendEnergy = AccessTools.DeclaredMethod(typeof(CardModel), "SpendEnergy", [typeof(int)])
                ?? throw new InvalidOperationException("CardModel.SpendEnergy(int) not found.");
            var spendStars = AccessTools.DeclaredMethod(typeof(CardModel), "SpendStars", [typeof(int)])
                ?? throw new InvalidOperationException("CardModel.SpendStars(int) not found.");

            var energyTaskObj = spendEnergy.Invoke(card, [energyToSpend]);
            var starsTaskObj = spendStars.Invoke(card, [starsToSpend]);
            if (energyTaskObj is not Task energyTask)
                throw new InvalidOperationException("SpendEnergy did not return a Task.");
            if (starsTaskObj is not Task starsTask)
                throw new InvalidOperationException("SpendStars did not return a Task.");

            await energyTask;
            await starsTask;
            return (energyToSpend, starsToSpend);
        }
    }

    /// <summary>Postfix：<see cref="PlayerCombatState.HasEnoughResourcesFor" />。</summary>
    public sealed class KingDislikesMathHasEnoughResourcesPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.king_math.has_enough_resources";

        public static string Description => "王不喜算术：辉星不足时允许用能量支付（与可打出判定一致）";

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(PlayerCombatState), nameof(PlayerCombatState.HasEnoughResourcesFor))];
        }

        public static void Postfix(
            PlayerCombatState __instance,
            CardModel card,
            ref UnplayableReason reason,
            ref bool __result)
        {
            KingDislikesMathResourceLogic.ApplyHasEnoughPostfix(__instance, card, ref reason, ref __result);
        }
    }

    /// <summary>Prefix：<see cref="CardModel.SpendResources" />。</summary>
    public sealed class KingDislikesMathSpendResourcesPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.king_math.spend_resources";

        public static string Description => "王不喜算术：扣费时应用辉星→能量换算";

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(CardModel), nameof(CardModel.SpendResources))];
        }

        public static bool Prefix(CardModel __instance, ref Task<(int, int)> __result)
        {
            return KingDislikesMathResourceLogic.SpendResourcesPrefix(__instance, ref __result);
        }
    }
}
