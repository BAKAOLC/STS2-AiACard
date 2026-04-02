using MegaCrit.Sts2.Core.CardSelection;
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
        private const string DiscardKey = "DiscardCards";
        private const string DexLossKey = "DexLoss";
        private const string GainEnergyKey = "GainEnergy";
        private const string ArtifactStacksKey = "ArtifactStacks";

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new CardsVar(DiscardKey, 3),
            new(DexLossKey, 2m),
            new EnergyVar(GainEnergyKey, 3),
            new(ArtifactStacksKey, 3m),
        ];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromPower<PlagueResonanceDexterityDownPower>(),
            HoverTipFactory.FromPower<ArtifactPower>(),
        ];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.PlagueResonance, Const.Paths.CardPortraits.PlagueResonance);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(CombatState);
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

            var discardN = DynamicVars[DiscardKey].IntValue;
            var handCount = PileType.Hand.GetPile(Owner).Cards.Count;
            var toDiscard = Math.Min(discardN, handCount);
            if (toDiscard > 0)
            {
                var picked = await CardSelectCmd.FromHandForDiscard(choiceContext, Owner,
                    new(CardSelectorPrefs.DiscardSelectionPrompt, toDiscard), null, this);
                await CardCmd.Discard(choiceContext, picked);
            }

            var dex = DynamicVars[DexLossKey].IntValue;
            await PowerCmd.Apply<PlagueResonanceDexterityDownPower>(Owner.Creature, dex, Owner.Creature, this);

            await PlayerCmd.GainEnergy(DynamicVars[GainEnergyKey].IntValue, Owner);

            var enemies = CombatState.HittableEnemies;
            if (enemies.Count > 0)
            {
                var target = Owner.RunState.Rng.CombatTargets.NextItem(enemies);
                if (target != null)
                {
                    var art = DynamicVars[ArtifactStacksKey].IntValue;
                    await PowerCmd.Apply<ArtifactPower>(target, art, Owner.Creature, this);
                }
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars[DiscardKey].UpgradeValueBy(-1m);
            DynamicVars[DexLossKey].UpgradeValueBy(-1m);
        }
    }
}
