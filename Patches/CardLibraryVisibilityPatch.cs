using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using STS2RitsuLib.Patching.Models;

namespace STS2_AiACard.Patches
{
    public sealed class CardLibraryVisibilityPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".library.visible";
        public static string Description => "Show this mod's card art in the library before discovery";
        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(NCardLibraryGrid), "GetCardVisibility", [typeof(CardModel)])];
        public static bool Prefix(CardModel card, ref ModelVisibility __result)
        {
            if (card.GetType().Assembly != typeof(Main).Assembly)
                return true;
            __result = ModelVisibility.Visible;
            return false;
        }
    }
}
