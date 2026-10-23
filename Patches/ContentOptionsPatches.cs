using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using STS2_AiACard.Content;
using STS2RitsuLib.Patching.Models;

namespace STS2_AiACard.Patches
{
    public sealed class ContentOptionsCardPoolPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.content.card_pool";
        public static string Description => "Filter disabled card offers";
        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(CardPoolModel), nameof(CardPoolModel.GetUnlockedCards),
                [typeof(UnlockState), typeof(CardMultiplayerConstraint)])];
        public static void Postfix(ref IEnumerable<CardModel> __result) =>
            __result = __result.Where(ContentOptions.IsEnabled);
    }

    public sealed class ContentOptionsGenerationPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.content.generation";
        public static string Description => "Filter disabled random card generation";
        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(CardFactory), nameof(CardFactory.FilterForCombat), [typeof(IEnumerable<CardModel>)])];
        public static void Postfix(ref IEnumerable<CardModel> __result) =>
            __result = __result.Where(ContentOptions.IsEnabled);
    }

    public sealed class ContentOptionsRelicPoolPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.content.relic_pool";
        public static string Description => "Filter disabled relic offers";
        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(RelicPoolModel), nameof(RelicPoolModel.GetUnlockedRelics), [typeof(UnlockState)])];
        public static void Postfix(ref IEnumerable<RelicModel> __result) =>
            __result = __result.Where(ContentOptions.IsEnabled);
    }

    public sealed class ContentOptionsPotionPoolPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.content.potion_pool";
        public static string Description => "Filter disabled potion offers";
        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(PotionPoolModel), nameof(PotionPoolModel.GetUnlockedPotions), [typeof(UnlockState)])];
        public static void Postfix(ref IEnumerable<PotionModel> __result) =>
            __result = __result.Where(ContentOptions.IsEnabled);
    }
}
