using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2_AiACard.Cards.Silent;

namespace STS2_AiACard.Powers
{
    public sealed class PlagueResonanceDexterityDownPower : AiACardPowerBase
    {
        private bool _isRemoving;

        public override PowerType Type => PowerType.Debuff;

        public override PowerStackType StackType => PowerStackType.Counter;

        public override LocString Title => ModelDb.Card<PlagueResonance>().TitleLocString;

        public override LocString Description => new("powers", "TEMPORARY_DEXTERITY_DOWN.description");

        protected override string SmartDescriptionLocKey => "TEMPORARY_DEXTERITY_DOWN.smartDescription";

        protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromCard<PlagueResonance>(),
            HoverTipFactory.FromPower<DexterityPower>(),
        ];

        public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier,
            CardModel? cardSource)
        {
            await PowerCmd.Apply<DexterityPower>(target, -amount, applier, cardSource, true);
        }

        public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier,
            CardModel? cardSource)
        {
            if (_isRemoving || power != this || amount == Amount) return;

            await PowerCmd.Apply<DexterityPower>(Owner, -amount, applier, cardSource, true);
        }

        public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
        {
            if (side != Owner.Side) return;

            Flash();
            _isRemoving = true;
            await PowerCmd.Remove(this);
            _isRemoving = false;
            await PowerCmd.Apply<DexterityPower>(Owner, Amount, Owner, null, true);
        }
    }
}
