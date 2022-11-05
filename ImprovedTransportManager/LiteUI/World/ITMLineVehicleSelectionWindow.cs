extern alias UUI;
using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.TransportSystems;
using ImprovedTransportManager.Xml;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    public class ITMLineVehicleSelectionWindow : GUIOpacityChanging
    {
        protected override bool showOverModals => false;
        protected override bool requireModal => false;
        protected override bool ShowCloseButton => false;
        protected override bool ShowMinimizeButton => true;
        protected override float FontSizeMultiplier => .9f;
        protected bool Resizable => false;
        protected string InitTitle => Str.itm_vehicleSelectWindow_title;
        protected Vector2 StartSize => new Vector2(400, 600);
        protected Vector2 StartPosition => new Vector2(999999, 0);

        public static ITMLineVehicleSelectionWindow Instance { get; private set; }

        private string[] m_availableGroups = new[] { Str.itm_vehicleSelectWindow_defaultGroupText };
        private ITMTransportLineXml m_currentLineSettings;
        private HashSet<VehicleInfo> m_currentHashSet;
        private List<KeyValuePair<string, VehicleInfo>> m_availableModels;
        private ushort m_currentLine;
        private Vector2 m_scrollPos;

        private GUIStyle m_selectionBtnSel;
        private GUIStyle m_selectionBtnUns;
        private GUIStyle m_previewTitle;

        private VehicleInfo m_currentPreview;
        private string m_currentPreviewTitle;
        private AVOPreviewRenderer m_previewRenderer;
        private readonly Vector3 m_previewSize = new Vector3(300, 200);

        public ITMLineVehicleSelectionWindow() : base()
        {
            DrawOverWindow = () =>
            {
                if (m_currentPreview != null)
                {
                    var defaultPos = GUIUtility.ScreenToGUIPoint(default);
                    var localPosMouse = UIScaler.MousePosition + defaultPos;
                    var pos = Vector2.Max(localPosMouse - new Vector2(-10, 200), new Vector2(0, 0));
                    var rect = new Rect(pos, m_previewSize);
                    GUI.DrawTexture(rect, GUIKwyttoCommons.almostWhiteTexture);
                    GUI.DrawTexture(rect, m_previewRenderer.Texture);
                    GUI.Label(rect, m_currentPreviewTitle, m_previewTitle);
                }
            };
        }
        protected override void DrawWindow(Vector2 size)
        {
            InitSkins();
            using (new GUILayout.HorizontalScope())
            {
                var selectionGroup = GUIComboBox.Box(m_currentLineSettings.AssetGroup, m_availableGroups, "AssetGroupPicker_line", this, size.x);
                if (selectionGroup != m_currentLineSettings.AssetGroup)
                {
                    m_currentLineSettings.AssetGroup = (byte)selectionGroup;
                    m_currentHashSet = ITMTransportLineSettings.Instance.GetEffectiveAssetsForLine(m_currentLine);
                }
            }
            using (var scroll = new GUILayout.ScrollViewScope(m_scrollPos))
            {
                if (Event.current.type == EventType.Repaint)
                {
                    m_currentPreview = null;
                }
                foreach (var kvp in m_availableModels)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        var isSelected = m_currentHashSet.Contains(kvp.Value);
                        if (GUILayout.Button("", isSelected ? m_selectionBtnSel : m_selectionBtnUns))
                        {
                            ToggleSelection(kvp.Value, isSelected);
                        }
                        GUILayout.Space(5);
                        GUILayout.Label(kvp.Key);
                        var lastRect = GUILayoutUtility.GetLastRect();
                        var targetPos = UIScaler.MousePosition + GUIUtility.ScreenToGUIPoint(default);
                        if (Event.current.type == EventType.MouseUp && Input.GetMouseButtonUp(0) && lastRect.Contains(targetPos))
                        {
                            ToggleSelection(kvp.Value, isSelected);
                        }
                        if (Event.current.type == EventType.Repaint && m_currentPreview is null && lastRect.Contains(targetPos))
                        {
                            m_currentPreviewTitle = kvp.Key;
                            m_currentPreview = kvp.Value;
                        }
                    }
                }
                m_scrollPos = scroll.scrollPosition;
            }
        }

        private void ToggleSelection(VehicleInfo kvp, bool isSelected)
        {
            if (isSelected)
            {
                m_currentHashSet.Remove(kvp);
            }
            else
            {
                m_currentHashSet.Add(kvp);
            }
        }

        private void InitSkins()
        {
            if (m_selectionBtnSel is null)
            {
                m_selectionBtnSel = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        background = GUIKwyttoCommons.greenTexture
                    },
                    hover =
                    {
                        background = GUIKwyttoCommons.yellowTexture
                    },
                    fixedHeight = 20,
                    fixedWidth = 20
                };
            }
            if (m_selectionBtnUns is null)
            {
                m_selectionBtnUns = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        background = GUIKwyttoCommons.blackTexture
                    },
                    hover =
                    {
                        background = GUIKwyttoCommons.whiteTexture
                    },
                    fixedHeight = 20,
                    fixedWidth = 20
                };
            }
            if (m_previewTitle is null)
            {
                m_previewTitle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.UpperCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = Mathf.RoundToInt(DefaultSize * 1.2f),
                    normal =
                    {
                        textColor = Color.black
                    }
                };
            }
        }

        internal void OnIdChanged(InstanceID currentId)
        {
            if (currentId.TransportLine > 0)
            {
                m_currentLine = currentId.TransportLine;
                var settings = ITMTransportLineSettings.Instance;
                m_currentLineSettings = settings.SafeGetLine(m_currentLine);
                if (!m_currentLineSettings.CachedTransportType.HasVehicles())
                {
                    Visible = false;
                    return;
                }
                m_availableModels = settings.GetAllBasicAssetsForLine(m_currentLine).OrderBy(x => x.Key).ToList();
                m_currentHashSet = settings.GetEffectiveAssetsForLine(m_currentLine);
            }
            else
            {
                m_availableModels?.Clear();
                m_currentHashSet = null;
            }
        }

        public override void Awake()
        {
            base.Awake();
            Init();
            Instance = this;
            GameObjectUtils.CreateElement(out m_previewRenderer, transform);
            m_previewRenderer.Size = m_previewSize;
            m_previewRenderer.Zoom = 3;
            m_previewRenderer.CameraRotation = 40;
            Visible = false;
        }
        private void Init() => Init(InitTitle, new Rect(StartPosition, StartSize), Resizable, true);

        private void Update()
        {
            if (Visible && !(m_currentPreview is null))
            {
                m_previewRenderer.CameraRotation -= 1;
                m_previewRenderer.RenderVehicle(m_currentPreview, Color.HSVToRGB(Math.Abs(m_previewRenderer.CameraRotation) / 360f, .5f, .5f), true);
            }
        }
    }
}
