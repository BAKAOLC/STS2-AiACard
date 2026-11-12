using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
#if STS2_AT_LEAST_0_113_0
using MegaCrit.Sts2.Core.Models;
#endif
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Utils;
using STS2_AiACard.Powers;

namespace STS2_AiACard.Patches
{
    internal static class VeiledFormTurnSource
    {
        private static readonly AttachedState<Player, bool> Sources = new();

        internal static bool IsVeiledTurn(Player player)
        {
            return Sources.TryGetValue(player, out var veiled) && veiled;
        }

        internal static void Capture(Player player)
        {
            Sources[player] =
                player.Creature.GetPower<VeiledFormExtraTurnPower>()?.Amount > 0;
        }
    }

    public sealed class VeiledFormExtraTurnPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.veiled.extra_turn";
        public static string Description => "Resolve Veiled Form extra turns independently";

        public static ModPatchTarget[] GetTargets() =>
#if STS2_AT_LEAST_0_113_0
        [
            new(typeof(Hook), nameof(Hook.ShouldTakeExtraTurn),
                [typeof(ICombatState), typeof(Player), typeof(AbstractModel).MakeByRefType()]),
        ];

        // ReSharper disable once InconsistentNaming
        public static bool Prefix(Player player, ref AbstractModel? source, ref bool __result)
        {
            VeiledFormTurnSource.Capture(player);
            if (!VeiledFormTurnSource.IsVeiledTurn(player))
                return true;
            source = player.Creature.GetPower<VeiledFormExtraTurnPower>();
            __result = true;
            return false;
        }
#else
            [new(typeof(Hook), nameof(Hook.ShouldTakeExtraTurn), [typeof(ICombatState), typeof(Player)])];
        public static void Prefix(Player player) => VeiledFormTurnSource.Capture(player);
#endif
        // ReSharper disable once InconsistentNaming
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

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(PaelsEye), nameof(PaelsEye.AfterTakingExtraTurn), [typeof(Player)])];
        }

        // ReSharper disable once InconsistentNaming
        public static bool Prefix(Player player, ref Task __result)
        {
            if (!VeiledFormTurnSource.IsVeiledTurn(player))
                return true;
            __result = Task.CompletedTask;
            return false;
        }
    }
}
