using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.TransportSystems;
using Kwytto.LiteUI;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehicleSkins.Localization;

namespace ImprovedTransportManager.UI
{
    public partial class ITMLineStopsWindow : ITMBaseWipDependentWindow<ITMLineStopsWindow, PublicTransportWorldInfoPanel>
    {
        protected override bool showOverModals => false;
        protected override bool requireModal => false;
        protected override bool ShowCloseButton => false;
        protected override bool ShowMinimizeButton => true;
        protected override float FontSizeMultiplier => .9f;
        protected override bool Resizable => true;
        protected override string InitTitle => Str.itm_lineMap_title;
        protected override Vector2 StartSize => new Vector2(700, 300);
        protected override Vector2 StartPosition => default;
        protected override Vector2 MinSize => new Vector2(700, 300);
        protected override Vector2 MaxSize => new Vector2(700, 999999);
        protected override Tuple<UIComponent, PublicTransportWorldInfoPanel>[] ComponentsWatching => ModInstance.Controller.PTPanels;


        private const int STATION_SIZE = 120;
        private GUIColorPicker picker;
        private Texture2D m_baseStation;
        private Texture2D m_baseStationFree;
        private Texture2D m_baseStationHigh;
        private Texture2D m_baseLineBg;
        public Texture2D TexStation { get; private set; }
        public Texture2D TexStationFree { get; private set; }
        public Texture2D TexStationHigh { get; private set; }
        public Texture2D TexLineBg { get; private set; }
        private Color m_currentLoadedColor;
        private readonly List<StationData> m_loadedStopData = new List<StationData>();
        private readonly DisposableFastList<VehicleData> m_loadedVehiclesData = new DisposableFastList<VehicleData>();
        private VehicleShowDataType m_currentVehicleDataShow = VehicleShowDataType.PassengerCapacity;
        private readonly string[] m_vehicleShowOptions = Enum.GetValues(typeof(VehicleShowDataType)).Cast<VehicleShowDataType>().Select(x => x.ValueToI18n()).ToArray();
        private Vector2 m_mapScroll;

        private GUIStyle m_smallLabel;
        private GUIStyle m_stationBtn;
        private GUIStyle m_centerLabel;
        private ushort m_currentLine;
        private ushort m_loadedVehiclesLine;
        private LineData m_currentLineData;
        private uint m_vehicleRecalcFrame;
        private GUIStyle m_noBreakLabel;
        private GUIStyle m_lineIconText;

        public override void OnAwake()
        {
            m_baseLineBg = KResourceLoader.LoadTextureMod("map_lineBase");
            m_baseStation = KResourceLoader.LoadTextureMod("map_station");
            m_baseStationFree = KResourceLoader.LoadTextureMod("map_stationFree");
            m_baseStationHigh = KResourceLoader.LoadTextureMod("map_stationHigh");
            TexLineBg = TextureUtils.New(m_baseLineBg.width, m_baseLineBg.height);
            TexStation = TextureUtils.New(m_baseStation.width, m_baseStation.height);
            TexStationFree = TextureUtils.New(m_baseStationFree.width, m_baseStationFree.height);
            TexStationHigh = TextureUtils.New(m_baseStationHigh.width, m_baseStationHigh.height);
            picker = GameObjectUtils.CreateElement<GUIColorPicker>(transform).Init();
            picker.Visible = false;
            Minimized = true;
        }

        private GUIStyle m_redButton;
        public GUIStyle RedButton
        {
            get
            {
                if (m_redButton is null)
                {
                    m_redButton = new GUIStyle(Skin.button)
                    {
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
                return m_redButton;
            }
        }


        protected override void DrawWindow(Vector2 size)
        {
            InitStyles();
            if (m_currentLine != 0)
            {
                m_currentLineData.GetUpdated();
                using (new GUILayout.HorizontalScope())
                {
                    if (m_currentLineData.m_type.HasVehicles())
                    {
                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.Label(Str.itm_lineMap_vehicleDataToShow, m_centerLabel);
                            using (new GUILayout.HorizontalScope())
                            {
                                m_currentVehicleDataShow = (VehicleShowDataType)GUIComboBox.Box((int)m_currentVehicleDataShow, m_vehicleShowOptions, "itmLineStopsVehicleDataShow", this, 200);
                            }
                            GUILayout.FlexibleSpace();
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"{Str.itm_lineMap_earningsLastPeriodAcronymLegend}\n{Str.itm_lineMap_earningsCurrentPeriodAcronymLegend}\n{Str.itm_lineMap_earningsAllTimeAcronymLegend}", m_smallLabel);
                }

                var headerRect = GUILayoutUtility.GetLastRect();
                var iconRect = new Rect((headerRect.size.x - headerRect.height) / 2, headerRect.position.y, headerRect.height, headerRect.height);
                if (m_currentLineData.LineIcon is Texture2D tex)
                {
                    if (GUI.Button(iconRect, tex, m_stationBtn))
                    {
                        //CALL CD if available
                    }
                }
                else
                {
                    var contrastColor = m_currentLineData.LineColor.ContrastColor();
                    GUI.DrawTexture(iconRect, GUIKwyttoCommons.whiteTexture);

                    var subRect = new Rect(iconRect.position + new Vector2(2, 2), iconRect.size - new Vector2(4, 4));

                    GUI.DrawTexture(subRect, m_currentLineData.m_uiTextureColor);
                    var identifier = m_currentLineData.LineIdentifier();
                    if (GUI.Button(subRect, identifier, new GUIStyle(m_lineIconText)
                    {
                        fontSize = Mathf.RoundToInt(16 * EffectiveFontSizeMultiplier * (.5f + (3f / identifier.Length))),
                        normal =
                        {
                            textColor = contrastColor
                        }
                    }))
                    {
                        //CALL CD if available
                    }
                }

                using (var scroll = new GUILayout.ScrollViewScope(m_mapScroll))
                {
                    var leftPivotLine = (size.x - TexLineBg.width) * .5f;
                    var lineLengthPixels = (m_loadedStopData.Count + .125f) * STATION_SIZE;
                    GUILayout.Space(lineLengthPixels);
                    GUI.DrawTexture(new Rect(leftPivotLine, 0, TexLineBg.width, lineLengthPixels), TexLineBg, ScaleMode.StretchToFill);
                    for (int i = 0; i < m_loadedStopData.Count; i++)
                    {
                        StationData stop = m_loadedStopData[i];
                        stop.GetUpdated();
                        var targetTex = GetStationImage(stop);
                        var labelWidth = size.x - leftPivotLine - (targetTex.width + 6);
                        var stationPosMapY = ((i + .25f) * STATION_SIZE) - (targetTex.height * .5f);
                        if (GUI.Button(new Rect(leftPivotLine, stationPosMapY, targetTex.width, targetTex.height), targetTex, m_stationBtn))
                        {
                            ToolsModifierControl.cameraController.SetTarget(new InstanceID { NetNode = stop.stopId }, stop.position, false);
                        }
                        var textsBasePosition = new Vector2(targetTex.width + leftPivotLine + 6, stationPosMapY);
                        var boredPercent = 1 - (stop.timeUntilBored * (1f / 255));
                        GUI.Label(new Rect(textsBasePosition, new Vector2(labelWidth, 20)), $"<b>{stop.cachedName}</b>");
                        GUI.Label(new Rect(textsBasePosition + new Vector2(0, 17), new Vector2(labelWidth, 20)), $"{Str.itm_lineMap_earningsCurrentPeriodAcronym} {stop.EarningCurrentWeek.ToString(Settings.moneyFormat, LocaleManager.cultureInfo)}; {Str.itm_lineMap_earningsLastPeriodAcronym} {stop.EarningLastWeek.ToString(Settings.moneyFormat, LocaleManager.cultureInfo)}; {Str.itm_lineMap_earningsAllTimeAcronym} {stop.EarningAllTime.ToString(Settings.moneyFormatNoCents, LocaleManager.cultureInfo)}", m_smallLabel);
                        GUI.Label(new Rect(textsBasePosition + new Vector2(0, 34), new Vector2(labelWidth, 20)), string.Format(Str.itm_lineMap_waitingTemplate, stop.residentsWaiting, stop.touristsWaiting, boredPercent * 100, Color.Lerp(Color.white, Color.Lerp(Color.yellow, Color.red, (boredPercent * 2) - 1), boredPercent * 2).ToRGB()), m_smallLabel);
                        GUI.Label(new Rect(new Vector2(textsBasePosition.x, stationPosMapY + (STATION_SIZE * .66f)), new Vector2(labelWidth, 20)), $"<i><color=cyan>{stop.distanceNextStop:N0}m</color></i>");
                    }
                    if (m_currentLineData.m_type.HasVehicles())
                    {
                        foreach (var vehicle in m_loadedVehiclesData)
                        {
                            var position = vehicle.GetPositionOffset(leftPivotLine - 4, STATION_SIZE);
                            var content = vehicle.GetContentFor(m_currentVehicleDataShow);
                            if (GUI.Button(new Rect(position, new Vector2(leftPivotLine * .25f, 20)), content, vehicle.CachedStyle))
                            {
                                ToolsModifierControl.cameraController.SetTarget(new InstanceID { Vehicle = vehicle.VehicleId }, default, false);
                            }
                        }
                    }

                    m_mapScroll = scroll.scrollPosition;
                }
            }
        }

        public bool HasAnyFreeStop() => m_loadedStopData.Any(x => x.tariffMultiplier == 0);

        private void InitStyles()
        {
            if (m_noBreakLabel is null)
            {
                m_noBreakLabel = new GUIStyle(GUI.skin.label)
                {
                    wordWrap = false,
                    alignment = TextAnchor.MiddleLeft,
                };
            }

            if (m_smallLabel is null)
            {
                m_smallLabel = new GUIStyle(GUI.skin.label)
                {
                    fontSize = Mathf.CeilToInt(GUI.skin.label.fontSize * .75f),
                };
            }
            if (m_centerLabel is null)
            {
                m_centerLabel = new GUIStyle(GUI.skin.label)
                {
                    margin = new RectOffset(0, 0, 1, 1),
                    contentOffset = new Vector2(0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                    alignment = TextAnchor.MiddleCenter,
                };
            }
            if (m_stationBtn is null)
            {
                m_stationBtn = new GUIStyle(GUI.skin.label)
                {
                    margin = new RectOffset(0, 0, 1, 1),
                    contentOffset = new Vector2(0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                    hover = GUI.skin.button.hover,
                };
            }
            if (m_lineIconText is null)
            {
                m_lineIconText = new GUIStyle(GUI.skin.label)
                {
                    margin = new RectOffset(0, 0, -4, -4),
                    contentOffset = new Vector2(0, -4),
                    padding = new RectOffset(0, 0, -4, -4),
                    wordWrap = true,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                    //hover = GUI.skin.button.hover, //Only if CD exists
                };
            }
        }
        private Texture2D GetStationImage(StationData s)
            => s.tariffMultiplier > 1.001f
                ? TexStationHigh
                : s.tariffMultiplier < 0.999f
                    ? TexStationFree
                    : TexStation;

        private void UpdateVehicleButtons(ushort lineID, bool force = false)
        {
            if (m_loadedStopData.Count == 0 || (m_loadedVehiclesLine == lineID && !force && m_vehicleRecalcFrame + 23 > SimulationManager.instance.m_referenceFrameIndex))
            {
                return;
            }
            m_loadedVehiclesLine = lineID;
            m_vehicleRecalcFrame = SimulationManager.instance.m_referenceFrameIndex;

            var bufferV = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
            ushort vehicleId = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_vehicles;
            var idx = 0;
            var cachedStopOrder = m_loadedStopData.Select(x => x.stopId).ToArray();
            while (vehicleId != 0)
            {
                VehicleData currentVehicle;
                if (idx >= m_loadedVehiclesData.m_size)
                {
                    currentVehicle = new VehicleData();
                    m_loadedVehiclesData.Add(currentVehicle);
                }
                else
                {
                    currentVehicle = m_loadedVehiclesData[idx];
                }
                currentVehicle.VehicleId = vehicleId;
                ref Vehicle vehicleData = ref bufferV[vehicleId];
                var nextStop = currentVehicle.m_nextStop = vehicleData.m_targetBuilding;
                var nextStationIdx = currentVehicle.m_nextStopIdx = Array.IndexOf(cachedStopOrder, nextStop);
                if (currentVehicle.m_nextStopIdx == 0)
                {
                    currentVehicle.m_nextStopIdx = m_loadedStopData.Count;
                }

                VehicleInfo info = vehicleData.Info;
                currentVehicle.m_progressState
                    = (vehicleData.m_flags & (Vehicle.Flags.Leaving)) != 0 ? VehicleStopProgressState.EXITING_FROM_PREVIOUS
                    : (vehicleData.m_flags & (Vehicle.Flags.Arriving)) != 0 ? VehicleStopProgressState.ARRIVING
                    : (vehicleData.m_flags & (Vehicle.Flags.Stopped)) != 0 ? VehicleStopProgressState.PREVIOUS
                    : VehicleStopProgressState.ON_ROUTE;

                currentVehicle.m_progressItemIdx = idx == 0 ? 0 : m_loadedVehiclesData.Take(idx).Count(x => currentVehicle.m_nextStopIdx == x.m_nextStopIdx && currentVehicle.m_progressState == x.m_progressState);
                currentVehicle.VehicleColor = info.m_vehicleAI.GetColor(vehicleId, ref vehicleData, 0);

                info.m_vehicleAI.GetBufferStatus(vehicleId, ref vehicleData, out _, out currentVehicle.m_passengers, out currentVehicle.m_capacity);

                ITMTransportLineStatusesManager.instance.GetCurrentIncomeAndExpensesForVehicles(vehicleId, out var incC, out var expC);
                ITMTransportLineStatusesManager.instance.GetLastWeekIncomeAndExpensesForVehicles(vehicleId, out var incL, out var expL);
                ITMTransportLineStatusesManager.instance.GetIncomeAndExpensesForVehicle(vehicleId, out var incA, out var expA);
                currentVehicle.m_profitAllTime = (incA - expA) * .01f;
                currentVehicle.m_profitLastWeek = (incL - expL) * .01f;
                currentVehicle.m_profitCurrentWeek = (incC - expC) * .01f;


                vehicleId = vehicleData.m_nextLineVehicle;
                if (++idx >= bufferV.Length)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            m_loadedVehiclesData.SetCapacity(idx);
        }

        protected override void OnIdChanged(InstanceID currentId)
        {
            m_currentLine = currentId.TransportLine;
            m_currentLineData?.Dispose();
            m_currentLineData = LineData.FromLine(m_currentLine);
            Visible = true;
            m_loadedStopData.Clear();
            ref TransportLine tl = ref TransportManager.instance.m_lines.m_buffer[m_currentLine];
            ushort currentStop = tl.GetStop(0);
            for (int i = 0; currentStop != 0 && i < 65536; currentStop = tl.GetStop(++i))
            {
                m_loadedStopData.Add(StationData.FromStop(currentStop));
            }
            if (m_currentLineData.m_type.HasVehicles())
            {
                UpdateVehicleButtons(m_currentLine, true);
            }
            else
            {
                m_loadedVehiclesData.Clear();
            }
        }

        protected override void OnFixedUpdateIfVisible()
        {
            if (m_currentLoadedColor != m_currentLineData.LineColor)
            {
                var lineColor = m_currentLineData.LineColor;
                TexStation.SetPixels(m_baseStation.GetPixels().Select(x => x == Color.black ? lineColor : x).ToArray());
                TexStation.Apply();
                TexStationFree.SetPixels(m_baseStationFree.GetPixels().Select(x => x == Color.black ? lineColor : x).ToArray());
                TexStationFree.Apply();
                TexStationHigh.SetPixels(m_baseStationHigh.GetPixels().Select(x => x == Color.black ? lineColor : x).ToArray());
                TexStationHigh.Apply();
                TexLineBg.SetPixels(m_baseLineBg.GetPixels().Select(x => x == Color.black ? lineColor : x).ToArray());
                TexLineBg.Apply();
                m_currentLoadedColor = lineColor;
            }
        }
    }

}

