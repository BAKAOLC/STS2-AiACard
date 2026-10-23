using Godot;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Ui.Shell.Theme;

namespace STS2_AiACard.Content
{
    internal sealed class ContentOptionsView
    {
        private static string _kind = "Cards";
        private static string _group = "";
        private static string _search = "";
        private static int _status;
        private readonly VBoxContainer _root = new()
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin
        };
        private readonly HFlowContainer _roles = new();
        private readonly HFlowContainer _grid = new()
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            Alignment = FlowContainer.AlignmentMode.Center
        };
        private readonly Label _summary = MakeLabel("", true);
        private readonly Label _empty = MakeLabel("", true);
        private readonly List<(Button Button, string Kind)> _kindButtons = [];
        private readonly List<(Button Button, int Status)> _statusButtons = [];
        private LineEdit _searchBox = null!;

        internal static Control Create() => new ContentOptionsView().Build();

        private Control Build()
        {
            _root.Name = "ContentBrowser";
            _root.AddThemeConstantOverride("separation", 12);
            var hasOtherKinds = ContentOptions.ModelTypes.Any(type => !typeof(CardModel).IsAssignableFrom(type));
            if (!hasOtherKinds)
                _kind = "Cards";
            if (!ContentOptions.CanEdit)
                _root.AddChild(MakeLabel(ContentOptions.Label("content.locked", "Settings locked"), true));
            _searchBox = ModSettingsUiControlTheming.CreateStyledLineEdit(_search,
                ContentOptions.Label("content.search", "Search by name"), 0f);
            _searchBox.Name = "Search";
            _searchBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _searchBox.TextChanged += text =>
            {
                _search = text.Trim();
                RefreshRows();
            };
            _root.AddChild(_searchBox);

            var kinds = Flow();
            kinds.Visible = hasOtherKinds;
            foreach (var kind in new[] { "Cards", "Relics", "Potions" })
            {
                if (!ContentOptions.ModelTypes.Any(type => MatchesCategory(type, kind, "")))
                    continue;
                var label = ContentOptions.Label("content.kind." + kind, kind);
                var count = ContentOptions.ModelTypes.Count(type => MatchesCategory(type, kind, ""));
                var button = ModSettingsUiControlTheming.CreateSettingsToggleButton(label + "  " + count, _kind == kind);
                button.Name = "Category" + kind;
                button.CustomMinimumSize = new(108, 40);
                button.Pressed += () =>
                {
                    _kind = kind;
                    _group = "";
                    RefreshCategories();
                    RefreshRows();
                };
                kinds.AddChild(button);
                _kindButtons.Add((button, kind));
            }
            _root.AddChild(kinds);
            _roles.AddThemeConstantOverride("h_separation", 8);
            _roles.AddThemeConstantOverride("v_separation", 8);
            _root.AddChild(_roles);

            var actions = Flow();
            foreach (var (status, key) in new[] { (0, "All"), (1, "Enabled"), (2, "Disabled") })
            {
                var button = ModSettingsUiControlTheming.CreateSettingsToggleButton(
                    ContentOptions.Label("content.status." + key, key), _status == status);
                button.Name = "Filter" + key;
                button.CustomMinimumSize = new(86, 38);
                button.Pressed += () =>
                {
                    _status = status;
                    foreach (var item in _statusButtons)
                        SetSelected(item.Button, item.Status == _status);
                    RefreshRows();
                };
                _statusButtons.Add((button, status));
                actions.AddChild(button);
            }
            foreach (var enabled in new[] { true, false })
            {
                var button = new ModSettingsMiniButton(
                    ContentOptions.Label(enabled ? "content.batch.enable" : "content.batch.disable",
                        enabled ? "Enable category" : "Disable category"),
                    () =>
                    {
                        ContentOptions.SetEnabledBatch(CategoryTypes(), enabled);
                        RefreshRows();
                    })
                {
                    Name = enabled ? "EnableCategory" : "DisableCategory",
                    Disabled = !ContentOptions.CanEdit,
                    CustomMinimumSize = new(122, 38)
                };
                actions.AddChild(button);
            }
            _root.AddChild(actions);
            _root.AddChild(_summary);
            _grid.Name = "ContentItems";
            _grid.AddThemeConstantOverride("h_separation", 16);
            _grid.AddThemeConstantOverride("v_separation", 20);
            _root.AddChild(_grid);
            _empty.Text = ContentOptions.Label("content.empty", "No matching content.");
            _empty.HorizontalAlignment = HorizontalAlignment.Center;
            _empty.CustomMinimumSize = new(0, 80);
            _root.AddChild(_empty);
            RefreshCategories();
            RefreshRows();
            return _root;
        }

        internal static bool MatchesCategory(Type type, string kind, string group)
        {
            if (kind == "Relics") return typeof(RelicModel).IsAssignableFrom(type);
            if (kind == "Potions") return typeof(PotionModel).IsAssignableFrom(type);
            if (kind != "Cards") return false;
            return typeof(CardModel).IsAssignableFrom(type) &&
                (group.Length == 0 || GroupOf(type) == group);
        }

        private static string GroupOf(Type type) => type.Namespace?.Split('.').Last() ?? "";
        private Type[] CategoryTypes() => ContentOptions.ModelTypes
            .Where(type => MatchesCategory(type, _kind, _group)).ToArray();

        private void RefreshCategories()
        {
            foreach (var item in _kindButtons)
                SetSelected(item.Button, item.Kind == _kind);
            ClearChildren(_roles);
            _roles.Visible = _kind == "Cards";
            if (!_roles.Visible)
                return;
            var groups = new[] { "" }.Concat(new[] { "Ironclad", "Silent", "Regent", "Necrobinder", "Defect", "Colorless" }
                .Where(group => ContentOptions.ModelTypes.Any(type => MatchesCategory(type, "Cards", group))));
            foreach (var group in groups)
            {
                var label = ContentOptions.Label(group.Length == 0 ? "content.roles.all" : "content.group." + group,
                    group.Length == 0 ? "All characters" : group);
                var button = ModSettingsUiControlTheming.CreateSettingsToggleButton(label, _group == group);
                button.Name = "Role" + (group.Length == 0 ? "All" : group);
                button.CustomMinimumSize = new(92, 36);
                button.Pressed += () =>
                {
                    _group = group;
                    RefreshCategories();
                    RefreshRows();
                };
                _roles.AddChild(button);
            }
        }

        private void RefreshRows()
        {
            ClearChildren(_grid);
            var category = CategoryTypes();
            var visible = category.Select(ContentOptions.GetModel)
                .Where(model => ContentOptions.GetTitle(model).Contains(_search, StringComparison.OrdinalIgnoreCase))
                .Where(model => _status == 0 || ContentOptions.IsEnabled(model) == (_status == 1))
                .OrderBy(ContentOptions.GetTitle, StringComparer.CurrentCulture).ToArray();
            foreach (var model in visible)
                _grid.AddChild(CreateRow(model));
            UpdateSummary(category, visible.Length);
            _empty.Visible = visible.Length == 0;
        }

        private Control CreateRow(AbstractModel model)
        {
            var item = new VBoxContainer
            {
                Name = model.GetType().Name,
                CustomMinimumSize = new(ContentOptionsPreview.GetItemWidth(model), 0),
                SizeFlagsHorizontal = Control.SizeFlags.Fill,
                MouseFilter = Control.MouseFilterEnum.Pass
            };
            item.AddThemeConstantOverride("separation", 8);
            var preview = ContentOptions.CreatePreview(model);
            preview.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            item.AddChild(preview);
            if (model is not CardModel)
            {
                var title = MakeLabel(ContentOptions.GetTitle(model));
                title.HorizontalAlignment = HorizontalAlignment.Center;
                title.CustomMinimumSize = new(0, 48);
                title.MaxLinesVisible = 2;
                item.AddChild(title);
            }
            var toggle = ModSettingsUiControlTheming.CreateSettingsToggleButton("", ContentOptions.IsEnabled(model));
            toggle.Name = "Enabled";
            toggle.CustomMinimumSize = new(0, 42);
            toggle.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            toggle.Disabled = !ContentOptions.CanEdit;
            item.AddChild(toggle);
            void RefreshState()
            {
                var enabled = ContentOptions.IsEnabled(model);
                SetSelected(toggle, enabled);
                toggle.Text = ContentOptions.Label(enabled ? "content.status.Enabled" : "content.status.Disabled",
                    enabled ? "Enabled" : "Disabled");
                preview.Modulate = new(1, 1, 1, enabled ? 1f : 0.45f);
            }
            toggle.Toggled += enabled =>
            {
                ContentOptions.SetEnabled(model.GetType(), enabled);
                RefreshState();
                UpdateSummary(CategoryTypes(), _grid.GetChildCount());
                if (_status != 0)
                    Callable.From(RefreshRows).CallDeferred();
            };
            preview.GuiInput += input =>
            {
                if (ContentOptions.CanEdit && input is InputEventMouseButton
                    { ButtonIndex: MouseButton.Left, Pressed: true })
                {
                    toggle.ButtonPressed = !toggle.ButtonPressed;
                    preview.AcceptEvent();
                }
            };
            RefreshState();
            return item;
        }

        private void UpdateSummary(Type[] category, int visible)
        {
            var enabled = category.Count(type => ContentOptions.IsEnabled(ContentOptions.GetModel(type)));
            _summary.Text = ContentOptions.Label("content.summary.visible", "Shown") + " " + visible +
                " / " + category.Length + "    ·    " +
                ContentOptions.Label("content.status.Enabled", "Enabled") + " " + enabled + "    ·    " +
                ContentOptions.Label("content.status.Disabled", "Disabled") + " " + (category.Length - enabled);
        }

        private static HFlowContainer Flow()
        {
            var flow = new HFlowContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            flow.AddThemeConstantOverride("h_separation", 8);
            flow.AddThemeConstantOverride("v_separation", 8);
            return flow;
        }

        private static void SetSelected(Button button, bool selected)
        {
            button.SetPressedNoSignal(selected);
            ModSettingsUiControlTheming.ApplySettingsToggleButtonStyle(button, selected, false);
        }

        private static Label MakeLabel(string text, bool secondary = false)
        {
            var theme = RitsuShellTheme.Current;
            var label = new Label
            {
                Text = text,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            label.AddThemeFontOverride("font", secondary ? theme.Font.Body : theme.Font.BodyBold);
            label.AddThemeFontSizeOverride("font_size", secondary ? theme.Metric.FontSize.Secondary : theme.Metric.FontSize.ValueLabel);
            label.AddThemeColorOverride("font_color", secondary ? theme.Text.LabelSecondary : theme.Text.LabelPrimary);
            return label;
        }

        private static void ClearChildren(Node parent)
        {
            foreach (var child in parent.GetChildren())
            {
                parent.RemoveChild(child);
                child.QueueFree();
            }
        }
    }
}
