using System.Diagnostics;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Models;

namespace STS2_AiACard.Utils
{
    internal static class GenerationPolicy
    {
        private static readonly HashSet<string> PotionOnlyCards =
        [
            "GnawBite", "HollowDebt", "PostCombatTreatment", "SnakeEloCard", "HeavenGiftCard",
            "MpCorpseSpeaks", "MpGamblingCard", "MpSharedWealth"
        ];

        internal static bool IsPotionOnly(CardModel card) =>
            card.GetType().Assembly == typeof(Main).Assembly && PotionOnlyCards.Contains(card.GetType().Name);

        // Async state-machine frames retain their declaring model across awaits. Use the nearest model
        // so a power triggered by a potion does not inherit the potion's generation permission.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool AllowsPotionGeneration()
        {
            foreach (var frame in new StackTrace().GetFrames())
            {
                var method = frame.GetMethod();
                if (method == null || method.Name == "get_CanBeGeneratedInCombat")
                    continue;
                for (var type = method.DeclaringType; type != null; type = type.DeclaringType)
                {
                    if (typeof(PotionModel).IsAssignableFrom(type))
                        return true;
                    if (typeof(CardModel).IsAssignableFrom(type) || typeof(PowerModel).IsAssignableFrom(type) ||
                        typeof(RelicModel).IsAssignableFrom(type))
                        return false;
                }
            }
            return false;
        }
    }
}
