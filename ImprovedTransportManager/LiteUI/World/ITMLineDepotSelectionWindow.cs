extern alias UUI;

using ColossalFramework.UI;
using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.TransportSystems;
using ImprovedTransportManager.Utility;
using ImprovedTransportManager.Xml;
using Kwytto.LiteUI;
using Kwytto.Localization;
using Kwytto.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Kwytto.LiteUI.KwyttoDialog;

namespace ImprovedTransportManager.UI
{
    public class ITMLineDepotSelectionWindow : GUIOpacityChanging
    {
        protected override bool showOverModals => false;
        protected override bool requireModal => false;
        protected override bool ShowCloseButton => false;
        protected override bool ShowMinimizeButton => true;
        protected override float FontSizeMultiplier => .9f;
        protected bool Resizable => false;
        protected string InitTitle => Str.itm_depotSelectWindow_title;
        protected Vector2 StartSize => new Vector2(400, UIScaler.MaxHeight / 2);
        protected Vector2 StartPosition => new Vector2(999999, UIScaler.MaxHeight / 2);

        public static ITMLineDepotSelectionWindow Instance { get; private set; }

        private string[] m_availableGroups = new[] { Str.itm_vehicleSelectWindow_defaultGroupText };
        private ITMTransportLineXml m_currentLineSettings;
        private HashSet<ushort> m_currentHashSet;
        private List<Tuple<ushort, string, string, string>> m_availableDepots;
        private ushort m_currentLine;
        private Vector2 m_scrollPos;

        private HashSet<ushort> m_clipboard;

        private GUIStyle m_selectionBtnSel;
        private GUIStyle m_selectionBtnUns;
        private GUIStyle m_previewTitle;
        private GUIStyle m_nobrLabel;
        private GUIStyle m_rightLabel;

        protected override void DrawWindow(Vector2 size)
        {
            InitStyles();
            using (new GUILayout.HorizontalScope())
            {
                var selectionGroup = GUIComboBox.Box(m_currentLineSettings.AssetGroup, m_availableGroups, "DepotGroupPicker_line", this, size.x);
                if (selectionGroup != m_currentLineSettings.DepotGroup)
                {
                    m_currentLineSettings.DepotGroup = (byte)selectionGroup;
                    m_currentHashSet = ITMTransportLineSettings.Instance.GetEffectiveDepotsForLine(m_currentLine);
                }
            }
            using (var scroll = new GUILayout.ScrollViewScope(m_scrollPos))
            {
                foreach (var kvp in m_availableDepots)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        var isSelected = m_currentHashSet.Contains(kvp.First);
                        if (GUILayout.Button("", isSelected ? m_selectionBtnSel : m_selectionBtnUns))
                        {
                            ToggleSelection(kvp.First, isSelected);
                        }
                        GUIKwyttoCommons.Space(5);
                        GUILayout.Label($"<b>{kvp.Second}</b>\n<size={DefaultSize * .75f}>{kvp.Third} - {kvp.Fourth}</size>", m_nobrLabel);
                        var lastRect = GUILayoutUtility.GetLastRect();
                        var targetPos = UIScaler.MousePosition + GUIUtility.ScreenToGUIPoint(default);
                        if (Event.current.type == EventType.MouseUp && Input.GetMouseButtonUp(0) && lastRect.Contains(targetPos))
                        {
                            ToggleSelection(kvp.First, isSelected);
                        }
                    }
                }
                m_scrollPos = scroll.scrollPosition;
            }
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(Str.itm_common_copy))
                {
                    m_clipboard = new HashSet<ushort>(m_currentHashSet);
                }
                if (m_clipboard != null && GUILayout.Button(Str.itm_common_paste))
                {
                    m_currentHashSet.Clear();
                    m_clipboard.ForEach(x => m_currentHashSet.Add(x));
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Str.itm_common_clear))
                {
                    m_currentHashSet.Clear();
                }
            }
        }

        private void ShowHelp() => KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
        {
            buttons = new[]
        {
            SpaceBtn,
            new ButtonDefinition
            {
                title = Str.itm_goToMaintenanceCostScreen,
                onClick = ()=> {
                    ITMCitySettingsGUI.Instance.GoToMaintenanceCost();
                    return true;
                }
            },
            new ButtonDefinition
            {
                title = KStr.comm_releaseNotes_Ok,
                onClick = ()=>true
            }
        },
            scrollText = Str.itm_vehicleSelectWindow_helpContent
        });

        private void ToggleSelection(ushort kvp, bool isSelected)
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

        private void InitStyles()
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
                    fixedHeight = 20 * GUIWindow.ResolutionMultiplier,
                    fixedWidth = 20 * GUIWindow.ResolutionMultiplier
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
                    fixedHeight = 20 * GUIWindow.ResolutionMultiplier,
                    fixedWidth = 20 * GUIWindow.ResolutionMultiplier
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
            if (m_nobrLabel is null)
            {
                m_nobrLabel = new GUIStyle(GUI.skin.label)
                {
                    wordWrap = false,
                };
            }
            if (m_rightLabel is null)
            {
                m_rightLabel = new GUIStyle(m_nobrLabel)
                {
                    alignment = TextAnchor.MiddleRight,
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
                var buff = BuildingManager.instance.m_buildings.m_buffer;
                m_availableDepots = ITMDepotUtils.GetAllDepotsFromCity(m_currentLineSettings.CachedTransportType).Select(x =>
                {
                    var pos = buff[x].CalculateSidewalkPosition();
                    return Tuple.New(x, BuildingManager.instance.GetBuildingName(x, default), ModInstance.Controller.ConnectorCD.GetAddressStreetAndNumber(pos, buff[x].m_position, out int number, out string street) ? $"{number:N0} {street}" : "", DistrictManager.instance.GetDistrictName(DistrictManager.instance.GetDistrict(pos)));
                }).OrderBy(x => x.First).ToList();
                m_currentHashSet = settings.GetEffectiveDepotsForLine(m_currentLine);
            }
            else
            {
                m_availableDepots?.Clear();
                m_currentHashSet = null;
            }
        }

        public override void Awake()
        {
            base.Awake();
            Init();
            Instance = this;
            Visible = false;
        }
        private void Init() => Init(InitTitle, new Rect(StartPosition, StartSize), Resizable, true);


    }
}
