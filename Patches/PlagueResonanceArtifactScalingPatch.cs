using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_AiACard.Cards.Silent;
using STS2RitsuLib.Patching.Models;

namespace STS2_AiACard.Patches
{
    /// <summary>荒疫共鸣给予的人工制品使用卡面层数，不套用原版敌方人工制品多人缩放。</summary>
    public sealed class PlagueResonanceArtifactScalingPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.plague_resonance.artifact_scaling";

        public static string Description => "荒疫共鸣：人工制品不随多人玩家数放大";

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(ArtifactPower), nameof(ArtifactPower.GetScaledAmountForMultiplayer)),
            ];
        }

        public static bool Prefix(
            ICombatState combatState,
            Creature? applier,
            decimal amount,
            Creature target,
            CardModel? cardSource,
            ref decimal __result)
        {
            if (cardSource is not PlagueResonance) return true;

            __result = amount;
            return false;
        }
    }
}
