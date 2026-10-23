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
    /// <summary>义之剑：打出时强化仁之剑伤害；若仁之剑已在己方任一战斗牌堆中则置入手牌。</summary>
    public sealed class YiSwordCard() : ModCardTemplate(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        // See RenSwordCard.AdditionalHoverTips — avoid FromCardWithCardHoverTips (mutual recursion).
        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromCard<RenSwordCard>()];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new DamageVar(5m, ValueProp.Move), new CardsVar(2), new DynamicVar("SisterDamage", 3m)];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.YiSword, Const.Paths.CardPortraits.YiSword);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target);
            PairedSwordsHelper.BuffSisterDamage<RenSwordCard>(Owner, DynamicVars["SisterDamage"].BaseValue);
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCardCompat(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
            await PairedSwordsHelper.MoveSisterToHandIfOwned<RenSwordCard>(Owner, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["SisterDamage"].UpgradeValueBy(2m);
        }
    }
}
