using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Cards.Defect
{
    /// <summary>必有我师：从其他角色能力池中随机展示至多 3 张能力牌；未升级时为未升级版本，升级后为升级版本；选一张加入手牌且本回合免费打出。</summary>
    public sealed class LearningJoy() : ModCardTemplate(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public override CardAssetProfile AssetProfile =>
            new(Const.Paths.CardPortraits.LearningJoy, Const.Paths.CardPortraits.LearningJoy);

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(CombatState);
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

            var selfPoolId = Owner.Character.CardPool.Id;
            var pools = new CardPoolModel[]
            {
                ModelDb.CardPool<IroncladCardPool>(),
                ModelDb.CardPool<SilentCardPool>(),
                ModelDb.CardPool<DefectCardPool>(),
                ModelDb.CardPool<RegentCardPool>(),
                ModelDb.CardPool<NecrobinderCardPool>(),
            }.Where(p => p.Id != selfPoolId);

            var canonical = pools
                .SelectMany(p => p.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint))
                .Where(c => c.Type == CardType.Power);
            canonical = CardFactory.FilterForCombat(canonical);

            var list = canonical.ToList();
            if (list.Count == 0)
                return;

            var rng = Owner.RunState.Rng.CombatCardGeneration;
            var take = Math.Min(3, list.Count);
            var chosenCanon = list.TakeRandom(take, rng).ToList();

            var options = new List<CardModel>(chosenCanon.Count);
            foreach (var c in chosenCanon)
            {
                var inst = CombatState.CreateCard(c, Owner);
                if (IsUpgraded)
                    CardCmd.Upgrade(inst);
                options.Add(inst);
            }

            var picked = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner, true);
            if (picked == null)
                return;

            picked.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(picked, PileType.Hand, true);
        }
    }
}
