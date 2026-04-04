using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2_AiACard.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Regent
{
    /// <summary>剑花纷飞</summary>
    public sealed class BlossomBlades() : ModCardTemplate(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromCard<SovereignBlade>()];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.BlossomBlades, Const.Paths.CardPortraits.BlossomBlades);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(Owner.PlayerCombatState);
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            if (!Owner.Creature.HasPower<BlossomBladesPower>())
                await PowerCmd.Apply<BlossomBladesPower>(Owner.Creature, 1, Owner.Creature, this);
            foreach (var blade in Owner.PlayerCombatState.AllCards.Where(static c => !c.IsDupe)
                         .OfType<SovereignBlade>())
                BlossomBladesPower.NormalizeBlade(blade);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
