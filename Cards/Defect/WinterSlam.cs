using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Defect
{
    /// <summary>凛冬打击：命中段数 = 本场已生成过的冰霜充能球次数 + 1（与 <see cref="OrbChanneledEntry" /> 一致）。</summary>
    public sealed class WinterSlam() : ModCardTemplate(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        private const string CalculatedHitsKey = "CalculatedHits";

        protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.Static(StaticHoverTip.Channeling),
            HoverTipFactory.FromOrb<FrostOrb>(),
        ];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new DamageVar(7m, ValueProp.Move),
            new CalculationBaseVar(1m),
            new CalculationExtraVar(1m),
            new CalculatedVar(CalculatedHitsKey).WithMultiplier(static (card, _) =>
                CombatManager.Instance.IsInProgress && card.CombatState != null
                    ? CombatManager.Instance.History.Entries.OfType<OrbChanneledEntry>()
                        .Count(e => e.Actor.Player == card.Owner && e.Orb is FrostOrb)
                    : 0m),
        ];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.PlaceholderPortrait, Const.Paths.PlaceholderPortrait);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target);
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
