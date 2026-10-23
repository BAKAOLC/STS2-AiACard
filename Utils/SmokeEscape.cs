using STS2RitsuLib.Utils;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.RunData;

namespace STS2_AiACard.Utils
{
    public sealed class SmokeEscapeSave
    {
        public int Floor { get; set; } = -1;
        public bool NoRewards { get; set; }
    }

    internal static class SmokeEscape
    {
        private sealed class EscapeState
        {
            internal bool Active = true;
            internal HashSet<Player> Players { get; } = [];
        }

        private static readonly AttachedState<ICombatState, EscapeState> States = new();
        private static RunSavedData<SmokeEscapeSave> _saved = null!;

        internal static void Initialize()
        {
            _saved = RunSavedDataStore.For(Const.ModId).Register("smoke-escape", () => new SmokeEscapeSave());
            CombatManager.Instance.CombatEnded += room =>
            {
                if (!States.TryGetValue(room.CombatState, out var escaped))
                    return;
                escaped.Active = false;
                foreach (var player in escaped.Players)
                    player.ActivateHooks();
            };
        }

        internal static bool IsEscaped(Creature creature) => creature.CombatState != null &&
            States.TryGetValue(creature.CombatState, out var state) && state.Active &&
            creature.Player != null && state.Players.Contains(creature.Player);

        internal static bool IsLocalEscaped => CombatManager.Instance.IsInProgress &&
            CombatManager.Instance.DebugOnlyGetState() is { } combat &&
            States.TryGetValue(combat, out var state) && state.Players.Any(LocalContext.IsMe);

        internal static void HideLocalCombatUi(NCombatUi? ui)
        {
            if (ui == null || !IsLocalEscaped)
                return;
            ui.Hand.CancelAllCardPlay();
            ui.Hand.DisableControllerNavigation();
            ui.Hand.SetProcessUnhandledInput(false);
            ui.Hand.Hide();
            ui.EndTurnButton.Disable();
            if (ui.EndTurnButton.IsInsideTree())
                ui.EndTurnButton.ReleaseFocus();
            ui.EndTurnButton.FocusMode = Control.FocusModeEnum.None;
            ui.Hide();
        }

        internal static bool HasNoReward(Player player, CombatRoom room)
        {
            if (player.RunState is not RunState run)
                return false;
            var data = _saved.Get(run);
            return data.Floor == player.RunState.TotalFloor && data.NoRewards;
        }

        internal static async Task Escape(Player player)
        {
            var combat = player.Creature.CombatState;
            if (combat == null || IsEscaped(player.Creature) || player.RunState is not RunState run)
                return;
            var data = _saved.Get(run);
            if (data.Floor != player.RunState.TotalFloor)
            {
                data.Floor = player.RunState.TotalFloor;
                data.NoRewards = false;
            }
            var state = States.GetOrAdd(combat, static _ => new());
            var listeners = combat.IterateHookListeners().Where(model => IsOwnedBy(model, player)).ToArray();
            if (player.RunState.CurrentRoom is CombatRoom room)
                foreach (var model in listeners)
                {
                    await model.AfterCombatEnd(room);
                    model.InvokeExecutionFinished();
                }
            foreach (var pet in player.PlayerCombatState!.Pets.ToList())
                await CreatureCmd.Escape(pet);
            player.AfterCombatEnd();
            state.Players.Add(player);
            player.DeactivateHooks();
            player.PlayerCombatState.Phase = PlayerTurnPhase.None;
            player.PlayerCombatState.OrbQueue.Clear();
            HideLocalCombatUi(NCombatRoom.Instance?.Ui);
            RemoveFromTurnBarriers(player);
            var node = NCombatRoom.Instance?.GetCreatureNode(player.Creature);
            if (node != null)
            {
                node.ToggleIsInteractable(false);
                node.Hitbox.ReleaseFocus();
                node.HideHoverTips();
                await AnimateExit(node, node.Hitbox.Size.X);
            }
            LeaveIfNobodyRemains(combat);
            await CombatManager.Instance.CheckWinCondition();
        }

        internal static async Task AnimateExit(Control node, float width)
        {
            var viewport = node.GetViewportRect();
            var offscreen = node.GetCanvasTransform().AffineInverse() *
                new Vector2(viewport.Position.X - width - 160f, 0f);
            var tween = node.CreateTween();
            tween.TweenProperty(node, "global_position:x", offscreen.X, 0.6)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
            await node.ToSignal(tween, Tween.SignalName.Finished);
            if (GodotObject.IsInstanceValid(node))
                node.Visible = false;
        }

        internal static void LeaveIfNobodyRemains(ICombatState combat)
        {
            if (!States.TryGetValue(combat, out var state) || !state.Active ||
                combat.Players.Any(player => !state.Players.Contains(player) && player.Creature.CurrentHp > 0))
                return;
            if (combat.RunState is RunState run)
                _saved.Get(run).NoRewards = true;
            foreach (var enemy in combat.Enemies.ToList())
                CreatureCmd.Escape(enemy).GetAwaiter().GetResult();
        }

        internal static void BeginEnding()
        {
            var combat = CombatManager.Instance.DebugOnlyGetState();
            if (combat != null && States.TryGetValue(combat, out var state))
            {
                state.Active = false;
            }
        }

        private static bool IsOwnedBy(AbstractModel model, Player player) => model switch
        {
            RelicModel relic => relic.Owner == player,
            PotionModel potion => potion.Owner == player,
            PowerModel power => power.Owner == player.Creature,
            CardModel card => card.Owner == player,
            OrbModel orb => player.PlayerCombatState!.OrbQueue.Orbs.Contains(orb),
            _ => false
        };

        internal static void RemoveFromTurnBarriers(Player player)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var manager = CombatManager.Instance;
            var turn = typeof(CombatManager).GetField("_turnState", flags)?.GetValue(manager);
            var owner = turn ?? manager;
            var gate = owner.GetType().GetProperty("ReadyLock", flags)?.GetValue(owner) ??
                typeof(CombatManager).GetField("_playerReadyLock", flags)?.GetValue(manager) ??
                throw new MissingFieldException("Combat ready lock was not found.");
            Player? readyPlayer = null;
            if (gate is System.Threading.Lock readyLock)
            {
                using (readyLock.EnterScope())
                    RemoveEntries();
            }
            else
                lock (gate)
                    RemoveEntries();
            if (readyPlayer != null)
                manager.SetReadyToEndTurn(readyPlayer, false);

            void RemoveEntries()
            {
                foreach (var (property, field) in new[]
                         {
                             ("PlayersReadyToEndTurn", "_playersReadyToEndTurn"),
                             ("PlayersReadyToBeginEnemyTurn", "_playersReadyToBeginEnemyTurn"),
                             ("PlayersTakingExtraTurn", "_playersTakingExtraTurn")
                         })
                {
                    var collection = owner.GetType().GetProperty(property, flags)?.GetValue(owner) ??
                        owner.GetType().GetField(field, flags)?.GetValue(owner) ??
                        throw new MissingFieldException("Combat turn membership was not found: " + property);
                    if (collection is ICollection<Player> players)
                        players.Remove(player);
                    else
                        throw new InvalidOperationException("Unsupported combat turn membership: " + property);
                }
                var combat = manager.DebugOnlyGetState();
                var active = combat?.Players.Where(player => !SmokeEscape.IsEscaped(player.Creature)).ToArray();
                var ready = owner.GetType().GetProperty("PlayersReadyToEndTurn", flags)?.GetValue(owner) ??
                    owner.GetType().GetField("_playersReadyToEndTurn", flags)?.GetValue(owner);
                if (combat?.CurrentSide == CombatSide.Player && active is { Length: > 0 } &&
                    ready is ICollection<Player> readyPlayers && active.All(readyPlayers.Contains))
                {
                    readyPlayer = active[0];
                    readyPlayers.Remove(readyPlayer);
                }
            }
        }
    }
}
