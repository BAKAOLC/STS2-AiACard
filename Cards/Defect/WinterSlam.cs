using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2_AiACard.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Defect
{
    /// <summary>凛冬打击：命中段数 = 本场已成功构筑的冰霜球次数 + 1（由 <see cref="WinterFrostGenTrackerPower" /> 计数）。</summary>
    public sealed class WinterSlam() : ModCardTemplate(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        private const string CalculatedHitsKey = "CalculatedHits";

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.Static(StaticHoverTip.Channeling),
            HoverTipFactory.FromOrb<FrostOrb>(),
        ];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new DamageVar(7m, ValueProp.Move),
            new CalculationBaseVar(0m),
            new CalculationExtraVar(1m),
            new CalculatedVar(CalculatedHitsKey).WithMultiplier(static (card, _) =>
            {
                if (!CombatManager.Instance.IsInProgress || card.CombatState == null)
                    return 0m;
                var p = card.Owner?.Creature?.GetPower<WinterFrostGenTrackerPower>();
                var n = p?.Amount ?? 1m;
                return n < 1 ? 1m : n;
            }),
        ];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.PlaceholderPortrait, Const.Paths.PlaceholderPortrait);

        public override async Task AfterCardEnteredCombat(CardModel card)
        {
            if (card != this || Owner?.Creature == null || CombatState == null)
                return;
            if (Owner.Creature.HasPower<WinterFrostGenTrackerPower>())
                return;
            await PowerCmd.Apply<WinterFrostGenTrackerPower>(Owner.Creature, 1, Owner.Creature, this);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target);
            if (!Owner.Creature.HasPower<WinterFrostGenTrackerPower>())
                await PowerCmd.Apply<WinterFrostGenTrackerPower>(Owner.Creature, 1, Owner.Creature, this);
            var hits = (int)((CalculatedVar)DynamicVars[CalculatedHitsKey]).Calculate(cardPlay.Target);
            if (hits < 1)
                hits = 1;
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitCount(hits)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(2m);
        }
    }
}
