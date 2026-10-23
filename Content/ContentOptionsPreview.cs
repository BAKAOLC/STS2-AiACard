using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace STS2_AiACard.Content
{
    internal static class ContentOptionsPreview
    {
        private const float CardScale = 0.68f;

        internal static float GetItemWidth(AbstractModel model) => model switch
        {
            CardModel => 228,
            RelicModel => 164,
            PotionModel => 148,
            _ => throw new ArgumentException("Unsupported content model.", nameof(model))
        };

        internal static Control Create(AbstractModel model)
        {
            var preview = new Control
            {
                Name = "ContentPreview",
                CustomMinimumSize = new(GetItemWidth(model), model is CardModel ? NCard.defaultSize.Y * CardScale + 24 : 128),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                MouseFilter = Control.MouseFilterEnum.Stop,
                MouseDefaultCursorShape = Control.CursorShape.PointingHand,
                FocusMode = Control.FocusModeEnum.All
            };
            Control visual;
            switch (model)
            {
                case CardModel card:
                    var cardNode = PreloadManager.Cache.GetScene("res://scenes/cards/card.tscn").Instantiate<NCard>();
                    cardNode.Model = card.ToMutable();
                    visual = cardNode;
                    break;
                case RelicModel relic:
                    var relicNode = PreloadManager.Cache.GetScene("res://scenes/relics/relic.tscn").Instantiate<NRelic>();
                    relicNode.Model = relic;
                    visual = relicNode;
                    break;
                case PotionModel potion:
                    var potionNode = PreloadManager.Cache.GetScene("res://scenes/potions/potion.tscn").Instantiate<NPotion>();
                    potionNode.Model = potion;
                    visual = potionNode;
                    break;
                default:
                    throw new ArgumentException("Unsupported content model.", nameof(model));
            }
            visual.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopLeft);
            IgnoreInput(visual);
            visual.Ready += () =>
            {
                if (visual is NCard card)
                    card.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
                Layout();
            };
            preview.AddChild(visual);
            preview.Resized += Layout;
            preview.MouseEntered += Show;
            preview.FocusEntered += Show;
            preview.MouseExited += Hide;
            preview.FocusExited += Hide;
            var scrolls = new List<ScrollContainer>();
            preview.TreeEntered += () =>
            {
                for (var parent = preview.GetParent(); parent != null; parent = parent.GetParent())
                    if (parent is ScrollContainer scroll)
                    {
                        scroll.GetVScrollBar().ValueChanged += OnScroll;
                        scroll.GetHScrollBar().ValueChanged += OnScroll;
                        scrolls.Add(scroll);
                    }
            };
            preview.TreeExiting += () =>
            {
                Hide();
                foreach (var scroll in scrolls)
                    if (GodotObject.IsInstanceValid(scroll))
                    {
                        scroll.GetVScrollBar().ValueChanged -= OnScroll;
                        scroll.GetHScrollBar().ValueChanged -= OnScroll;
                    }
                scrolls.Clear();
            };
            return preview;

            void Layout()
            {
                if (!visual.IsNodeReady())
                    return;
                if (visual is NCard)
                {
                    visual.Scale = Vector2.One * CardScale;
                    visual.Position = preview.Size * 0.5f;
                }
                else
                {
                    var size = visual.GetCombinedMinimumSize();
                    visual.Size = size;
                    visual.Scale = Vector2.One * (96f / Math.Max(1, size.X));
                    visual.Position = (preview.Size - size * visual.Scale) * 0.5f;
                }
            }

            void Show()
            {
                Hide();
                var tip = NHoverTipSet.CreateAndShow(preview, GetHoverTips(model),
                    HoverTip.GetHoverTipAlignment(preview, 0.5f));
                tip?.SetFollowOwner();
            }
            void Hide() => NHoverTipSet.Remove(preview);
            void OnScroll(double _) => Hide();
        }

        internal static IEnumerable<IHoverTip> GetHoverTips(AbstractModel model)
        {
            if (model is CardModel card)
            {
                if (card.IsUpgradable)
                {
                    var upgraded = card.ToMutable();
                    CardCmd.Upgrade(upgraded);
                    yield return new CardHoverTip(upgraded);
                }
                foreach (var tip in card.HoverTips)
                    yield return tip;
            }
            else
            {
                var tips = model switch
                {
                    RelicModel relic => relic.HoverTips,
                    PotionModel potion => potion.HoverTips,
                    _ => []
                };
                foreach (var tip in tips)
                    yield return tip;
            }
        }

        private static void IgnoreInput(Node node)
        {
            if (node is Control control)
            {
                control.MouseFilter = Control.MouseFilterEnum.Ignore;
                control.FocusMode = Control.FocusModeEnum.None;
            }
            foreach (var child in node.GetChildren())
                IgnoreInput(child);
        }
    }
}
