using ColossalFramework;
using ColossalFramework.Globalization;
using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.Singleton;
using ImprovedTransportManager.TransportSystems;
using ImprovedTransportManager.Utility;
using Kwytto.LiteUI;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VehicleSkins.Localization;
using WriteEverywhere.Tools;

namespace ImprovedTransportManager.UI
{
    public partial class ITMLineStopsWindow : GUIOpacityChanging
    {
        protected override bool showOverModals => false;
        protected override bool requireModal => false;
        protected override bool ShowCloseButton => false;
        protected override bool ShowMinimizeButton => true;
        protected override float FontSizeMultiplier => .9f;
        protected bool Resizable => true;
        protected string InitTitle => Str.itm_lineMap_title;
        protected Vector2 StartSize => new Vector2(700, 700);
        protected Vector2 StartPosition => default;
        protected Vector2 MinSize => new Vector2(700, 300);
        protected Vector2 MaxSize => new Vector2(700, 999999);


        private const int STATION_SIZE = 120;
        private const string LOGO_CTX_MENU_ID = "LINELOGO_CTXMENU_$$_";
        private GUIColorPicker picker;
        private Texture2D m_baseStation;
        private Texture2D m_baseStationFree;
        private Texture2D m_baseStationHigh;
        private Texture2D m_baseStationTerminus;
        private Texture2D m_baseStationTerminusFree;
        private Texture2D m_baseStationTerminusHigh;


        public static ITMLineStopsWindow Instance { get; private set; }

        private Texture2D m_baseLineBg;
        public Texture2D TexStation { get; private set; }
        public Texture2D TexStationFree { get; private set; }
        public Texture2D TexStationHigh { get; private set; }
        public Texture2D TexStationTerminus { get; private set; }
        public Texture2D TexStationTerminusFree { get; private set; }
        public Texture2D TexStationTerminusHigh { get; private set; }
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

        private string m_currentCtx;

        public override void Awake()
        {
            base.Awake();
            Init();
            Instance = this;
            m_baseLineBg = KResourceLoader.LoadTextureMod("map_lineBase");
            m_baseStation = KResourceLoader.LoadTextureMod("map_station");
            m_baseStationFree = KResourceLoader.LoadTextureMod("map_stationFree");
            m_baseStationHigh = KResourceLoader.LoadTextureMod("map_stationHigh");
            m_baseStationTerminus = KResourceLoader.LoadTextureMod("map_stationTerminus");
            m_baseStationTerminusFree = KResourceLoader.LoadTextureMod("map_stationTerminusFree");
            m_baseStationTerminusHigh = KResourceLoader.LoadTextureMod("map_stationTerminusHigh");
            TexLineBg = TextureUtils.New(m_baseLineBg.width, m_baseLineBg.height);
            TexStation = TextureUtils.New(m_baseStation.width, m_baseStation.height);
            TexStationFree = TextureUtils.New(m_baseStationFree.width, m_baseStationFree.height);
            TexStationHigh = TextureUtils.New(m_baseStationHigh.width, m_baseStationHigh.height);
            TexStationTerminus = TextureUtils.New(m_baseStationTerminus.width, m_baseStationTerminus.height);
            TexStationTerminusFree = TextureUtils.New(m_baseStationTerminusFree.width, m_baseStationTerminusFree.height);
            TexStationTerminusHigh = TextureUtils.New(m_baseStationTerminusHigh.width, m_baseStationTerminusHigh.height);
            picker = GameObjectUtils.CreateElement<GUIColorPicker>(transform).Init();
            picker.Visible = false;
            Visible = false;
        }
        private void Init() => Init(InitTitle, new Rect(StartPosition, StartSize), Resizable, true, MinSize, MaxSize);

        private GUIStyle m_redButton;
        private bool m_dirtyStops;

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
                UpdateVehicleButtons(m_currentLine);
                using (new GUILayout.HorizontalScope())
                {
                    if (m_currentLineData.m_type.HasVehicles())
                    {
                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Space(5);
                            GUILayout.Label(Str.itm_lineMap_vehicleDataToShow, m_centerLabel);
                            using (new GUILayout.HorizontalScope())
                            {
                                m_currentVehicleDataShow = (VehicleShowDataType)GUIComboBox.Box((int)m_currentVehicleDataShow, m_vehicleShowOptions, "itmLineStopsVehicleDataShow", this, 200);
                            }
                            GUILayout.Space(5);
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"{Str.itm_lineMap_earningsLastPeriodAcronymLegend}\n{Str.itm_lineMap_earningsCurrentPeriodAcronymLegend}\n{Str.itm_lineMap_earningsAllTimeAcronymLegend}", m_smallLabel);
                }

                var headerRect = GUILayoutUtility.GetLastRect();
                var iconRect = new Rect((headerRect.size.x - headerRect.height) / 2, headerRect.position.y, headerRect.height, headerRect.height);
                if (m_currentLineData.LineIcon is Texture2D tex)
                {
                    GUI.Label(iconRect, tex, m_stationBtn);
                }
                else
                {
                    tex = null;
                    var contrastColor = m_currentLineData.LineColor.ContrastColor();
                    GUI.DrawTexture(iconRect, GUIKwyttoCommons.whiteTexture);

                    var subRect = new Rect(iconRect.position + new Vector2(2, 2), iconRect.size - new Vector2(4, 4));

                    GUI.DrawTexture(subRect, m_currentLineData.m_uiTextureColor);
                    var identifier = m_currentLineData.LineIdentifier();
                    GUI.Label(subRect, identifier, new GUIStyle(m_lineIconText)
                    {
                        fontSize = Mathf.RoundToInt(16 * EffectiveFontSizeMultiplier * (.5f + (3f / identifier.Length))),
                        normal =
                        {
                            textColor = contrastColor
                        }
                    });
                }
                RunContextMenuLine(iconRect, tex);

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
                        var labelWidth = size.x - leftPivotLine - targetTex.width - 20;
                        var stationPosMapY = ((i + .25f) * STATION_SIZE) - (targetTex.height * .5f);
                        var stationIconRect = new Rect(leftPivotLine, stationPosMapY, targetTex.width, targetTex.height);
                        GUI.Label(stationIconRect, targetTex, m_stationBtn);
                        RunContextMenuStop(i, stop, stationIconRect);
                        var textsBasePosition = new Vector2(targetTex.width + leftPivotLine + 6, stationPosMapY);
                        var boredPercent = 1 - (stop.timeUntilBored * (1f / 255));
                        var stopNameRect = new Rect(textsBasePosition, new Vector2(labelWidth, 20));
                        GUI.Label(stopNameRect, $"<b>{stop.cachedName}</b>");
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
                                DefaultTool.OpenWorldInfoPanel(new InstanceID { Vehicle = vehicle.VehicleId }, default);
                                ToolsModifierControl.cameraController.SetTarget(new InstanceID { Vehicle = vehicle.VehicleId }, default, false);
                            }
                        }
                    }

                    m_mapScroll = scroll.scrollPosition;
                }
                GUILayout.FlexibleSpace();
            }
        }

        private void RunContextMenuLine(Rect iconRect, Texture2D tex)
        {
            if (m_currentCtx == LOGO_CTX_MENU_ID || iconRect.Contains(GUIUtility.ScreenToGUIPoint(default) + UIScaler.MousePosition))
            {
                var CDavailable = ModInstance.Controller.ConnectorCD.CustomDataAvailable;
                string[] m_logoOptionsArray = new string[] {
                            Str.itm_lineMap_recalculateAllStopNames,
                       CDavailable? tex!=null?  Str.itm_lineLogo_changeCustomLogo: Str.itm_lineLogo_setCustomLogo: null,
                            CDavailable&& tex!=null?       Str.itm_lineLogo_deleteCustomLogo : null,
                            }.Where(x => x != null).ToArray();
                if (m_logoOptionsArray.Length > 0 && GUIComboBox.ContextMenuRect(iconRect, m_logoOptionsArray, LOGO_CTX_MENU_ID, this, "", GUI.skin.label) is int idx)
                {
                    switch (idx)
                    {
                        case -2:
                            m_currentCtx = LOGO_CTX_MENU_ID;
                            break;
                        case -3:
                            m_currentCtx = null;
                            break;
                    }
                    if (idx >= 0)
                    {
                        var optionSelected = m_logoOptionsArray[idx];
                        if (optionSelected == Str.itm_lineLogo_changeCustomLogo || optionSelected == Str.itm_lineLogo_setCustomLogo)
                        {
                            KCImageFilePicker.PickAFile(string.Format(Str.itm_pickALogoForLine, m_currentLineData.LineName), OnNewLogoPicked);
                        }
                        else if (optionSelected == Str.itm_lineLogo_deleteCustomLogo)
                        {
                            ModInstance.Controller.ConnectorCD.SetLineIcon(m_currentLineData.m_id.TransportLine, null);
                        }
                        else if (optionSelected == Str.itm_lineMap_recalculateAllStopNames)
                        {
                            m_loadedStopData.ForEach(x => ITMNodeSettings.Instance.GetNodeName(x.stopId, true));
                        }
                    }
                }
            }
        }

        private void RunContextMenuStop(int i, StationData stop, Rect stationIconRect)
        {
            var contextMenuName = $"{stop.stopId}_CTXMENU_$$_";
            if (m_currentCtx == contextMenuName || stationIconRect.Contains(GUIUtility.ScreenToGUIPoint(default) + UIScaler.MousePosition))
            {
                var optionsArray = new string[] {
                              i > 0 ? Str.itm_lineView_setAsFirstStop : null,
                              i== 0 ? null : stop.isTerminus? Str.itm_lineView_unsetAsTerminus : Str.itm_lineView_setAsTerminus,
                              Str.itm_lineMap_forceBindToDistrict,
                              Str.itm_lineMap_forceBindToPark,
                              Str.itm_lineMap_forceBindToBuilding,
                              Str.itm_lineMap_forceBindToRoad,
                              Str.itm_lineMap_recalculateAutoBind,
                              Str.itm_lineView_removeStop
                            }.Where(x => x != null).ToArray();
                if (GUIComboBox.ContextMenuRect(stationIconRect, optionsArray, contextMenuName, this, "", GUI.skin.label) is int idx)
                {
                    switch (idx)
                    {
                        case -2:
                            m_currentCtx = contextMenuName;
                            DefaultTool.OpenWorldInfoPanel(new InstanceID { NetNode = stop.stopId }, stop.position);
                            ToolsModifierControl.cameraController.SetTarget(new InstanceID { NetNode = stop.stopId }, stop.position, false);
                            break;
                        case -3:
                            m_currentCtx = null;
                            break;
                    }

                    if (idx >= 0)
                    {
                        var selectedText = optionsArray[idx];
                        if (selectedText == Str.itm_lineView_setAsFirstStop)
                        {
                            stop.SetAsFirst();
                        }
                        else if (selectedText == Str.itm_lineView_unsetAsTerminus)
                        {
                            stop.UnsetTerminus();
                        }
                        else if (selectedText == Str.itm_lineView_setAsTerminus)
                        {
                            stop.SetTerminus();
                        }
                        else if (selectedText == Str.itm_lineView_removeStop)
                        {
                            stop.RemoveStop(() => m_dirtyStops = true);
                        }

                        else if (Str.itm_lineMap_forceBindToDistrict == selectedText)
                        {
                            if (!ITMNodeSettings.Instance.ForceBindToDistrict(stop.stopId))
                            {
                                KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                                {
                                    buttons = KwyttoDialog.basicOkButtonBar,
                                    scrollText = Str.itm_lineMap_failedSettingDistrict
                                });
                            }
                        }
                        else if (Str.itm_lineMap_forceBindToPark == selectedText)
                        {
                            if (!ITMNodeSettings.Instance.ForceBindToPark(stop.stopId))
                            {
                                KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                                {
                                    buttons = KwyttoDialog.basicOkButtonBar,
                                    scrollText = Str.itm_lineMap_failedSettingPark
                                });
                            }
                        }
                        else if (Str.itm_lineMap_forceBindToBuilding == selectedText)
                        {
                            ModInstance.Controller.BuildingToolInstance.OnBuildingSelect = (x) => ITMNodeSettings.Instance.ForceBindToBuilding(stop.stopId, x);
                            ToolsModifierControl.SetTool<BuildingSelectorTool>();
                        }
                        else if (Str.itm_lineMap_forceBindToRoad == selectedText)
                        {
                            ModInstance.Controller.RoadSegmentToolInstance.OnSegmentSelect = (x) => ITMNodeSettings.Instance.ForceBindToRoad(stop.stopId, x);
                            ToolsModifierControl.SetTool<SegmentSelectorTool>();
                        }
                        else if (Str.itm_lineMap_recalculateAutoBind == selectedText)
                        {
                            ITMNodeSettings.Instance.GetNodeName(stop.stopId, true);
                        }

                        m_currentCtx = null;
                    }
                }
            }
        }

        private void OnNewLogoPicked(string x)
        {
            if (x != null)
            {
                var result = TextureAtlasUtils.LoadTextureFromFile(x, linear: true);
                if (result.width != 256 || result.height != 256)
                {
                    ModInstance.Controller.StartCoroutine(ShowErrorModal());
                    Destroy(result);
                }
                else
                {
                    ModInstance.Controller.ConnectorCD.SetLineIcon(m_currentLineData.m_id.TransportLine, result);
                }
            }
        }
        public IEnumerator ShowErrorModal()
        {
            yield return 0;
            yield return 0;
            KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
            {
                title = Str.itm_lineLogo_invalidTexture,
                message = Str.itm_lineLogo_invalidTextureContent,
                messageAlign = TextAnchor.MiddleCenter,
                messageTextSizeMultiplier = 1.5f,
                buttons = KwyttoDialog.basicOkButtonBar
            });
        }

        public bool HasAnyFreeStop() => m_loadedStopData.Any(x => x.fareMultiplier == 0);

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
                    fontStyle = FontStyle.Bold,
                    hover = ModInstance.Controller.ConnectorCD.CustomDataAvailable ? GUI.skin.button.hover : GUI.skin.label.hover, //Only if CD exists
                };
            }
        }
        private Texture2D GetStationImage(StationData s)
            => s.isTerminus
            ? s.fareMultiplier > 1.001f
                ? TexStationTerminusHigh
                : s.fareMultiplier < 0.999f
                    ? TexStationTerminusFree
                    : TexStationTerminus
            : s.fareMultiplier > 1.001f
                ? TexStationHigh
                : s.fareMultiplier < 0.999f
                    ? TexStationFree
                    : TexStation;

        private void UpdateVehicleButtons(ushort lineID, bool force = false)
        {
            if (m_loadedStopData.Count == 0 || (!force && m_loadedVehiclesLine == lineID && m_vehicleRecalcFrame + 23 > SimulationManager.instance.m_referenceFrameIndex))
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

                ITMTransportLineStatusesManager.Instance.GetCurrentIncomeAndExpensesForVehicles(vehicleId, out var incC, out var expC);
                ITMTransportLineStatusesManager.Instance.GetLastWeekIncomeAndExpensesForVehicles(vehicleId, out var incL, out var expL);
                ITMTransportLineStatusesManager.Instance.GetIncomeAndExpensesForVehicle(vehicleId, out var incA, out var expA);
                currentVehicle.m_profitCurrentWeek = (incC - expC) * .01f;
                currentVehicle.m_profitAllTime = ((incA - expA) * .01f) - currentVehicle.m_profitCurrentWeek;
                currentVehicle.m_profitLastWeek = (incL - expL) * .01f;


                vehicleId = vehicleData.m_nextLineVehicle;
                if (++idx >= bufferV.Length)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            m_loadedVehiclesData.SetCapacity(idx);
        }

        public void OnIdChanged(InstanceID currentId)
        {
            m_currentLine = currentId.TransportLine;
            Visible = true;
            ReloadStops();
            if (m_currentLineData.m_type.HasVehicles())
            {
                UpdateVehicleButtons(m_currentLine, true);
            }
            else
            {
                m_loadedVehiclesData.Clear();
            }
        }

        private void ReloadStops()
        {
            m_currentLineData?.Dispose();
            m_currentLineData = LineData.FromLine(m_currentLine);
            m_loadedStopData.Clear();
            ITMLineUtils.DoWithEachStop(m_currentLine, (x, _) => m_loadedStopData.Add(StationData.FromStop(x)));
            m_dirtyStops = false;
        }

        protected void FixedUpdate()
        {
            if (m_dirtyStops)
            {
                ReloadStops();

            }
            if (m_currentLineData != null && m_currentLoadedColor != m_currentLineData.LineColor)
            {
                var lineColor = m_currentLineData.LineColor;
                foreach (var tex in new[] {
                    Tuple.New(TexStation ,m_baseStation),
                    Tuple.New(TexStationFree ,m_baseStationFree),
                    Tuple.New(TexStationHigh ,m_baseStationHigh),
                    Tuple.New(TexStationTerminus ,m_baseStationTerminus),
                    Tuple.New(TexStationTerminusFree ,m_baseStationTerminusFree),
                    Tuple.New(TexStationTerminusHigh ,m_baseStationTerminusHigh),
                    Tuple.New(TexLineBg ,m_baseLineBg)
                })
                {

                    tex.First.SetPixels(tex.Second.GetPixels().Select(x => x == Color.black ? lineColor : x).ToArray());
                    tex.First.Apply();
                }
                m_currentLoadedColor = lineColor;
            }
        }
    }

}

