using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_AiACard.Powers;
using STS2RitsuLib.Patching.Models;

namespace STS2_AiACard.Patches
{
    /// <summary>在原版 <see cref="SwordSagePower" /> 写完段数之后，若存在剑花则改回「剑花基础 + 铸造 + 剑圣层数」。</summary>
    public sealed class BlossomBladesSwordSageAfterPowerAmountPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.blossom_blades.sword_sage_after_power_amount";

        public static string Description => "剑花纷飞：剑圣层数变化后与剑花段数合并";

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(SwordSagePower), nameof(SwordSagePower.AfterPowerAmountChanged))];
        }

        public static void Postfix(SwordSagePower __instance, PowerModel power)
        {
            BlossomBladesPower.HarmonyAfterSwordSagePowerAmountChanged(__instance, power);
        }
    }

    /// <summary>Postfix：<see cref="SwordSagePower.AfterCardEnteredCombat" />。</summary>
    public sealed class BlossomBladesSwordSageAfterCardEnteredCombatPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.blossom_blades.sword_sage_card_entered_combat";

        public static string Description => "剑花纷飞：君王之剑入场时与剑圣层数合并段数";

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(SwordSagePower), nameof(SwordSagePower.AfterCardEnteredCombat))];
        }

        public static void Postfix(SwordSagePower __instance, CardModel card)
        {
            BlossomBladesPower.HarmonyAfterSwordSageCardEnteredCombat(__instance, card);
        }
    }

    /// <summary>Postfix：<see cref="SwordSagePower.AfterRemoved" />。</summary>
    public sealed class BlossomBladesSwordSageAfterRemovedPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.blossom_blades.sword_sage_removed";

        public static string Description => "剑花纷飞：失去剑圣后按剑花规则重算段数";

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(SwordSagePower), nameof(SwordSagePower.AfterRemoved))];
        }

        public static void Postfix(SwordSagePower _, Creature oldOwner)
        {
            BlossomBladesPower.HarmonyAfterSwordSageRemoved(oldOwner);
        }
    }
}
