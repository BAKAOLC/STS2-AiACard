using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Ironclad
{
    /// <summary>恶意弥漫</summary>
    public sealed class MaliceSpread() : ModCardTemplate(0, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
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

            var times = ResolveEnergyXValue();
            if (IsUpgraded)
                times++;

            foreach (var enemy in CombatState.HittableEnemies)
            {
                for (var i = 0; i < times; i++)
                {
                    await PowerCmd.Apply<WeakPower>(choiceContext, enemy, 1, Owner.Creature, this);
                    await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, 1, Owner.Creature, this);
                }
            }
        }

        public override async Task AfterAutoPrePlayPhaseEnteredEarly(PlayerChoiceContext choiceContext, Player player)
        {
            if (Pile?.Type != PileType.Exhaust || player != Owner)
                return;

            await CardCmd.AutoPlay(choiceContext, this, null);
        }
    }
}
