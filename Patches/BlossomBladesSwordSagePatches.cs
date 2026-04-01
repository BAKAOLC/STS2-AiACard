using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_AiACard;
using STS2_AiACard.Powers;
using STS2RitsuLib.Patching.Models;

namespace STS2_AiACard.Patches
{
    /// <summary>剑花存在时 Prefix 跳过原版改段数，Postfix 用剑花公式刷新。</summary>
    public sealed class BlossomBladesSwordSageAfterPowerAmountPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.blossom_blades.sword_sage_after_power_amount";

        public static string Description => "剑花纷飞：剑圣层数变化时按剑花公式刷新段数（跳过原版 SetRepeats）";

        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(SwordSagePower), nameof(SwordSagePower.AfterPowerAmountChanged))];

        public static bool Prefix(SwordSagePower __instance, PowerModel power) =>
            BlossomBladesPower.HarmonyPrefixSwordSageAfterPowerAmountChanged(__instance, power);

        public static void Postfix(SwordSagePower __instance, PowerModel power) =>
            BlossomBladesPower.HarmonyAfterSwordSagePowerAmountChanged(__instance, power);
    }

    /// <summary>Prefix + Postfix：<see cref="SwordSagePower.AfterCardEnteredCombat" />。</summary>
    public sealed class BlossomBladesSwordSageAfterCardEnteredCombatPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.blossom_blades.sword_sage_card_entered_combat";

        public static string Description => "剑花纷飞：君王之剑入场时按剑花公式刷新段数";

        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(SwordSagePower), nameof(SwordSagePower.AfterCardEnteredCombat))];

        public static bool Prefix(SwordSagePower __instance, CardModel card) =>
            BlossomBladesPower.HarmonyPrefixSwordSageAfterCardEnteredCombat(__instance, card);

        public static void Postfix(SwordSagePower __instance, CardModel card) =>
            BlossomBladesPower.HarmonyAfterSwordSageCardEnteredCombat(__instance, card);
    }

    /// <summary>Prefix + Postfix：<see cref="SwordSagePower.AfterRemoved" />。</summary>
    public sealed class BlossomBladesSwordSageAfterRemovedPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.blossom_blades.sword_sage_removed";

        public static string Description => "剑花纷飞：失去剑圣后按剑花规则重算段数";

        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(SwordSagePower), nameof(SwordSagePower.AfterRemoved))];

        public static bool Prefix(SwordSagePower __instance, Creature oldOwner) =>
            BlossomBladesPower.HarmonyPrefixSwordSageAfterRemoved(oldOwner);

        public static void Postfix(SwordSagePower __instance, Creature oldOwner) =>
            BlossomBladesPower.HarmonyAfterSwordSageRemoved(oldOwner);
    }
}
