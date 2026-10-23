using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using STS2_AiACard.Utils;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Potions
{
    public sealed class SmokeBomb : ModPotionTemplate
    {
        public override PotionRarity Rarity => PotionRarity.Uncommon;
        public override PotionUsage Usage => PotionUsage.CombatOnly;
        public override TargetType TargetType => TargetType.AnyPlayer;
        public override PotionAssetProfile AssetProfile => new(
            ImagePath: Const.Paths.Root + "/potions/smoke_bomb.png",
            OutlinePath: Const.Paths.Root + "/potions/smoke_bomb_outline.png");
        public override bool PassesCustomUsabilityCheck => CombatManager.Instance.IsInProgress &&
            Owner.RunState.CurrentRoom is CombatRoom { ParentEventId: null } room &&
            room.RoomType != MegaCrit.Sts2.Core.Rooms.RoomType.Boss && !SmokeEscape.IsEscaped(Owner.Creature);

        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
        {
            if (!PassesCustomUsabilityCheck || target?.Player == null || !target.IsAlive ||
                target.CombatState != Owner.Creature.CombatState)
                return;
            await SmokeEscape.Escape(target.Player);
        }
    }
}
