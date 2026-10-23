using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2_AiACard.Utils;
using STS2RitsuLib.Patching.Models;

namespace STS2_AiACard.Patches
{
    public sealed class SmokeEscapeUiActivationPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.ui";
        public static string Description => "Hide combat controls for the escaped local player";
        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(NCombatUi), nameof(NCombatUi.Activate), [typeof(CombatState)])];
        public static void Postfix(NCombatUi __instance) => SmokeEscape.HideLocalCombatUi(__instance);
    }

    public sealed class SmokeEscapeHandInputPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.hand_input";
        public static string Description => "Prevent hand input and reactivation after escape";
        public static ModPatchTarget[] GetTargets() =>
        [
            new(typeof(NPlayerHand), nameof(NPlayerHand.AnimIn), Type.EmptyTypes),
            new(typeof(NPlayerHand), "AnimEnable", Type.EmptyTypes),
            new(typeof(NPlayerHand), nameof(NPlayerHand.EnableControllerNavigation), Type.EmptyTypes),
            new(typeof(NPlayerHand), "OnHolderPressed", [typeof(NCardHolder)]),
            new(typeof(NPlayerHand), nameof(NPlayerHand._UnhandledInput), [typeof(InputEvent)])
        ];
        public static bool Prefix() => !SmokeEscape.IsLocalEscaped;
    }

    public sealed class SmokeEscapeEndTurnEnabledPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.end_turn_enabled";
        public static string Description => "Keep the end turn button disabled after escape";
        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(NEndTurnButton), nameof(NEndTurnButton.RefreshEnabled), Type.EmptyTypes)];
        public static bool Prefix(NEndTurnButton __instance)
        {
            if (!SmokeEscape.IsLocalEscaped)
                return true;
            __instance.Disable();
            return false;
        }
    }

    public sealed class SmokeEscapeEndTurnInputPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.end_turn_input";
        public static string Description => "Prevent mouse, controller and hotkey turn actions after escape";
        public static ModPatchTarget[] GetTargets() =>
        [
            new(typeof(NEndTurnButton), nameof(NEndTurnButton.CallReleaseLogic), Type.EmptyTypes),
            new(typeof(NEndTurnButton), nameof(NEndTurnButton.SecretEndTurnLogicViaFtue), Type.EmptyTypes),
            new(typeof(NEndTurnButton), "OnPress", Type.EmptyTypes),
            new(typeof(NEndTurnButton), "OnRelease", Type.EmptyTypes)
        ];
        public static bool Prefix() => !SmokeEscape.IsLocalEscaped;
    }
}
