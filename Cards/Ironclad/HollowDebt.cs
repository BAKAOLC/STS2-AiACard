using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Ironclad
{
    /// <summary>虚妄代偿</summary>
    public sealed class HollowDebt() : ModCardTemplate(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromCard<Toxic>()];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new DamageVar(4m, ValueProp.Move)];

        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.HollowDebt, Const.Paths.CardPortraits.HollowDebt);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(CombatState);

            await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.AttackAnimDelay);

            var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .TargetingAllOpponents(CombatState)
                .WithHitFx("vfx/vfx_attack_slash");

            await attack.Execute(choiceContext);

            var healHp = attack.Results.Sum(r => r.UnblockedDamage);
            if (healHp > 0)
                await CreatureCmd.Heal(Owner.Creature, healHp);

            for (var i = 0; i < 2; i++)
            {
                var toHand = CombatState.CreateCard<Toxic>(Owner);
                CardCmd.ApplyKeyword(toHand, CardKeyword.Ethereal);
                await CardPileCmd.AddGeneratedCardToCombat(toHand, PileType.Hand, true);
            }

            var drawDiscardPreview = new List<CardPileAddResult>(4);
            for (var i = 0; i < 2; i++)
            {
                var toDraw = CombatState.CreateCard<Toxic>(Owner);
                CardCmd.ApplyKeyword(toDraw, CardKeyword.Ethereal);
                drawDiscardPreview.Add(await CardPileCmd.AddGeneratedCardToCombat(toDraw, PileType.Draw, true,
                    CardPilePosition.Random));
            }

            for (var i = 0; i < 2; i++)
            {
                var toDiscard = CombatState.CreateCard<Toxic>(Owner);
                CardCmd.ApplyKeyword(toDiscard, CardKeyword.Ethereal);
                drawDiscardPreview.Add(await CardPileCmd.AddGeneratedCardToCombat(toDiscard, PileType.Discard, true));
            }

            if (LocalContext.IsMe(Owner) && drawDiscardPreview.Count > 0)
            {
                CardCmd.PreviewCardPileAdd(drawDiscardPreview);
                await Cmd.Wait(1f);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(2m);
        }
    }
}
