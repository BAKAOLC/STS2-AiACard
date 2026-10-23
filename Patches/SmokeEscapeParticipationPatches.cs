using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2_AiACard.Utils;
using STS2RitsuLib.Patching.Models;

namespace STS2_AiACard.Patches
{
    public sealed class SmokeEscapeParticipantsPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.participants";
        public static string Description => "Exclude escaped players from combat participation";
        public static ModPatchTarget[] GetTargets() =>
        [
            new(typeof(CombatState), nameof(CombatState.Creatures), Type.EmptyTypes, MethodType.Getter),
            new(typeof(CombatState), nameof(CombatState.Allies), Type.EmptyTypes, MethodType.Getter)
        ];
        public static void Postfix(ref IReadOnlyList<Creature> __result)
        {
            if (__result.Any(SmokeEscape.IsEscaped))
                __result = __result.Where(creature => !SmokeEscape.IsEscaped(creature)).ToArray();
        }
    }

    public sealed class SmokeEscapePlayerLookupPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.identity";
        public static string Description => "Preserve escaped player identity for synchronization";
        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(CombatState), nameof(CombatState.GetPlayer), [typeof(ulong)])];
        public static void Postfix(CombatState __instance, ulong __0, ref Player? __result)
        {
            if (__result != null)
                return;
            var player = __instance.RunState.Players.FirstOrDefault(player => player.NetId == __0);
            if (player != null && SmokeEscape.IsEscaped(player.Creature))
                __result = player;
        }
    }

    public sealed class SmokeEscapeReadyPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.ready";
        public static string Description => "Do not submit turn readiness for escaped players";
        public static ModPatchTarget[] GetTargets() => typeof(CombatManager)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.Name is nameof(CombatManager.SetReadyToEndTurn) or
                nameof(CombatManager.SetReadyToBeginEnemyTurn) or nameof(CombatManager.UndoReadyToEndTurn))
            .Select(method => new ModPatchTarget(typeof(CombatManager), method.Name,
                method.GetParameters().Select(parameter => parameter.ParameterType).ToArray())).ToArray();
        public static bool Prefix(Player player) => !SmokeEscape.IsEscaped(player.Creature);
    }

    public sealed class SmokeEscapeTargetPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.targets";
        public static string Description => "Reject escaped owners and targets";
        public static ModPatchTarget[] GetTargets() =>
        [
            new(typeof(CardModel), nameof(CardModel.IsValidTarget), [typeof(Creature)]),
            new(typeof(PotionModel), nameof(PotionModel.IsValidTarget), [typeof(Creature)])
        ];
        public static void Postfix(AbstractModel __instance, Creature? target, ref bool __result)
        {
            var owner = __instance switch
            {
                CardModel card when card.IsMutable => card.Owner,
                PotionModel potion when potion.IsMutable => potion.Owner,
                _ => null
            };
            if (target != null && SmokeEscape.IsEscaped(target) ||
                owner != null && SmokeEscape.IsEscaped(owner.Creature))
                __result = false;
        }
    }

    public sealed class SmokeEscapeHealthPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.health";
        public static string Description => "Do not heal or change health of escaped players";
        public static ModPatchTarget[] GetTargets() => typeof(CreatureCmd)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.ReturnType == typeof(Task) && method.Name is
                nameof(CreatureCmd.Heal) or nameof(CreatureCmd.SetCurrentHp) or
                nameof(CreatureCmd.GainMaxHp) or nameof(CreatureCmd.LoseMaxHp) or
                nameof(CreatureCmd.SetMaxAndCurrentHp))
            .Select(method => new ModPatchTarget(typeof(CreatureCmd), method.Name,
                method.GetParameters().Select(parameter => parameter.ParameterType).ToArray())).ToArray();
        public static bool Prefix(Creature creature, ref Task __result)
        {
            if (!SmokeEscape.IsEscaped(creature))
                return true;
            __result = Task.CompletedTask;
            return false;
        }
    }

    public sealed class SmokeEscapeResourcesPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.resources";
        public static string Description => "Do not modify escaped player combat resources";
        public static ModPatchTarget[] GetTargets() => typeof(PlayerCmd)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.Name is nameof(PlayerCmd.GainEnergy) or nameof(PlayerCmd.LoseEnergy) or
                nameof(PlayerCmd.SetEnergy) or nameof(PlayerCmd.GainStars) or nameof(PlayerCmd.LoseStars) or
                nameof(PlayerCmd.SetStars))
            .Select(method => new ModPatchTarget(typeof(PlayerCmd), method.Name,
                method.GetParameters().Select(parameter => parameter.ParameterType).ToArray())).ToArray();
        public static bool Prefix(Player player, ref Task __result)
        {
            if (!SmokeEscape.IsEscaped(player.Creature))
                return true;
            __result = Task.CompletedTask;
            return false;
        }
    }

    public sealed class SmokeEscapeMaxHealthPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.max_health";
        public static string Description => "Preserve escaped player maximum health";
        public static ModPatchTarget[] GetTargets() =>
            [new(typeof(CreatureCmd), nameof(CreatureCmd.SetMaxHp), [typeof(Creature), typeof(decimal)])];
        public static bool Prefix(Creature creature, ref Task<decimal> __result)
        {
            if (!SmokeEscape.IsEscaped(creature))
                return true;
            __result = Task.FromResult((decimal)creature.MaxHp);
            return false;
        }
    }

    public sealed class SmokeEscapeKillPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.kill";
        public static string Description => "Exclude escaped players from forced kills";
        public static ModPatchTarget[] GetTargets() => typeof(CreatureCmd)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.Name == nameof(CreatureCmd.Kill))
            .Select(method => new ModPatchTarget(typeof(CreatureCmd), method.Name,
                method.GetParameters().Select(parameter => parameter.ParameterType).ToArray())).ToArray();
        public static bool Prefix(object[] __args, ref Task __result)
        {
            for (var index = 0; index < __args.Length; index++)
            {
                if (__args[index] is Creature creature && SmokeEscape.IsEscaped(creature))
                {
                    __result = Task.CompletedTask;
                    return false;
                }
                if (__args[index] is IReadOnlyCollection<Creature> creatures && creatures.Any(SmokeEscape.IsEscaped))
                    __args[index] = creatures.Where(creature => !SmokeEscape.IsEscaped(creature)).ToArray();
            }
            return true;
        }
    }

    public sealed class SmokeEscapeOrbTurnPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.orb_turn";
        public static string Description => "Skip escaped player orb turns";
        public static ModPatchTarget[] GetTargets() => typeof(OrbQueue)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.Name is nameof(OrbQueue.AfterTurnStart) or nameof(OrbQueue.BeforeTurnEnd))
            .Select(method => new ModPatchTarget(typeof(OrbQueue), method.Name,
                method.GetParameters().Select(parameter => parameter.ParameterType).ToArray())).ToArray();
        public static bool Prefix(Player ____owner, ref Task __result)
        {
            if (!SmokeEscape.IsEscaped(____owner.Creature))
                return true;
            __result = Task.CompletedTask;
            return false;
        }
    }

    public sealed class SmokeEscapeHoverPatch : IPatchMethod
    {
        public static string PatchId => Const.ModId + ".smoke.hover";
        public static string Description => "Do not focus or display escaped creature hover tips";
        public static ModPatchTarget[] GetTargets() => typeof(NCreature)
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(method => method.Name is "OnFocus" or nameof(NCreature.ShowHoverTips))
            .Select(method => new ModPatchTarget(typeof(NCreature), method.Name,
                method.GetParameters().Select(parameter => parameter.ParameterType).ToArray())).ToArray();
        public static bool Prefix(NCreature __instance) => !SmokeEscape.IsEscaped(__instance.Entity);
    }
}
