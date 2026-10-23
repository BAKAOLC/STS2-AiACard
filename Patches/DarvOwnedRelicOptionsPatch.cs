using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Patching.Models;

namespace STS2_AiACard.Patches
{
    public sealed class DarvOwnedRelicOptionsPatch : IPatchMethod
    {
        public static string PatchId => $"{Const.ModId}.darv.exclude_owned_relics";
        public static string Description => "Avoid offering already-owned Darv relics";
        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(Darv), "GenerateInitialOptions", Type.EmptyTypes)];
        public static void Postfix(Darv __instance, ref IReadOnlyList<EventOption> __result)
        {
            var owner = __instance.Owner;
            if (owner == null)
                return;
            var options = __result.ToList();
            var candidates = __instance.AllPossibleOptions
                .Where(option => option.Relic is not null and not DustyTome)
                .Where(option => !owner.Relics.Any(relic => relic.Id == option.Relic!.Id))
                .Where(option => !options.Any(offered => offered.Relic?.Id == option.Relic!.Id))
                .ToList();
            for (var i = options.Count - 1; i >= 0; i--)
            {
                if (options[i].Relic is not { } relic || !owner.Relics.Any(owned => owned.Id == relic.Id))
                    continue;
                if (candidates.Count == 0)
                {
                    options.RemoveAt(i);
                    continue;
                }
                var replacement = __instance.Rng.NextItem(candidates);
                ArgumentNullException.ThrowIfNull(replacement);
                candidates.Remove(replacement);
                options[i] = replacement;
            }
            __result = options;
        }
    }
}
