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
            [HoverTipFactory.FromCard<Beckon>()];

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new DamageVar(4m, ValueProp.Move)];

        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.PlaceholderPortrait, Const.Paths.PlaceholderPortrait);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(CombatState);

            await CreatureCmd.TriggerAnim(Owner.Creature, "Attack", Owner.Character.AttackAnimDelay);

            var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .TargetingAllOpponents(CombatState)
                .WithHitFx("vfx/vfx_attack_slash");

            await attack.Execute(choiceContext);

            var healHp = attack.Results.Sum(r => r.UnblockedDamage + r.OverkillDamage);
            if (healHp > 0)
                await CreatureCmd.Heal(Owner.Creature, healHp);

            var handPile = PileType.Hand.GetPile(Owner);
            var deficit = Math.Max(0, Const.CombatHandMax - handPile.Cards.Count);
            for (var i = 0; i < deficit; i++)
            {
                var token = CombatState.CreateCard<Beckon>(Owner);
                var added = await CardPileCmd.AddGeneratedCardToCombat(token, PileType.Hand, true);
                if (!added.success || handPile.Cards.Count >= Const.CombatHandMax)
                    break;
            }

            var drawDiscard = new List<CardPileAddResult>(6);
            for (var i = 0; i < 3; i++)
            {
                var toDraw = CombatState.CreateCard<Beckon>(Owner);
                drawDiscard.Add(await CardPileCmd.AddGeneratedCardToCombat(toDraw, PileType.Draw, true,
                    CardPilePosition.Random));
            }

            for (var i = 0; i < 3; i++)
            {
                var toDiscard = CombatState.CreateCard<Beckon>(Owner);
                drawDiscard.Add(await CardPileCmd.AddGeneratedCardToCombat(toDiscard, PileType.Discard, true));
            }

            if (LocalContext.IsMe(Owner))
            {
                CardCmd.PreviewCardPileAdd(drawDiscard);
                await Cmd.Wait(1f);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(2m);
        }
    }
}
