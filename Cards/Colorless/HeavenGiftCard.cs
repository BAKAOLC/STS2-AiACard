using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2_AiACard.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Colorless
{
    /// <summary>上天的恩赐</summary>
    public sealed class HeavenGiftCard() : ModCardTemplate(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        private const string CalculatedGoldKey = "CalculatedGold";

        protected override bool HasEnergyCostX => true;

        public override IEnumerable<CardKeyword> CanonicalKeywords =>
            [CardKeyword.Ethereal, CardKeyword.Exhaust];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromPower<HeavenGiftPower>()];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new CalculationBaseVar(0m),
            new CalculationExtraVar(5m),
            new CalculatedVar(CalculatedGoldKey).WithMultiplier(static (card, _) =>
            {
                if (!CombatManager.Instance.IsInProgress || card.CombatState == null)
                    return 0m;
                return card.EnergyCost.CapturedXValue;
            }),
        ];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.PlaceholderPortrait, Const.Paths.PlaceholderPortrait);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            await PlayerCmd.LoseGold(3, Owner);
            var gold = (int)((CalculatedVar)DynamicVars[CalculatedGoldKey]).Calculate(null);
            await PowerCmd.Apply<HeavenGiftPower>(Owner.Creature, gold, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.CalculationExtra.UpgradeValueBy(2m);
        }
    }
}
