using MegaCrit.Sts2.Core.Models.CardPools;
using STS2_AiACard.Cards.Colorless;
using STS2_AiACard.Cards.Defect;
using STS2_AiACard.Cards.Ironclad;
using STS2_AiACard.Cards.Necrobinder;
using STS2_AiACard.Cards.Regent;
using STS2_AiACard.Cards.Silent;
using STS2_AiACard.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_AiACard.Content.Descriptors
{
    internal static class AiACardContentManifest
    {
        public static IReadOnlyList<IContentRegistrationEntry> ContentEntries { get; } =
        [
            new PowerRegistrationEntry<VeiledFormPower>(),
            new PowerRegistrationEntry<VeiledFormExtraTurnPower>(),
            new PowerRegistrationEntry<PlagueResonanceStrengthDownPower>(),
            new PowerRegistrationEntry<PlagueResonanceDexterityDownPower>(),
            new PowerRegistrationEntry<RequiemEnergyPower>(),
            new PowerRegistrationEntry<SwordPracticePower>(),
            new PowerRegistrationEntry<ManipulationRealityPower>(),
            new PowerRegistrationEntry<WinterFrostGenTrackerPower>(),
            new PowerRegistrationEntry<HeavenGiftPower>(),
            new PowerRegistrationEntry<PostCombatHealPower>(),
            new PowerRegistrationEntry<SnakeEloPower>(),
            new CardRegistrationEntry<IroncladCardPool, GnawBite>(),
            new CardRegistrationEntry<IroncladCardPool, HollowDebt>(),
            new CardRegistrationEntry<IroncladCardPool, MaliceSpread>(),
            new CardRegistrationEntry<SilentCardPool, VeiledForm>(),
            new CardRegistrationEntry<SilentCardPool, PlagueResonance>(),
            new CardRegistrationEntry<SilentCardPool, SerpentArray>(),
            new CardRegistrationEntry<RegentCardPool, PracticePerfect>(),
            new CardRegistrationEntry<RegentCardPool, BlossomBlades>(),
            new CardRegistrationEntry<RegentCardPool, RegentDaggerCard>(),
            new CardRegistrationEntry<RegentCardPool, ForgeChorus>(),
            new CardRegistrationEntry<NecrobinderCardPool, RequiemCard>(),
            new CardRegistrationEntry<NecrobinderCardPool, DreamReturn>(),
            new CardRegistrationEntry<NecrobinderCardPool, UnderworldDraw>(),
            new CardRegistrationEntry<DefectCardPool, WinterSlam>(),
            new CardRegistrationEntry<DefectCardPool, LearningJoy>(),
            new CardRegistrationEntry<DefectCardPool, BootSequence>(),
            new CardRegistrationEntry<ColorlessCardPool, PostCombatTreatment>(),
            new CardRegistrationEntry<ColorlessCardPool, ManipulationRealityCard>(),
            new CardRegistrationEntry<ColorlessCardPool, SnakeContractCard>(),
            new CardRegistrationEntry<ColorlessCardPool, HeavenGiftCard>(),
            new CardRegistrationEntry<ColorlessCardPool, SnakeEloCard>(),
        ];
    }
}
