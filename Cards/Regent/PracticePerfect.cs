using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2_AiACard.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Regent
{
    /// <summary>熟能生巧</summary>
    public sealed class PracticePerfect() : ModCardTemplate(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
            [HoverTipFactory.FromCard<SovereignBlade>()];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.PracticePerfect, Const.Paths.CardPortraits.PracticePerfect);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(Owner.PlayerCombatState);
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            if (!Owner.Creature.HasPower<SwordPracticePower>())
                await PowerCmd.Apply<SwordPracticePower>(Owner.Creature, 1, Owner.Creature, this);
            foreach (var c in Owner.PlayerCombatState.AllCards.Where(static x => !x.IsDupe).OfType<SovereignBlade>())
                CardCmd.Upgrade(c);
        }

        protected override void OnUpgrade()
        {
            AddKeyword(CardKeyword.Innate);
        }
    }
}
