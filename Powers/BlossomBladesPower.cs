using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace STS2_AiACard.Powers;

/// <summary>
///     剑花纷飞：本场战斗中君王之剑单次伤害视为 6；攻击次数由当前伤害数值按每 10 点 +1 次计算；铸造不再加伤害，每铸造 10 改为增加攻击次数。
/// </summary>
public sealed class BlossomBladesPower : AiACardPowerBase
{
    public const decimal BlossomDamagePerHit = 6m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>将当前伤害读数换算为段数，并把伤害压到 6。</summary>
    public static void NormalizeBlade(SovereignBlade blade)
    {
        var dmg = blade.DynamicVars.Damage.BaseValue;
        var hits = 1 + (int)(dmg / 10m);
        if (hits < 1)
            hits = 1;
        blade.SetRepeats(hits);
        blade.AddDamage(BlossomDamagePerHit - blade.DynamicVars.Damage.BaseValue);
    }

    public override Task AfterForge(decimal amount, Player forger, AbstractModel? source)
    {
        if (forger != Owner.Player || amount == 0m)
            return Task.CompletedTask;

        foreach (var blade in EnumerateSovereignBlades(forger))
        {
            blade.AddDamage(-amount);
            var bonusHits = (int)(amount / 10m);
            if (bonusHits > 0)
                blade.SetRepeats(blade.DynamicVars.Repeat.BaseValue + bonusHits);
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (card.IsDupe || oldPileType != PileType.None || card is not SovereignBlade blade || blade.Owner != Owner.Player)
            return Task.CompletedTask;

        NormalizeBlade(blade);
        return Task.CompletedTask;
    }

    private static IEnumerable<SovereignBlade> EnumerateSovereignBlades(Player player)
    {
        var pcs = player.PlayerCombatState;
        if (pcs == null)
            return [];
        return pcs.AllCards.Where(static c => !c.IsDupe).OfType<SovereignBlade>();
    }
}
