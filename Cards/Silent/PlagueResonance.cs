using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_AiACard.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Silent
{
    /// <summary>荒疫共鸣</summary>
    public sealed class PlagueResonance() : ModCardTemplate(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        private const string StatKey = "StatLoss";
        private const string GainEnergyKey = "GainEnergy";

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new(StatKey, 2m), new EnergyVar(GainEnergyKey, 2)];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromPower<PlagueResonanceStrengthDownPower>(),
            HoverTipFactory.FromPower<PlagueResonanceDexterityDownPower>(),
            HoverTipFactory.FromPower<ArtifactPower>(),
        ];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.PlaceholderPortrait, Const.Paths.PlaceholderPortrait);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(CombatState);
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            var n = DynamicVars[StatKey].IntValue;
            await PowerCmd.Apply<PlagueResonanceStrengthDownPower>(Owner.Creature, n, Owner.Creature, this);
            await PowerCmd.Apply<PlagueResonanceDexterityDownPower>(Owner.Creature, n, Owner.Creature, this);
            foreach (var e in CombatState.HittableEnemies)
                await PowerCmd.Apply<ArtifactPower>(e, n, Owner.Creature, this);
            await PlayerCmd.GainEnergy(DynamicVars[GainEnergyKey].IntValue, Owner);
        }

        protected override void OnUpgrade()
        {
            DynamicVars[StatKey].UpgradeValueBy(-1m);
        }
    }
}
