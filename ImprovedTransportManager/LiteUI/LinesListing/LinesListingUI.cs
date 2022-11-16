using ColossalFramework;
using ColossalFramework.UI;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.ModShared;
using ImprovedTransportManager.TransportSystems;
using ImprovedTransportManager.Utility;
using Kwytto.LiteUI;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehicleSkins.Localization;

namespace ImprovedTransportManager.UI
{
    internal class LinesListingUI : GUIOpacityChanging
    {
        protected override bool showOverModals => false;
        protected override bool requireModal => false;
        protected override float FontSizeMultiplier => .9f;
        public static LinesListingUI Instance { get; private set; }
        public override void Awake()
        {
            base.Awake();
            Instance = this;
            Init(Str.itm_linesListingWindow_tilte, new Rect(128, 128, 680, 420), resizable: true, minSize: new Vector2(440, 260));
            m_eyeSlashIcon.SetPixels(m_eyeSlashIcon.GetPixels().Select(x => new Color(x.r * .75f, 0, 0, x.a)).ToArray());
            m_eyeSlashIcon.Apply();
            m_picker = GameObjectUtils.CreateElement<GUIColorPicker>(transform).Init();
            m_picker.Visible = false;
            Visible = false;
        }

        private uint m_lastUsedCount = 0;
        private readonly Dictionary<InstanceID, LineData> m_lines = new Dictionary<InstanceID, LineData>();
        private SortOrder m_currentOrder = SortOrder.Id;
        private TransportSystemType m_currentTab = TransportSystemType.BUS;
        private TransportSystemType[] m_availableTypes;
        private string[] m_availableTypesNames;
        private bool m_invertOrder = false;
        private Vector2 m_scrollLines;
        private GUIColorPicker m_picker;
        private InstanceID m_currentLineColorPicker;
        private string[] m_lineActivityOptionsNames;

        private GUIStyle m_LineBasicLabelStyle;
        private GUIStyle m_HeaderLineStyle;
        private GUIStyle m_LineBasicTextStyle;
        private GUIStyle m_redButton;
        private readonly Texture2D m_iconGoToLine = KResourceLoader.LoadTextureKwytto(Kwytto.UI.CommonsSpriteNames.K45_Right);
        private readonly Texture2D m_deleteIcon = KResourceLoader.LoadTextureKwytto(Kwytto.UI.CommonsSpriteNames.K45_Delete);
        private readonly Texture2D m_eyeIcon = KResourceLoader.LoadTextureKwytto(Kwytto.UI.CommonsSpriteNames.K45_Eye);
        private readonly Texture2D m_eyeSlashIcon = KResourceLoader.LoadTextureKwytto(Kwytto.UI.CommonsSpriteNames.K45_EyeSlash);
        private readonly Texture2D m_settings = KResourceLoader.LoadTextureKwytto(Kwytto.UI.CommonsSpriteNames.K45_Settings);

        private static readonly string[] m_optionsContext =
        {
            Str.itm_ctxMenuLineList_autonameVisibleLines,
            Str.itm_ctxMenuLineList_autonameAllCityLines
        };

        protected override void DrawWindow(Vector2 size)
        {
            InitStyles();
            if (m_lastUsedCount != TransportManager.instance.m_lines.ItemCount())
            {
                m_lines.ForEach(x => x.Value.Dispose());
                m_lines.Clear();
                var buff = Singleton<TransportManager>.instance.m_lines.m_buffer;
                for (ushort lineID = 1; lineID < TransportManager.instance.m_lines.m_buffer.Length; lineID++)
                {
                    if ((buff[lineID].m_flags & (TransportLine.Flags.Created | TransportLine.Flags.Temporary)) == TransportLine.Flags.Created)
                    {
                        m_lines[new InstanceID { TransportLine = lineID }] = LineData.FromLine(lineID);
                    }
                }

                m_availableTypes = m_lines.GroupBy(x => x.Value.m_type).Select(x => x.Key).OrderBy(x => (int)x).ToArray();
                m_availableTypesNames = m_availableTypes.Select(x => x.GetTransportName()).ToArray();
                if (m_availableTypes.Length > 0 && !m_availableTypes.Contains(m_currentTab))
                {
                    m_currentTab = m_availableTypes[0];
                }
                m_lastUsedCount = TransportManager.instance.m_lines.ItemCount();
            }
            if (m_availableTypes.Length == 0)
            {
                GUILayout.Label(Str.itm_linesListingWindow_thereAreNoLinesInCity);
            }
            var targetCount = Mathf.FloorToInt(Mathf.Min(m_availableTypes.Length, size.x / 200 ));
            var curIdx = Array.IndexOf(m_availableTypes, m_currentTab);
            var sel = GUILayout.SelectionGrid(curIdx, m_availableTypesNames, targetCount, new GUIStyle(GUI.skin.button) { fontSize = Mathf.FloorToInt(EffectiveFontSizeMultiplier * 14) });
            if (sel >= 0 && curIdx != sel)
            {
                m_currentTab = m_availableTypes[sel];
            }
            var currentView = GetCurrentView();
            var lineNameSize = size.x - 440 ;
            var anyVisible = currentView.Any(x => x.IsVisible());
            using (new GUILayout.HorizontalScope(GUILayout.Height(20 )))
            {
                using (new GUILayout.VerticalScope(GUILayout.Width(20 )))
                {
                    GUILayout.FlexibleSpace();
                    GUIKwyttoCommons.SquareTextureButton2(anyVisible ? m_eyeIcon : m_eyeSlashIcon, "", () => currentView.ForEach(line => line.ChangeLineVisibility(!anyVisible)), size: 20, style: m_HeaderLineStyle);
                    GUILayout.FlexibleSpace();
                }
                HeaderButton("ID", 40 , SortOrder.Id);
                HeaderButton(Str.itm_linesListingWindow_nameColumnTitle, lineNameSize + 2, SortOrder.Name);
                HeaderButton(Str.itm_linesListingWindow_stopsColumnTitle, 40 , SortOrder.Stops);
                HeaderButton(Str.itm_linesListingWindow_budgetColumnTitle, 40 , SortOrder.Budget);
                HeaderButton(Str.itm_linesListingWindow_vehiclesColumnTitle, 40 , SortOrder.Vehicles);
                HeaderButton(Str.itm_linesListingWindow_passengersColumnTitle, 40 , SortOrder.Passengers);
                HeaderButton(Str.itm_linesListingWindow_balanceColumnTitle, 80 , SortOrder.Balance);
                HeaderButton(Str.itm_linesListingWindow_activityColumnTitle, 80 , SortOrder.Acitivty);
                GUIKwyttoCommons.Space(40 );
                var rect = GUILayoutUtility.GetLastRect();
                switch (GUIComboBox.ContextMenuRect(new Rect(rect.position, new Vector2(rect.width, 20 )), m_optionsContext, "CTX_LINELIST", this, new GUIContent(m_settings), new GUIStyle(GUI.skin.button)
                {
                    contentOffset = default,
                    padding = new RectOffset(),
                }))
                {
                    case 0:
                        currentView.ForEach(x => ITMLineUtils.DoAutoname(x.m_id.TransportLine));                        
                        break;
                    case 1:
                        for (ushort i = 1; i < TransportManager.instance.m_lines.m_size; i++)
                        {
                            ITMLineUtils.DoAutoname(i);
                        }
                        break;
                }
            }
            using (var scroll = new GUILayout.ScrollViewScope(m_scrollLines))
            {
                foreach (var line in currentView)
                {
                    line.GetUpdated();
                    using (new GUILayout.HorizontalScope(GUILayout.Height(22 ), GUILayout.Width(size.x - 20 )))
                    {
                        if (line.IsHovered)
                        {
                            GUIKwyttoCommons.Space(0);
                            var rectBg = GUILayoutUtility.GetLastRect();
                            GUI.DrawTexture(new Rect(rectBg.position, new Vector2(size.x - 20 , 25 )), GUIKwyttoCommons.darkGreenTexture);
                        }
                        GUIKwyttoCommons.SquareTextureButton2(line.IsVisible() ? m_eyeIcon : m_eyeSlashIcon, "", () => line.ChangeLineVisibility(!line.IsVisible()), size: 20, style: m_HeaderLineStyle);
                        GUILayout.Label("", m_LineBasicLabelStyle);
                        var rect = GUILayoutUtility.GetLastRect();
                        GUI.DrawTexture(rect, line.m_uiTextureColor);
                        if (GUI.Button(rect, $"<color=#{line.LineColor.ContrastColor().ToRGB()}>{line.LineIdentifier()}</color>", m_LineBasicLabelStyle))
                        {
                            m_currentLineColorPicker = line.m_id;
                            m_picker.Show("itm_linesWindow_clrPicker", line.LineColor, -1);
                        }
                        else if (m_currentLineColorPicker == line.m_id && m_picker.Visible && m_picker.SelectedColor != line.LineColor)
                        {
                            line.LineColor = m_picker.SelectedColor;
                        }
                        var oldName = line.LineName;
                        if (GUILayout.TextField(oldName, new GUIStyle(GUI.skin.textField)
                        {
                            fixedWidth = lineNameSize,
                            margin = new RectOffset(0, 0, 0, 0),
                            contentOffset = new Vector2(0, 0),
                            padding = new RectOffset(1, 1, 1, 1),
                            stretchHeight = true
                        }) is string str && str != oldName)
                        {
                            line.LineName = oldName;
                        }
                        var hasVehicles = line.m_type.HasVehicles();
                        GUILayout.Label($"{line.m_stopsCount:N0}", m_LineBasicLabelStyle);
                        GUILayout.Label(hasVehicles ? $"{line.BudgetEffectiveNow:N0}%" : Str.itm_common_notApplicableAcronym, m_LineBasicLabelStyle);
                        GUILayout.Label(hasVehicles ? $"{line.m_vehiclesCount:N0}/{line.VehiclesTargetNow:N0}" : Str.itm_common_notApplicableAcronym, m_LineBasicLabelStyle);
                        GUILayout.Label($"{line.m_passengersResCount + line.m_passengersTouCount:N0}", m_LineBasicLabelStyle);
                        GUILayout.Label(hasVehicles ? $"{line.m_lineFinancesBalance.ToGameCurrencyFormat()}" : Str.itm_common_notApplicableAcronym, new GUIStyle(m_HeaderLineStyle) { fixedWidth = 80 });
                        if (GUIComboBox.Button((int)line.LineActivity, m_lineActivityOptionsNames, $"{line.m_id}", this, 80) is int newIdx && newIdx != (int)line.LineActivity)
                        {
                            line.LineActivity = (LineActivityOptions)newIdx;
                        }
                        GUIKwyttoCommons.SquareTextureButton2(m_iconGoToLine, "", () => line.GoTo(), size: 20, style: m_HeaderLineStyle);
                        GUIKwyttoCommons.SquareTextureButton2(m_deleteIcon, "", () => line.Delete(), size: 20, style: m_redButton);
                    }
                    if (Event.current.type == EventType.Repaint)
                    {
                        if (GUILayoutUtility.GetLastRect().Contains(GUIUtility.ScreenToGUIPoint(default) + UIScaler.MousePosition))
                        {
                            line.OnMouseEnter();
                        }
                        else
                        {
                            line.OnMouseLeave();
                        }
                    }
                    GUIKwyttoCommons.Space(4);
                }
                m_scrollLines = scroll.scrollPosition;
            }
            GUILayout.Label(string.Format(Str.itm_linesListingWindow_footerFormat, m_currentTab.GetTransportName(), currentView.Count(), m_lines.Count), m_HeaderLineStyle, GUILayout.Height(22));
        }

        private void HeaderButton(string content, float width, SortOrder relativeSort)
        {
            if (relativeSort == m_currentOrder)
            {
                content = $"<color=yellow>{content}\n{(m_invertOrder ? "\u25b2" : "\u25bc")}</color>";
            }
            if (GUILayout.Button(content, m_HeaderLineStyle, GUILayout.Width(width))) SetSorting(relativeSort);

        }

        private void SetSorting(SortOrder so)
        {
            if (so == m_currentOrder)
            {
                m_invertOrder = !m_invertOrder;
            }
            else
            {
                m_currentOrder = so;
                m_invertOrder = false;
            }
        }

        private IEnumerable<LineData> GetCurrentView()
        {
            Func<LineData, object> orderFn = null;
            switch (m_currentOrder)
            {
                case SortOrder.Id:
                    orderFn = (x) => x.LineInternalSequentialNumber();
                    break;
                case SortOrder.Name:
                    orderFn = (x) => x.LineName;
                    break;
                case SortOrder.Stops:
                    orderFn = (x) => x.m_stopsCount;
                    break;
                case SortOrder.Budget:
                    orderFn = (x) => x.BudgetEffectiveNow;
                    break;
                case SortOrder.Vehicles:
                    orderFn = (x) => x.m_vehiclesCount;
                    break;
                case SortOrder.Passengers:
                    orderFn = (x) => x.m_passengersResCount + x.m_passengersTouCount;
                    break;
                case SortOrder.Balance:
                    orderFn = (x) => x.m_lineFinancesBalance;
                    break;
                case SortOrder.Acitivty:
                    orderFn = (x) => x.LineActivity;
                    break;
            }
            var baseList = m_lines.Values.Where(x => x.m_type == m_currentTab);
            if (m_invertOrder)
            {

                return baseList.OrderByDescending((a) => orderFn(a));
            }
            else
            {
                return baseList.OrderBy((a) => orderFn(a));
            }
        }

        private void InitStyles()
        {
            if (m_LineBasicLabelStyle is null)
            {
                m_LineBasicLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    stretchHeight = true,
                    fixedWidth = 40 ,
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.CeilToInt(14 ),
                    margin = new RectOffset(0, 0, 1, 1),
                    contentOffset = new Vector2(0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                    wordWrap = false,
                };
            }
            if (m_HeaderLineStyle is null)
            {
                m_HeaderLineStyle = new GUIStyle(GUI.skin.label)
                {
                    stretchHeight = true,
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.CeilToInt(14 ),
                    margin = new RectOffset(0, 0, 1, 1),
                    contentOffset = new Vector2(0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                    wordWrap = false,
                };
            }
            if (m_LineBasicTextStyle is null)
            {
                m_LineBasicTextStyle = new GUIStyle(GUI.skin.textField)
                {
                    stretchHeight = true,
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = Mathf.CeilToInt(13 ),
                    margin = new RectOffset(0, 0, 1, 1),
                    contentOffset = new Vector2(0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                };
            }
            if (m_redButton is null)
            {
                m_redButton = new GUIStyle(Skin.button)
                {
                    margin = new RectOffset(0, 0, 1, 1),
                    normal = new GUIStyleState()
                    {
                        background = GUIKwyttoCommons.darkRedTexture,
                        textColor = Color.white
                    },
                    hover = new GUIStyleState()
                    {
                        background = GUIKwyttoCommons.redTexture,
                        textColor = Color.white
                    },
                };
            }
        }

        //public void X()
        //{
        //    if (m_lineID != 0)
        //    {
        //        Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[Singleton<TransportManager>.instance.m_lines.m_buffer[m_lineID].m_stops].m_position;
        //        InstanceID iid = InstanceID.Empty;
        //        iid.TransportLine = m_lineID;
        //        WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(position, iid);
        //    }
        //    else
        //    {
        //        Vector3 position = default;
        //        InstanceID iid = InstanceID.Empty;
        //        iid.Set(ITMInstanceType.TransportSystemDefinition, ITMPanel.Instance.m_linesPanel.TSD.Id);
        //        WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(position, iid);
        //    }

        //}


        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();
            m_lineActivityOptionsNames = Enum.GetValues(typeof(LineActivityOptions)).Cast<LineActivityOptions>().Select(x => x.ValueToI18n("SHORT")).ToArray();
        }
        protected override void OnWindowClosed()
        {
            base.OnWindowClosed();
            m_lastUsedCount = 0;
        }

        protected override void OnWindowDestroyed()
        {
            Instance = null;
        }

        private enum SortOrder
        {
            Id,
            Name,
            Stops,
            Budget,
            Vehicles,
            Passengers,
            Balance,
            Acitivty
        }
    }
}
