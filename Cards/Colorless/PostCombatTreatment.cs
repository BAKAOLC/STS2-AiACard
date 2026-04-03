using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2_AiACard.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Colorless
{
    /// <summary>准备治疗</summary>
    public sealed class PostCombatTreatment() : ModCardTemplate(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new HealVar(6m)];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromPower<PostCombatHealPower>()];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.PostCombatTreatment, Const.Paths.CardPortraits.PostCombatTreatment);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            await PowerCmd.Apply<PostCombatHealPower>(Owner.Creature, DynamicVars.Heal.BaseValue, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Heal.UpgradeValueBy(3m);
        }
    }
}
