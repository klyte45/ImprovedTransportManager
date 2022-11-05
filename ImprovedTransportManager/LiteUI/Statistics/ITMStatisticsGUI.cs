using ImprovedTransportManager.Localization;
using ImprovedTransportManager.TransportSystems;
using ImprovedTransportManager.Utility;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    internal class ITMStatisticsGUI : GUIOpacityChanging
    {
        public static ITMStatisticsGUI Instance { get; private set; }
        protected override float FontSizeMultiplier => .9f;

        private int m_currentLineIdxSelected = -1;
        private int m_currentStopIdxSelected = -1;
        private int m_currentVehicleIdxSelected = -1;

        private readonly Dictionary<string, ushort> m_linesDic = new Dictionary<string, ushort>();
        private string[] m_linesOptions = new string[0];

        private readonly Dictionary<string, ushort> m_stopsDic = new Dictionary<string, ushort>();
        private string[] m_stopsOptions = new string[0];

        private readonly Dictionary<string, ushort> m_vehiclesDic = new Dictionary<string, ushort>();
        private string[] m_vehiclesOptions = new string[0];

        private Texture2D m_reloadTex;
        private Texture2D m_deleteTex;
        private GUIStyle m_centeredLabelTitle;
        private GUIStyle m_centeredLabelSubtitle;

        internal ushort GetCurrentSelectedLine() => m_currentLineIdxSelected < 0 ? default : m_linesDic[m_linesOptions[m_currentLineIdxSelected]];
        internal ushort GetCurrentSelectedStop() => m_currentStopIdxSelected < 0 ? default : m_stopsDic[m_stopsOptions[m_currentStopIdxSelected]];
        internal ushort GetCurrentSelectedVehicle() => m_currentVehicleIdxSelected < 0 ? default : m_vehiclesDic[m_vehiclesOptions[m_currentVehicleIdxSelected]];
        public override void Awake()
        {
            base.Awake();
            Instance = this;
            Init($"{ModInstance.Instance.GeneralName} - {Str.itm_statistics_title}", new Rect(128, 128, 680, 420), resizable: true, minSize: new Vector2(440, 260));
            var tabs = new IGUIVerticalITab[] {
                        new FinanceReportTab(GetCurrentSelectedLine,GetCurrentSelectedStop,GetCurrentSelectedVehicle),
                        new StudentTouristsReportTab(GetCurrentSelectedLine,GetCurrentSelectedStop,GetCurrentSelectedVehicle),
                        new PassengerAgeReportTab(GetCurrentSelectedLine,GetCurrentSelectedStop,GetCurrentSelectedVehicle),
                        new PassengerWealthReportTab(GetCurrentSelectedLine,GetCurrentSelectedStop,GetCurrentSelectedVehicle),
                        new PassengerGenderReportTab(GetCurrentSelectedLine,GetCurrentSelectedStop,GetCurrentSelectedVehicle)
                    };
            m_tabsContainer = new GUIVerticalTabsContainer(tabs);
            Visible = false;
            m_reloadTex = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Reload);
            m_deleteTex = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Delete);
        }
        protected override bool showOverModals => false;

        protected override bool requireModal => false;

        private GUIVerticalTabsContainer m_tabsContainer;


        protected override void DrawWindow(Vector2 size)
        {
            InitStyles();
            var targetFilterWidth = size.x / 3 - 30;
            using (new GUILayout.HorizontalScope())
            {
                var newSel0 = GUIComboBox.Box(m_currentLineIdxSelected, m_linesOptions, "LINEFILTER_001", this, nullStr: Str.itm_statistics_nullSelLine, maxWidth: targetFilterWidth);
                if (newSel0 != m_currentLineIdxSelected)
                {
                    ClearFilters();
                    SelectLine(newSel0);
                }
                GUIKwyttoCommons.SquareTextureButton(m_reloadTex, Str.itm_statistics_reloadLines, ReloadLines, size: 20);
                GUILayout.FlexibleSpace();
                var newSel = GUIComboBox.Box(m_currentStopIdxSelected, m_currentVehicleIdxSelected < 0 ? m_stopsOptions : new string[0], "STOPFILTER_001", this, nullStr: Str.itm_statistics_nullSelStop, maxWidth: targetFilterWidth);
                if (newSel != m_currentStopIdxSelected)
                {
                    m_currentVehicleIdxSelected = -1;
                    m_currentStopIdxSelected = newSel;
                    var currentTab = m_tabsContainer.CurrentTabIdx;
                    m_tabsContainer.Reset();
                    m_tabsContainer.CurrentTabIdx = currentTab;
                }
                var newSel2 = GUIComboBox.Box(m_currentVehicleIdxSelected, m_currentStopIdxSelected < 0 ? m_vehiclesOptions : new string[0], "VEHFILTER_001", this, nullStr: Str.itm_statistics_nullVehicle, maxWidth: targetFilterWidth);
                if (newSel2 != m_currentVehicleIdxSelected)
                {
                    m_currentStopIdxSelected = -1;
                    m_currentVehicleIdxSelected = newSel2;
                    var currentTab = m_tabsContainer.CurrentTabIdx;
                    m_tabsContainer.Reset();
                    m_tabsContainer.CurrentTabIdx = currentTab;
                }
                GUIKwyttoCommons.SquareTextureButton(m_deleteTex, Str.itm_statistics_clearFilters, ClearFilters, size: 20);
            }
            if (m_currentLineIdxSelected >= 0)
            {
                GUILayout.Label(m_linesOptions[m_currentLineIdxSelected], m_centeredLabelTitle);
                if (m_currentStopIdxSelected >= 0)
                {
                    GUILayout.Label(string.Format(Str.itm_statistics_subtitleTableFormatStops, m_stopsOptions[m_currentStopIdxSelected]), m_centeredLabelSubtitle);
                }
                else if (m_currentVehicleIdxSelected >= 0)
                {
                    GUILayout.Label(string.Format(Str.itm_statistics_subtitleTableFormatVehicles, m_vehiclesOptions[m_currentVehicleIdxSelected]), m_centeredLabelSubtitle);
                }
                else
                {
                    GUILayout.Label(Str.itm_statistics_subtitleTableFormatAll, m_centeredLabelSubtitle);
                }
                GUILayout.Space(0);
                var rect = GUILayoutUtility.GetLastRect();
                if (rect.position == default)
                {
                    rect = cachedRect;
                }
                else
                {
                    cachedRect = rect;
                }
                rect.x = 0;
                m_tabsContainer.DrawListTabs(new Rect(rect.position, size - rect.position), 150);
            }
        }

        Rect cachedRect;

        private void InitStyles()
        {
            if (m_centeredLabelTitle is null)
            {
                m_centeredLabelTitle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.RoundToInt(DefaultSize * 1.4f),
                    fontStyle = FontStyle.Bold
                };
            }
            if (m_centeredLabelSubtitle is null)
            {
                m_centeredLabelSubtitle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.RoundToInt(DefaultSize * 1.1f)
                };
            }
        }

        private void ClearFilters()
        {
            m_currentStopIdxSelected = -1;
            m_currentVehicleIdxSelected = -1;
            var currentTab = m_tabsContainer.CurrentTabIdx;
            m_tabsContainer.Reset();
            m_tabsContainer.CurrentTabIdx = currentTab;
        }

        private void ReloadLines()
        {
            var currentSelectedLine = GetCurrentSelectedLine();
            var currentSelectedStop = GetCurrentSelectedStop();
            var currentSelectedVehicle = GetCurrentSelectedVehicle();
            SelectLine(-1);

            m_linesDic.Clear();
            m_linesDic.AddRange(TransportManager.instance.m_lines.m_buffer
                .Select((x, i) => Tuple.NewRef(ref i, ref x))
                .Where(x => x.First != 0 && (x.Second.m_flags & TransportLine.Flags.Created) != 0)
                .Select(x => Tuple.New($"{TransportSystemTypeExtensions.FromLineId((ushort)x.First, false).GetTransportName()} {x.Second.GetEffectiveIdentifier((ushort)x.First)} - {TransportManager.instance.GetLineName((ushort)x.First)}", (ushort)x.First))
                .GroupBy(x => x.First)
                .SelectMany((w) => w.Select((x, i) => i != 0 ? Tuple.New(x.First + $" ({i + 1})", x.Second) : x))
                .OrderBy(x => x.First)
                .ToDictionary(x => x.First, x => x.Second));
            m_linesOptions = m_linesDic.Keys.ToArray();

            if (m_linesDic.ContainsValue(currentSelectedLine))
            {
                var targetKey = m_linesDic.Where(x => x.Value == currentSelectedLine).First();
                SelectLine(Array.IndexOf(m_linesOptions, targetKey.Key));
                if (m_currentLineIdxSelected > 0)
                {
                    if (currentSelectedStop > 0 && m_stopsDic.ContainsValue(currentSelectedStop))
                    {
                        var targetKeyStop = m_stopsDic.Where(x => x.Value == currentSelectedStop).First();
                        m_currentStopIdxSelected = Array.IndexOf(m_stopsOptions, targetKeyStop.Key);
                    }
                    else if (currentSelectedVehicle > 0 && m_vehiclesDic.ContainsValue(currentSelectedVehicle))
                    {
                        var targetKeyVehicles = m_vehiclesDic.Where(x => x.Value == currentSelectedVehicle).First();
                        m_currentVehicleIdxSelected = Array.IndexOf(m_vehiclesOptions, targetKeyVehicles.Key);
                    }
                }
            }
        }

        private void SelectLine(int idx)
        {
            if (idx < 0 || idx >= m_linesOptions.Length)
            {
                m_currentLineIdxSelected = -1;
                m_stopsDic.Clear();
                m_vehiclesDic.Clear();
                m_vehiclesOptions = new string[0];
                m_stopsOptions = new string[0];
                ClearFilters();
                return;
            }
            m_currentLineIdxSelected = idx;
            ushort targetLineId = m_linesDic[m_linesOptions[m_currentLineIdxSelected]];
            m_stopsDic.Clear();
            ITMLineUtils.DoWithEachStop(targetLineId, (id, idxStop) =>
            {
                var stopName = $"{idxStop}: {ITMLineUtils.GetEffectiveStopName(id)}";

                m_stopsDic[stopName] = id;
            });
            m_stopsOptions = m_stopsDic.Keys.ToArray();

            var buffV = VehicleManager.instance.m_vehicles.m_buffer;
            m_vehiclesDic.Clear();
            ITMLineUtils.DoWithEachVehicle(targetLineId, (id, idxVeh) =>
            {
                var vehicleName = $"{idxVeh}: {ITMLineUtils.GetEffectiveVehicleName(id)} ({buffV[id].Info.GetUncheckedLocalizedTitle()})";
                m_vehiclesDic[vehicleName] = id;
            });
            m_vehiclesOptions = m_vehiclesDic.Keys.ToArray();


        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();
        }

        protected override void OnWindowDestroyed()
        {
            Instance = null;
        }
    }
}
