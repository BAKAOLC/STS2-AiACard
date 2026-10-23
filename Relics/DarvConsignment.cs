using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Relics
{
    public sealed class DarvConsignment : ModRelicTemplate
    {
        public override RelicRarity Rarity => RelicRarity.Ancient;
        public override bool HasUponPickupEffect => true;
        public override RelicAssetProfile AssetProfile => new(
            IconPath: Const.Paths.Root + "/relics/darv_consignment.png",
            IconOutlinePath: Const.Paths.Root + "/relics/darv_consignment_outline.png");

        internal static RelicModel? FindStarter(Player player) => player.Relics.FirstOrDefault(relic =>
            player.Character.StartingRelics.Any(starter => starter.Id == relic.Id));

        internal static List<RelicModel> GetCandidates(Player player) => ModelDb.AncientEvent<Darv>()
            .AllPossibleOptions.Select(option => option.Relic).OfType<RelicModel>()
            .Where(relic => relic is not DustyTome && !player.Relics.Any(owned => owned.Id == relic.Id))
            .Select(relic => (RelicModel)relic.CanonicalInstance)
            .DistinctBy(relic => relic.Id).ToList();

        public override async Task AfterObtained()
        {
            var starter = FindStarter(Owner);
            var candidates = GetCandidates(Owner);
            if (starter == null || candidates.Count == 0)
                return;
            var chosen = Owner.PlayerRng.Rewards.NextItem(candidates);
            ArgumentNullException.ThrowIfNull(chosen);
            var reward = chosen.ToMutable();
            await RelicCmd.Replace(starter, reward);
        }
    }
}
