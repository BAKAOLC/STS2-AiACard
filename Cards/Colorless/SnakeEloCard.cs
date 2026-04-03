using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2_AiACard.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Colorless
{
    /// <summary>蛇咬 ELO</summary>
    public sealed class SnakeEloCard() : ModCardTemplate(1, CardType.Power, CardRarity.Basic, TargetType.Self)
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromCard<Snakebite>()];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.SnakeElo, Const.Paths.CardPortraits.SnakeElo);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(CombatState);
            ArgumentNullException.ThrowIfNull(Owner.PlayerCombatState);
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            if (!Owner.Creature.HasPower<SnakeEloPower>())
                await PowerCmd.Apply<SnakeEloPower>(Owner.Creature, 1, Owner.Creature, this);
            foreach (var c in Owner.PlayerCombatState.AllCards.ToList())
            {
                if (c.Type != CardType.Attack || c is Snakebite)
                    continue;
                var sn = CombatState.CreateCard<Snakebite>(Owner);
                await CardCmd.Transform(c, sn);
            }
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
