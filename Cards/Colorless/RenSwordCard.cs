using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2_AiACard.Utils;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Colorless
{
    /// <summary>仁之剑：打出时强化义之剑伤害；若义之剑已在己方任一战斗牌堆中则置入手牌。</summary>
    public sealed class RenSwordCard() : ModCardTemplate(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        // FromCard only: FromCardWithCardHoverTips chains HoverTips and would recurse with YiSwordCard's tip to Ren.
        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromCard<YiSwordCard>()];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new DamageVar(5m, ValueProp.Move), new CardsVar(2), new DynamicVar("SisterDamage", 3m)];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.RenSword, Const.Paths.CardPortraits.RenSword);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target);
            PairedSwordsHelper.BuffSisterDamage<YiSwordCard>(Owner, DynamicVars["SisterDamage"].BaseValue);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCardCompat(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
            await PairedSwordsHelper.MoveSisterToHandIfOwned<YiSwordCard>(Owner, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["SisterDamage"].UpgradeValueBy(2m);
        }
    }
}
