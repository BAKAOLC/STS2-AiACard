using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Colorless
{
    /// <summary>义之剑：打出时强化仁之剑伤害；若仁之剑已在己方任一战斗牌堆中则置入手牌。</summary>
    public sealed class YiSwordCard() : ModCardTemplate(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            HoverTipFactory.FromCardWithCardHoverTips<RenSwordCard>();

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new DamageVar(0m, ValueProp.Move)];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.PlaceholderPortrait, Const.Paths.PlaceholderPortrait);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target);
            PairedSwordsHelper.BuffSisterDamage<RenSwordCard>(Owner, 3m);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await PairedSwordsHelper.MoveSisterToHandIfOwned<RenSwordCard>(Owner, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
        }
    }
}
