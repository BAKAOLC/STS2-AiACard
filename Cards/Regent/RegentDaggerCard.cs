using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Regent
{
    /// <summary>君王小匕首：由剑花纷飞转化；每段 9 点伤害。</summary>
    public sealed class RegentDaggerCard()
        : ModCardTemplate(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        private const string HitCountKey = "HitCount";

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new DamageVar(9m, ValueProp.Move), new(HitCountKey, 1m)];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromCard<SovereignBlade>()];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.PlaceholderPortrait, Const.Paths.PlaceholderPortrait);

        public void ConfigureHitsFromSovereignBaseDamage(decimal sovereignBaseDamage)
        {
            var hits = 1 + (int)(sovereignBaseDamage / 10m);
            if (hits < 1)
                hits = 1;
            DynamicVars[HitCountKey].BaseValue = hits;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitCount(DynamicVars[HitCountKey].IntValue)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }
}
