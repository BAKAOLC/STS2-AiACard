using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using STS2_AiACard.Utils;
using STS2RitsuLib.Patching.Models;

namespace STS2_AiACard.Patches
{
    public sealed class SmokeEscapeAlivePatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.alive";
        public static string Description => "Keep escaped players out of living combat targets without killing them";
        public static ModPatchTarget[] GetTargets() =>
        [
            new(typeof(Creature), nameof(Creature.IsAlive), Type.EmptyTypes, MethodType.Getter),
            new(typeof(Creature), nameof(Creature.IsHittable), Type.EmptyTypes, MethodType.Getter),
            new(typeof(Creature), nameof(Creature.CanReceivePowers), Type.EmptyTypes, MethodType.Getter)
        ];
        public static void Postfix(Creature __instance, ref bool __result)
        {
            if (SmokeEscape.IsEscaped(__instance))
                __result = false;
        }
    }

    public sealed class SmokeEscapeRunLossPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.run_loss";
        public static string Description => "An escaped living player prevents a total party death";
        public static ModPatchTarget[] GetTargets()
        {
            var targets = typeof(CreatureCmd).GetNestedTypes(BindingFlags.NonPublic)
                .SelectMany(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                .Where(method => method.Name.Contains("<Kill>", StringComparison.Ordinal) &&
                    method.ReturnType == typeof(bool) && method.GetParameters() is [{ ParameterType: var parameter }] &&
                    parameter == typeof(Player))
                .Select(method => new ModPatchTarget(method.DeclaringType!, method.Name, [typeof(Player)])).ToArray();
            if (targets.Length == 0)
                throw new MissingMethodException("CreatureCmd.Kill party-death predicate was not found.");
            return targets;
        }
        public static void Postfix(Player __0, ref bool __result)
        {
            if (SmokeEscape.IsEscaped(__0.Creature) && __0.Creature.CurrentHp > 0)
                __result = false;
        }
    }

    public sealed class SmokeEscapeTurnPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.turn";
        public static string Description => "Skip escaped player turn setup and automatic plays";
        public static ModPatchTarget[] GetTargets() => typeof(CombatManager)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(method => method.Name is "SetupPlayerTurn" or "RunAutoPrePlayPhase" or "DoTurnEnd")
            .Select(method => new ModPatchTarget(typeof(CombatManager), method.Name,
                method.GetParameters().Select(parameter => parameter.ParameterType).ToArray())).ToArray();
        public static bool Prefix(object[] __args, ref Task __result)
        {
            var player = __args.OfType<Player>().Single();
            if (!SmokeEscape.IsEscaped(player.Creature))
                return true;
            __result = Task.CompletedTask;
            return false;
        }
    }

    public sealed class SmokeEscapePlayPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.play";
        public static string Description => "Prevent escaped players from playing cards";
        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(CardModel), nameof(CardModel.CanPlay), Type.EmptyTypes)];
        public static void Postfix(CardModel __instance, ref bool __result)
        {
            if (__instance.Owner != null && SmokeEscape.IsEscaped(__instance.Owner.Creature))
                __result = false;
        }
    }

    public sealed class SmokeEscapePotionPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.potion";
        public static string Description => "Prevent escaped players from using potions";
        public static ModPatchTarget[] GetTargets() => typeof(PotionModel).Assembly.GetTypes()
            .Where(type => typeof(PotionModel).IsAssignableFrom(type))
            .Where(type => type.GetProperty(nameof(PotionModel.PassesCustomUsabilityCheck),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) != null)
            .Select(type => new ModPatchTarget(type, nameof(PotionModel.PassesCustomUsabilityCheck),
                Type.EmptyTypes, MethodType.Getter)).ToArray();
        public static void Postfix(PotionModel __instance, ref bool __result)
        {
            if (__instance.IsMutable && SmokeEscape.IsEscaped(__instance.Owner.Creature))
                __result = false;
        }
    }

    public sealed class SmokeEscapeWinCheckPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.finish";
        public static string Description => "End combat when nobody remains to fight";
        public static ModPatchTarget[] GetTargets() => typeof(CombatManager)
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(method => method.Name == nameof(CombatManager.CheckWinCondition))
            .Select(method => new ModPatchTarget(typeof(CombatManager), method.Name,
                method.GetParameters().Select(parameter => parameter.ParameterType).ToArray())).ToArray();
        public static void Prefix(CombatManager __instance)
        {
            if (__instance.DebugOnlyGetState() is { } state)
                SmokeEscape.LeaveIfNobodyRemains(state);
        }
    }

    public sealed class SmokeEscapeEndingPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.ending";
        public static string Description => "Restore escaped players for normal combat teardown";
        public static ModPatchTarget[] GetTargets() => typeof(CombatManager)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(method => method.Name == "EndCombatInternal")
            .Select(method => new ModPatchTarget(typeof(CombatManager), method.Name,
                method.GetParameters().Select(parameter => parameter.ParameterType).ToArray())).ToArray();
        public static void Prefix() => SmokeEscape.BeginEnding();
    }

    public sealed class SmokeEscapeRewardPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.rewards";
        public static string Description => "A party that withdraws without victory receives no combat settlement rewards";
        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(RewardsSet), nameof(RewardsSet.WithRewardsFromRoom), [typeof(AbstractRoom)])];
        public static bool Prefix(RewardsSet __instance, AbstractRoom room, ref RewardsSet __result)
        {
            if (room is not CombatRoom combat || !SmokeEscape.HasNoReward(__instance.Player, combat))
                return true;
            __result = __instance.EmptyForRoom(room);
            if (combat.ExtraRewards.TryGetValue(__instance.Player, out var extra))
                __result.Rewards.AddRange(extra);
            return false;
        }
    }

}
