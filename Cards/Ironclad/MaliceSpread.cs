using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Ironclad
{
    /// <summary>恶意弥漫</summary>
    public sealed class MaliceSpread() : ModCardTemplate(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        protected override bool HasEnergyCostX => true;

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromPower<WeakPower>(),
            HoverTipFactory.FromPower<VulnerablePower>(),
        ];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.MaliceSpread, Const.Paths.CardPortraits.MaliceSpread);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(CombatState);

            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            VfxCmd.PlayOnCreatureCenter(Owner.Creature, "vfx/vfx_flying_slash");

            var amount = ResolveEnergyXValue();
            if (IsUpgraded)
                amount++;

            foreach (var enemy in CombatState.HittableEnemies)
            {
                await PowerCmd.Apply<WeakPower>(choiceContext, enemy, amount, Owner.Creature, this);
                await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, amount, Owner.Creature, this);
            }
        }
    }
}
