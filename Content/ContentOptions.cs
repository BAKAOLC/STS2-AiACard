using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;
using STS2RitsuLib.Networking.Sidecar;
using STS2RitsuLib.RunData;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;

namespace STS2_AiACard.Content
{
    public sealed class ContentOptionsState
    {
        public HashSet<string> Disabled { get; set; } = [];
        internal bool IsValid => Disabled != null && Disabled.Count <= 512;

        public ContentOptionsState Copy() => new()
        {
            Disabled = new(Disabled ?? [], StringComparer.Ordinal),
        };
    }

    internal static class ContentOptions
    {
        private const string DataKey = "content-options";
        private static readonly List<IDisposable> Subscriptions = [];
        private static readonly List<WeakReference<IModSettingsUiActionHost>> UiHosts = [];
        private static readonly RitsuLibSidecarJsonSerializer<ContentOptionsState> Serializer = new();
        private static readonly RitsuLibSidecarMessageDescriptor<ContentOptionsState> Snapshot = new(
            Const.ModId, "content-options", Serializer.Serialize, Serializer.Deserialize, Required: true);
        private static RunSavedData<ContentOptionsState> _runOptions = null!;
        private static INetGameService? _net;
        private static StartRunLobby? _lobby;
        private static RunState? _run;
        private static ContentOptionsState? _remote;

        private static ContentOptionsState Local => RitsuLibFramework.GetDataStore(Const.ModId)
            .Get<ContentOptionsState>(DataKey);

        private static ContentOptionsState Current => _run != null && _runOptions.TryGet(_run, out var saved)
            ? saved : _net?.Type == NetGameType.Client ? _remote ?? new() : Local;

        internal static bool CanEdit => RunManager.Instance?.IsInProgress != true &&
            _net?.Type != NetGameType.Client;

        internal static bool IsEnabled(AbstractModel model) => model.GetType().Assembly != typeof(Main).Assembly ||
            Current.Disabled?.Contains(model.GetType().FullName!) != true;

        internal static IReadOnlyList<Type> ModelTypes { get; } = typeof(Main).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && (typeof(CardModel).IsAssignableFrom(t) ||
                typeof(RelicModel).IsAssignableFrom(t) || typeof(PotionModel).IsAssignableFrom(t)))
            .Where(t => t.Name != "MpCorpseCurseToken")
            .OrderBy(t => t.FullName, StringComparer.Ordinal).ToArray();

        internal static void Initialize()
        {
            RitsuLibFramework.GetDataStore(Const.ModId).Register<ContentOptionsState>(DataKey,
                "content-options.json", SaveScope.Global);
            _runOptions = RunSavedDataStore.For(Const.ModId).Register(DataKey, () => Local.Copy());
            Subscriptions.Add(RitsuLibSidecarTypedMessageRegistry.Subscribe(Snapshot, ctx =>
            {
                if (_net is not NetClientGameService { IsConnected: true } client ||
                    ctx.SenderNetId != client.HostNetId || ctx.Message is not { IsValid: true })
                    return;
                _remote = ctx.Message.Copy();
                RefreshUi();
            }));
            Subscriptions.Add(RitsuLibSidecarEvents.OnSessionBound(evt =>
            {
                _net = evt.NetService;
                _remote = null;
                _lobby = null;
                _run = null;
                RefreshUi();
            }));
            Subscriptions.Add(RitsuLibSidecarEvents.OnSessionUnbound(_ =>
            {
                _net = null;
                _remote = null;
                _lobby = null;
                _run = null;
                RefreshUi();
            }));
            Subscriptions.Add(RitsuLibSidecarEvents.OnHandshakeCompleted(evt =>
            {
                if (_net?.Type == NetGameType.Host)
                    RitsuLibSidecarTypedMessageRegistry.SendToPeer(_net, evt.PeerNetId, Snapshot, Current.Copy());
            }));
            Subscriptions.Add(RitsuLibFramework.SubscribeLifecycle<RunSavedDataLobbyStagingEvent>(evt =>
            {
                _lobby = evt.Lobby;
                _net = evt.Lobby.NetService;
                if (evt.IsHost || !evt.IsMultiplayer)
                    _runOptions.Lobby.Set(evt.Lobby, Local.Copy());
            }));
            Subscriptions.Add(RitsuLibFramework.SubscribeLifecycle<RunStartedEvent>(evt =>
            {
                _run = evt.RunState;
                _runOptions.Get(evt.RunState);
                _lobby = null;
                RefreshUi();
            }));
            Subscriptions.Add(RitsuLibFramework.SubscribeLifecycle<RunLoadedEvent>(evt =>
            {
                _run = evt.RunState;
                _lobby = null;
                RefreshUi();
            }));
            Subscriptions.Add(RitsuLibFramework.SubscribeLifecycle<RunEndedEvent>(_ =>
            {
                _run = null;
                RefreshUi();
            }));

            RitsuLibFramework.RegisterModSettings(Const.ModId, page =>
            {
                page.WithTitle(Text("content.title", "Content"))
                    .WithModDisplayName(ModSettingsText.Literal(Const.Name))
                    .WithDescription(Text("content.description", "The host chooses content before the run."));
                page.AddSection("content", section =>
                {
                    section.WithTitle(Text("content.browser.title", "Content browser"));
                    section.AddCustom("browser", Text("content.browser.title", "Content browser"), host =>
                    {
                        UiHosts.Add(new(host));
                        return ContentOptionsView.Create();
                    });
                });
            });
        }

        internal static AbstractModel GetModel(Type type) => ModelDb.GetById<AbstractModel>(ModelDb.GetId(type));

        internal static string GetTitle(AbstractModel model) => model switch
        {
            CardModel card => card.Title,
            RelicModel relic => relic.Title.GetFormattedText(),
            PotionModel potion => potion.Title.GetFormattedText(),
            _ => model.Id.Entry
        };

        internal static Control CreatePreview(AbstractModel model) => ContentOptionsPreview.Create(model);

        internal static void SetEnabled(Type type, bool enabled) => SetEnabledBatch([type], enabled);

        internal static void SetEnabledBatch(IEnumerable<Type> types, bool enabled)
        {
            var ownedTypes = types.Where(ModelTypes.Contains).ToArray();
            if (ownedTypes.Length == 0)
                return;
            ChangeLocal(state =>
            {
                state.Disabled ??= [];
                foreach (var type in ownedTypes)
                    if (enabled)
                        state.Disabled.Remove(type.FullName!);
                    else
                        state.Disabled.Add(type.FullName!);
            });
        }

        private static void ChangeLocal(Action<ContentOptionsState> change)
        {
            if (!CanEdit)
                return;
            change(Local);
            if (_lobby != null)
                _runOptions.Lobby.Set(_lobby, Local.Copy());
            if (_net?.Type == NetGameType.Host)
                RitsuLibSidecarTypedMessageRegistry.Broadcast(_net, Snapshot, Local.Copy());
            Save();
        }

        private static void Save()
        {
            if (CanEdit)
                RitsuLibFramework.GetDataStore(Const.ModId).Save(DataKey);
        }

        internal static ModSettingsText Text(string key, string fallback) =>
            ModSettingsText.LocString("settings_ui", Const.ModId + "." + key, fallback);

        internal static string Label(string key, string fallback) => Text(key, fallback).Resolve();

        private static void RefreshUi()
        {
            UiHosts.RemoveAll(reference => !reference.TryGetTarget(out _));
            foreach (var reference in UiHosts)
                if (reference.TryGetTarget(out var host))
                    host.RequestRefresh();
        }
    }
}
