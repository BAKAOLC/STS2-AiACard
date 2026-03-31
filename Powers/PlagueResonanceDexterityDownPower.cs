using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_AiACard.Cards.Silent;

namespace STS2_AiACard.Powers;

public sealed class PlagueResonanceDexterityDownPower : TemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Card<PlagueResonance>();

    protected override bool IsPositive => false;
}
