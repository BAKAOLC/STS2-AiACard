using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2_AiACard.Content;
using STS2_AiACard.Patches;
using STS2RitsuLib;
using STS2RitsuLib.Patching.Core;

namespace STS2_AiACard
{
    [ModInitializer(nameof(Initialize))]
    public static class Main
    {
        public static readonly Logger Logger = RitsuLibFramework.CreateLogger(Const.ModId);

        public static bool IsModActive { get; private set; }

        public static void Initialize()
        {
            if (IsModActive)
            {
                Logger.Debug("Mod already initialized, skipping duplicate initialization.");
                return;
            }

            Logger.Info($"Mod ID: {Const.ModId}");
            Logger.Info($"Version: {Const.Version}");
            Logger.Info("Initializing mod...");

            try
            {
                RitsuLibFramework.EnsureGodotScriptsRegistered(Assembly.GetExecutingAssembly(), Logger);

                var kingMathPatcher = RitsuLibFramework.CreatePatcher(Const.ModId, "king_dislikes_math", "王不喜算术");
                kingMathPatcher.RegisterPatch<KingDislikesMathHasEnoughResourcesPatch>();
                kingMathPatcher.RegisterPatch<KingDislikesMathSpendResourcesPatch>();
                if (!RitsuLibFramework.ApplyRequiredPatcher(
                        kingMathPatcher,
                        () => IsModActive = false,
                        "王不喜算术相关补丁未能应用，本 Mod 已禁用。"))
                    return;

                var blossomSagePatcher =
                    RitsuLibFramework.CreatePatcher(Const.ModId, "blossom_blades_sword_sage", "剑花与剑圣");
                blossomSagePatcher.RegisterPatch<BlossomBladesSwordSageAfterPowerAmountPatch>();
                blossomSagePatcher.RegisterPatch<BlossomBladesSwordSageAfterCardEnteredCombatPatch>();
                blossomSagePatcher.RegisterPatch<BlossomBladesSwordSageAfterRemovedPatch>();
                if (!RitsuLibFramework.ApplyRequiredPatcher(
                        blossomSagePatcher,
                        () => IsModActive = false,
                        "剑花纷飞与剑圣合并补丁未能应用，本 Mod 已禁用。"))
                    return;

                var plagueResonancePatcher =
                    RitsuLibFramework.CreatePatcher(Const.ModId, "plague_resonance", "荒疫共鸣");
                plagueResonancePatcher.RegisterPatch<PlagueResonanceArtifactScalingPatch>();
                if (!RitsuLibFramework.ApplyRequiredPatcher(
                        plagueResonancePatcher,
                        () => IsModActive = false,
                        "荒疫共鸣相关补丁未能应用，本 Mod 已禁用。"))
                    return;

                AiACardContentRegistrar.RegisterAll();
                ContentOptions.Initialize();
                Utils.SmokeEscape.Initialize();
                var contentPatcher = RitsuLibFramework.CreatePatcher(Const.ModId, "content_options", "Content options");
                contentPatcher.RegisterPatch<ContentOptionsCardPoolPatch>();
                contentPatcher.RegisterPatch<ContentOptionsGenerationPatch>();
                contentPatcher.RegisterPatch<ContentOptionsRelicPoolPatch>();
                contentPatcher.RegisterPatch<ContentOptionsPotionPoolPatch>();
                contentPatcher.RegisterPatch<CardLibraryVisibilityPatch>();
                contentPatcher.RegisterPatch<VeiledFormExtraTurnPatch>();
                contentPatcher.RegisterPatch<VeiledFormPaelsEyePatch>();
                contentPatcher.RegisterPatch<RandomCostStarPatch>();
                contentPatcher.RegisterPatch<DarvOwnedRelicOptionsPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeAlivePatch>();
                contentPatcher.RegisterPatch<SmokeEscapeRunLossPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeTurnPatch>();
                contentPatcher.RegisterPatch<SmokeEscapePlayPatch>();
                contentPatcher.RegisterPatch<SmokeEscapePotionPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeWinCheckPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeEndingPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeRewardPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeParticipantsPatch>();
                contentPatcher.RegisterPatch<SmokeEscapePlayerLookupPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeReadyPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeTargetPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeHealthPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeOrbTurnPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeHoverPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeResourcesPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeMaxHealthPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeKillPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeUiActivationPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeHandInputPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeEndTurnEnabledPatch>();
                contentPatcher.RegisterPatch<SmokeEscapeEndTurnInputPatch>();
                if (!contentPatcher.PatchAll())
                    return;

                IsModActive = true;
                Logger.Info("Mod initialization complete - Mod is now ACTIVE");
            }
            catch (Exception ex)
            {
                Logger.Error($"Mod initialization failed with exception: {ex.Message}");
                Logger.Error($"Stack trace: {ex.StackTrace}");
                IsModActive = false;
            }
        }
    }
}
