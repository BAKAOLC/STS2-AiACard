using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace STS2_AiACard.Powers
{
    /// <summary>
    ///     剑花纷飞：本场战斗中君王之剑单次伤害视为 6；攻击次数由当前伤害数值按每 10 点 +1 次计算；铸造不再加伤害，每铸造 10 改为增加攻击次数。
    /// </summary>
    public sealed class BlossomBladesPower : AiACardPowerBase
    {
        public const decimal BlossomDamagePerHit = 6m;

        private static readonly ConditionalWeakTable<SovereignBlade, BladeBlossomState> BladeStates = new();

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Single;

        /// <summary>将当前伤害读数换算为段数，并把伤害压到 6；最终段数 = (剑花基础 + 铸造加成) × (1 + 剑圣层数)，与原版剑圣对默认 1 段的「每 1 层多一整轮」一致。</summary>
        public static void NormalizeBlade(SovereignBlade blade)
        {
            var state = BladeStates.GetValue(blade, static _ => new());
            var dmg = blade.DynamicVars.Damage.BaseValue;
            var alreadyNormalized = state.Initialized && dmg == BlossomDamagePerHit;

            if (!alreadyNormalized)
            {
                var blossomHits = 1 + (int)(dmg / 10m);
                if (blossomHits < 1)
                    blossomHits = 1;
                state.BaseBlossomHits = blossomHits;
                if (!state.Initialized)
                {
                    state.ForgeAccumulator = 0;
                    state.ForgeBonusHits = 0;
                }

                state.Initialized = true;
                blade.AddDamage(BlossomDamagePerHit - dmg);
            }

            var sage = blade.Owner?.Creature.GetPower<SwordSagePower>()?.Amount ?? 0m;
            ApplyTotalRepeats(blade, sage);
        }

        public override Task AfterForge(decimal amount, Player forger, AbstractModel? source)
        {
            if (forger != Owner.Player || amount == 0m)
                return Task.CompletedTask;

            foreach (var blade in EnumerateSovereignBlades(forger))
            {
                var state = BladeStates.GetValue(blade, static _ => new());
                state.ForgeAccumulator += amount;
                var extraHits = (int)(state.ForgeAccumulator / 10m);
                state.ForgeAccumulator -= extraHits * 10m;
                state.ForgeBonusHits += extraHits;
                blade.AddDamage(-amount);
                var sage = blade.Owner?.Creature.GetPower<SwordSagePower>()?.Amount ?? 0m;
                ApplyTotalRepeats(blade, sage);
            }

            return Task.CompletedTask;
        }

        public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
        {
            if (card.IsDupe || oldPileType != PileType.None || card is not SovereignBlade blade ||
                blade.Owner != Owner.Player)
                return Task.CompletedTask;

            NormalizeBlade(blade);
            return Task.CompletedTask;
        }

        /// <summary>Harmony Postfix：紧跟原版 <see cref="SwordSagePower.AfterPowerAmountChanged" />。</summary>
        internal static void HarmonyAfterSwordSagePowerAmountChanged(SwordSagePower sageInstance, PowerModel power)
        {
            if (power is not SwordSagePower || power.Owner != sageInstance.Owner)
                return;
            if (sageInstance.Owner.GetPower<BlossomBladesPower>() is null)
                return;
            RefreshAllSovereignBladesMerged(sageInstance.Owner, sageInstance.Amount);
        }

        /// <summary>Harmony Postfix：紧跟原版 <see cref="SwordSagePower.AfterCardEnteredCombat" />。</summary>
        internal static void HarmonyAfterSwordSageCardEnteredCombat(SwordSagePower sageInstance, CardModel card)
        {
            if (card.Owner != sageInstance.Owner.Player)
                return;
            if (card.IsDupe || card is not SovereignBlade blade)
                return;
            if (sageInstance.Owner.GetPower<BlossomBladesPower>() is null)
                return;
            ApplyTotalRepeats(blade, sageInstance.Amount);
        }

        /// <summary>Harmony Postfix：紧跟原版 <see cref="SwordSagePower.AfterRemoved" />。</summary>
        internal static void HarmonyAfterSwordSageRemoved(Creature oldOwner)
        {
            if (oldOwner.GetPower<BlossomBladesPower>() is null)
                return;
            RefreshAllSovereignBladesMerged(oldOwner, 0m);
        }

        private static void RefreshAllSovereignBladesMerged(Creature creature, decimal swordSageStackAmount)
        {
            var player = creature.Player;
            var pcs = player?.PlayerCombatState;
            if (pcs == null)
                return;
            foreach (var item in pcs.AllCards)
            {
                if (item.IsDupe || item is not SovereignBlade blade)
                    continue;
                ApplyTotalRepeats(blade, swordSageStackAmount);
            }
        }

        private static void ApplyTotalRepeats(SovereignBlade blade, decimal swordSageStackAmount)
        {
            var state = BladeStates.GetValue(blade, static _ => new());
            if (!state.Initialized)
            {
                var dmg = blade.DynamicVars.Damage.BaseValue;
                var bh = 1 + (int)(dmg / 10m);
                if (bh < 1)
                    bh = 1;
                state.BaseBlossomHits = bh;
                state.ForgeAccumulator = 0;
                state.ForgeBonusHits = 0;
                state.Initialized = true;
            }

            var coreHits = state.BaseBlossomHits + state.ForgeBonusHits;
            blade.SetRepeats(coreHits * (1m + swordSageStackAmount));
        }

        private static IEnumerable<SovereignBlade> EnumerateSovereignBlades(Player player)
        {
            var pcs = player.PlayerCombatState;
            if (pcs == null)
                return [];
            return pcs.AllCards.Where(static c => !c.IsDupe).OfType<SovereignBlade>();
        }

        private sealed class BladeBlossomState
        {
            public int BaseBlossomHits;
            public decimal ForgeAccumulator;
            public int ForgeBonusHits;
            public bool Initialized;
        }
    }
}
