using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace STS2_AiACard.Utils
{
    internal static class AttackCommandCompatExtensions
    {
        internal static AttackCommand FromCardCompat(
            this AttackCommand attack,
            CardModel card,
            CardPlay cardPlay)
        {
#if STS2_AT_LEAST_0_110_0
            return attack.FromCard(card, cardPlay);
#else
            return attack.FromCard(card);
#endif
        }
    }
}
