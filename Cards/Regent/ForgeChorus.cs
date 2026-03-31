using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Regent
{
    /// <summary>全力锻造</summary>
    public sealed class ForgeChorus() : ModCardTemplate(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
        protected override bool HasEnergyCostX => true;

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new ForgeVar(10)];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.PlaceholderPortrait, Const.Paths.PlaceholderPortrait);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            var x = ResolveEnergyXValue();
            var amt = DynamicVars.Forge.BaseValue;
            for (var i = 0; i < x; i++)
                await ForgeCmd.Forge(amt, Owner, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Forge.UpgradeValueBy(5m);
        }
    }
}
