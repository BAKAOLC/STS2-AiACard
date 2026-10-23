using STS2RitsuLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2_AiACard.Powers;
using STS2RitsuLib.Patching.Models;

namespace STS2_AiACard.Patches
{
    internal static class VeiledFormTurnSource
    {
        private static readonly AttachedState<Player, bool> Sources = new();
        internal static bool IsVeiledTurn(Player player) => Sources.TryGetValue(player, out var veiled) && veiled;
        internal static void Capture(Player player) => Sources[player] =
            player.Creature.GetPower<VeiledFormExtraTurnPower>()?.Amount > 0;
    }

    public sealed class VeiledFormExtraTurnPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.veiled.extra_turn";
        public static string Description => "Resolve Veiled Form extra turns independently";
        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(Hook), nameof(Hook.ShouldTakeExtraTurn), [typeof(ICombatState), typeof(Player)])];
        public static void Prefix(Player player) => VeiledFormTurnSource.Capture(player);
        public static void Postfix(Player player, ref bool __result)
        {
            if (VeiledFormTurnSource.IsVeiledTurn(player))
                __result = true;
        }
    }

    public sealed class VeiledFormPaelsEyePatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.veiled.paels_eye";
        public static string Description => "Do not spend Pael's Eye on Veiled Form extra turns";
        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(PaelsEye), nameof(PaelsEye.AfterTakingExtraTurn), [typeof(Player)])];
        public static bool Prefix(Player player, ref Task __result)
        {
            if (!VeiledFormTurnSource.IsVeiledTurn(player))
                return true;
            __result = Task.CompletedTask;
            return false;
        }
    }
}
