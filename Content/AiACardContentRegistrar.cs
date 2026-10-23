using STS2_AiACard.Content.Descriptors;
using STS2RitsuLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using STS2_AiACard.Relics;
using STS2RitsuLib.Scaffolding.Ancients.Options;

namespace STS2_AiACard.Content
{
    internal static class AiACardContentRegistrar
    {
        internal static void RegisterAll()
        {
            RitsuLibFramework.CreateContentPack(Const.ModId)
                .ContentManifest(AiACardContentManifest.ContentEntries)
                .AncientOption<Neow>(ModAncientOptionRule.Single(
                    ancient => ancient.RelicOption(ModelDb.Relic<DarvConsignment>().ToMutable()),
                    ancient => ancient.Owner != null && ContentOptions.IsEnabled(ModelDb.Relic<DarvConsignment>()) &&
                        DarvConsignment.FindStarter(ancient.Owner) != null &&
                        DarvConsignment.GetCandidates(ancient.Owner).Count > 0))
                .Apply();
        }
    }
}
