using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Silent
{
    /// <summary>万蛇阵</summary>
    public sealed class SerpentArray() : ModCardTemplate(0, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
    {
        private const string PoisonStacksKey = "PoisonStacks";

        protected override bool HasEnergyCostX => true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new(PoisonStacksKey, 3m)];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromPower<PoisonPower>()];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.SerpentArray, Const.Paths.CardPortraits.SerpentArray);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(CombatState);
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            var x = ResolveEnergyXValue();
            var times = IsUpgraded ? x + 1 : x;
            var stacksPerApply = DynamicVars[PoisonStacksKey].IntValue;
            for (var i = 0; i < times; i++)
                foreach (var e in CombatState.HittableEnemies)
                    await PowerCmd.Apply<PoisonPower>(choiceContext, e, stacksPerApply, Owner.Creature, this);
        }
    }
}
