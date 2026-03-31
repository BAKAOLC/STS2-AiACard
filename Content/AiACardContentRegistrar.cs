using STS2_AiACard.Content.Descriptors;
using STS2RitsuLib;

namespace STS2_AiACard.Content
{
    internal static class AiACardContentRegistrar
    {
        internal static void RegisterAll()
        {
            RitsuLibFramework.CreateContentPack(Const.ModId)
                .ContentManifest(AiACardContentManifest.ContentEntries)
                .Apply();
        }
    }
}
