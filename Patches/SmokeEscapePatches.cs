using System.Reflection;
using System.Runtime.ExceptionServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;
using STS2_AiACard.Utils;

namespace STS2_AiACard.Patches
{
    public sealed class SmokeEscapeAlivePatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.alive";
        public static string Description => "Keep escaped players out of living combat targets without killing them";

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(Creature), nameof(Creature.IsAlive), Type.EmptyTypes, MethodType.Getter),
                new(typeof(Creature), nameof(Creature.IsHittable), Type.EmptyTypes, MethodType.Getter),
                new(typeof(Creature), nameof(Creature.CanReceivePowers), Type.EmptyTypes, MethodType.Getter),
            ];
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(Creature __instance, ref bool __result)
        {
            if (SmokeEscape.IsEscaped(__instance))
                __result = false;
        }
        // ReSharper restore InconsistentNaming
    }

    public sealed class SmokeEscapeRunLossPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.run_loss";
        public static string Description => "An escaped living player prevents a total party death";

        public static ModPatchTarget[] GetTargets()
        {
            var targets = typeof(CreatureCmd).GetNestedTypes(BindingFlags.NonPublic)
                .SelectMany(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public |
                                                    BindingFlags.Instance | BindingFlags.Static))
                .Where(method => method.Name.Contains("<Kill>", StringComparison.Ordinal) &&
                                 method.ReturnType == typeof(bool) && method.GetParameters() is
                                     [{ ParameterType: var parameter }] &&
                                 parameter == typeof(Player))
                .Select(method => new ModPatchTarget(method.DeclaringType!, method.Name, [typeof(Player)])).ToArray();
            return targets.Length > 0
                ? targets
                : throw new MissingMethodException("CreatureCmd.Kill party-death predicate was not found.");
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(Player __0, ref bool __result)
        {
            if (SmokeEscape.IsEscaped(__0.Creature) && __0.Creature.CurrentHp > 0)
                __result = false;
        }
        // ReSharper restore InconsistentNaming
    }

    public sealed class SmokeEscapeTurnPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.turn";
        public static string Description => "Skip escaped player turn setup and automatic plays";

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                .. typeof(CombatManager)
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(method => method.Name is "SetupPlayerTurn" or "RunAutoPrePlayPhase" or "DoTurnEnd")
                    .Select(method => new ModPatchTarget(typeof(CombatManager), method.Name,
                        [.. method.GetParameters().Select(parameter => parameter.ParameterType)])),
            ];
        }

        // ReSharper disable InconsistentNaming
        public static bool Prefix(object[] __args, ref Task __result)
        {
            var player = __args.OfType<Player>().Single();
            if (!SmokeEscape.IsEscaped(player.Creature))
                return true;
            __result = Task.CompletedTask;
            return false;
        }
        // ReSharper restore InconsistentNaming
    }

    public sealed class SmokeEscapePlayPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.play";
        public static string Description => "Prevent escaped players from playing cards";

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(CardModel), nameof(CardModel.CanPlay), Type.EmptyTypes)];
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(CardModel __instance, ref bool __result)
        {
            if (SmokeEscape.IsEscaped(__instance.Owner.Creature))
                __result = false;
        }
        // ReSharper restore InconsistentNaming
    }

    public sealed class SmokeEscapePotionPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.potion";
        public static string Description => "Prevent escaped players from using potions";

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                .. typeof(PotionModel).Assembly.GetTypes()
                    .Where(type => typeof(PotionModel).IsAssignableFrom(type))
                    .Where(type => type.GetProperty(nameof(PotionModel.PassesCustomUsabilityCheck),
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) != null)
                    .Select(type => new ModPatchTarget(type, nameof(PotionModel.PassesCustomUsabilityCheck),
                        Type.EmptyTypes, MethodType.Getter)),
            ];
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(PotionModel __instance, ref bool __result)
        {
            if (__instance.IsMutable && SmokeEscape.IsEscaped(__instance.Owner.Creature))
                __result = false;
        }
        // ReSharper restore InconsistentNaming
    }

    public sealed class SmokeEscapeWinCheckPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.finish";
        public static string Description => "End combat when nobody remains to fight";

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                .. typeof(CombatManager)
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(method => method.Name == nameof(CombatManager.CheckWinCondition))
                    .Select(method => new ModPatchTarget(typeof(CombatManager), method.Name,
                        [.. method.GetParameters().Select(parameter => parameter.ParameterType)])),
            ];
        }

        // ReSharper disable InconsistentNaming
        public static bool Prefix(CombatManager __instance, MethodBase __originalMethod, object[] __args,
            ref Task<bool> __result)
        {
            var combat = __instance.DebugOnlyGetState();
            if (combat == null)
                return true;
#if STS2_AT_LEAST_0_113_0
            if (__args is [CombatTurnState turnState] && turnState.State != combat)
                return true;
#endif
            var withdrawal = SmokeEscape.GetWithdrawal(combat);
            if (withdrawal == null)
                return true;
            __result = CheckAfterWithdrawal(withdrawal, __instance, __originalMethod, __args, combat);
            return false;
        }
        // ReSharper restore InconsistentNaming

        private static async Task<bool> CheckAfterWithdrawal(Task withdrawal, CombatManager manager,
            MethodBase originalMethod, object[] args, ICombatState combat)
        {
            await withdrawal;
            if (manager.DebugOnlyGetState() != combat)
                return false;
            Task<bool> check;
            try
            {
                check = (Task<bool>)originalMethod.Invoke(manager, args)!;
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }

            return await check;
        }
    }

    public sealed class SmokeEscapeEndingPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.ending";
        public static string Description => "Restore escaped players for normal combat teardown";

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                .. typeof(CombatManager)
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(method => method.Name == "EndCombatInternal")
                    .Select(method => new ModPatchTarget(typeof(CombatManager), method.Name,
                        [.. method.GetParameters().Select(parameter => parameter.ParameterType)])),
            ];
        }

        public static void Prefix()
        {
            SmokeEscape.BeginEnding();
        }
    }

    public sealed class SmokeEscapeHealingPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.healing";
        public static string Description => "Heal escaped players before combat settlement is saved";

        public static ModPatchTarget[] GetTargets()
        {
            return
            [
                new(typeof(Hook), nameof(Hook.AfterCombatEnd),
                    [typeof(IRunState), typeof(ICombatState), typeof(CombatRoom)]),
            ];
        }

        // ReSharper disable InconsistentNaming
        public static void Postfix(ICombatState? combatState, ref Task __result)
        {
            __result = SmokeEscape.HealAfterEscape(__result, combatState);
        }
        // ReSharper restore InconsistentNaming
    }

    public sealed class SmokeEscapeRewardPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.rewards";

        public static string Description =>
            "A party that withdraws without victory receives no combat settlement rewards";

        public static ModPatchTarget[] GetTargets()
        {
            return [new(typeof(RewardsSet), nameof(RewardsSet.WithRewardsFromRoom), [typeof(AbstractRoom)])];
        }

        // ReSharper disable InconsistentNaming
        public static bool Prefix(RewardsSet __instance, AbstractRoom room, ref RewardsSet __result)
        {
            if (room is not CombatRoom combat || !SmokeEscape.HasNoReward(__instance.Player, combat))
                return true;
            __result = __instance.EmptyForRoom(room);
            if (combat.ExtraRewards.TryGetValue(__instance.Player, out var extra))
                __result.Rewards.AddRange(extra);
            return false;
        }
        // ReSharper restore InconsistentNaming
    }
}
