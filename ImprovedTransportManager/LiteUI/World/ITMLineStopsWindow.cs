using ColossalFramework;
using ColossalFramework.UI;
using ImprovedTransportManager.Utility;
using Kwytto.LiteUI;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    public class ITMLineStopsWindow : GUIRootWindowBase
    {
        protected override bool showOverModals => false;
        protected override bool requireModal => false;
        protected override bool ShowCloseButton => false;
        protected override bool ShowMinimizeButton => true;
        protected override float FontSizeMultiplier => .9f;


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

        public static ITMLineStopsWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GameObjectUtils.CreateElement<ITMLineStopsWindow>(UIView.GetAView().transform);
                    instance.Init(ModInstance.Instance.GeneralName, new Rect(0, 0, 600, 600), resizable: false, minSize: new Vector2(100, UIScaler.MaxHeight - (300 / UIScaler.UIScale)), hasTitlebar: true);
                    instance.Visible = false;
                }
                return instance;
            }
        }

        private static ITMLineStopsWindow instance;
        private Tuple<UIComponent, PublicTransportWorldInfoPanel>[] currentBWIP;

        public void Awake()
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
        }
        GUIStyle m_noBreakLabel;

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
                var bufferV = TransportManager.instance.m_lines.m_buffer;
                var bufferN = NetManager.instance.m_nodes.m_buffer;
                ref TransportLine tl = ref bufferV[m_currentLine];
                m_currentLineData.GetUpdated();
                if (m_loadedStopData.Count == 0)
                {
                    ushort nextStop = tl.GetStop(0);
                    for (int i = 0; nextStop != 0 && i < 65536; nextStop = tl.GetStop(++i))
                    {
                        ref NetNode nd = ref bufferN[nextStop];
                        m_loadedStopData.Add(new StationData
                        {
                            stopId = nextStop,
                            cachedName = $"Stop #{nextStop}",
                            distanceNextStop = -1000,
                            position = nd.m_position,
                            tariffMultiplier = nd.m_position.DistrictTariffMultiplierHere()
                        });
                    }
                }
                using (var scroll = new GUILayout.ScrollViewScope(m_mapScroll))
                {
                    var lineLengthPixels = (m_loadedStopData.Count + .125f) * STATION_SIZE;
                    GUILayout.Space(lineLengthPixels);
                    GUI.DrawTexture(new Rect(200, 0, TexLineBg.width, lineLengthPixels), TexLineBg, ScaleMode.StretchToFill);
                    for (int i = 0; i < m_loadedStopData.Count; i++)
                    {
                        StationData stop = m_loadedStopData[i];
                        var targetTex = GetStationImage(stop);
                        var labelWidth = size.x - (targetTex.width + 26);
                        var stationPosMapY = ((i + .25f) * STATION_SIZE) - (targetTex.height * .5f);
                        if (GUI.Button(new Rect(200, stationPosMapY, targetTex.width, targetTex.height), targetTex, m_stationBtn))
                        {
                            ToolsModifierControl.cameraController.SetTarget(new InstanceID { NetNode = stop.stopId }, stop.position, false);
                        }
                        var textsBasePosition = new Vector2(targetTex.width + 206, stationPosMapY);
                        GUI.Label(new Rect(textsBasePosition, new Vector2(labelWidth, 20)), $"<b>{stop.cachedName}</b>");
                        GUI.Label(new Rect(textsBasePosition + new Vector2(0, 17), new Vector2(labelWidth, 20)), "54656546546 54d56a4d56a4d5 A", m_smallLabel);
                        GUI.Label(new Rect(textsBasePosition + new Vector2(0, 34), new Vector2(labelWidth, 20)), "lkjad lkd akljd adjakl ", m_smallLabel);
                        GUI.Label(new Rect(textsBasePosition + new Vector2(0, 51), new Vector2(labelWidth, 20)), "kjad lkd akljd aaaaaa ", m_smallLabel);
                        GUI.Label(new Rect(new Vector2(textsBasePosition.x, stationPosMapY + (STATION_SIZE * .66f)), new Vector2(labelWidth, 20)), $"<i><color=cyan>{stop.distanceNextStop:N0}m</color></i>");
                    }
                    foreach (var vehicle in m_loadedVehiclesData)
                    {
                        var position = vehicle.GetPositionOffset(STATION_SIZE);
                        if (GUI.Button(new Rect(position, new Vector2(50, 20)), vehicle.VehicleName, vehicle.CachedStyle))
                        {
                            ToolsModifierControl.cameraController.SetTarget(new InstanceID { Vehicle = vehicle.VehicleId }, default, false);
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
            if (m_stationBtn is null)
            {
                m_stationBtn = new GUIStyle(GUI.skin.label)
                {
                    contentOffset = default,
                    padding = new RectOffset(),
                    hover = GUI.skin.button.hover,
                };
            }
        }

        private Vector2 m_mapScroll;

        private GUIStyle m_smallLabel;
        private GUIStyle m_stationBtn;
        private ushort m_currentLine;
        private LineData m_currentLineData;
        private uint m_vehicleRecalcFrame;


        private void FixedUpdate()
        {
            if (currentBWIP is null)
            {
                var BWIPs = UIView.GetAView().GetComponentsInChildren<PublicTransportWorldInfoPanel>();
                if (BWIPs is null || BWIPs.Length == 0)
                {
                    return;
                }
                currentBWIP = BWIPs.Select(x => Tuple.New(x.GetComponent<UIComponent>(), x)).ToArray();
            }
            if (currentBWIP.FirstOrDefault(x => x.First.isVisible) is Tuple<UIComponent, PublicTransportWorldInfoPanel> window)
            {
                if (m_currentLine != WorldInfoPanel.GetCurrentInstanceID().TransportLine)
                {
                    m_currentLine = WorldInfoPanel.GetCurrentInstanceID().TransportLine;
                    if (m_currentLine > 0)
                    {
                        m_currentLineData?.Dispose();
                        m_currentLineData = LineData.FromLine(m_currentLine);
                        Visible = true;
                        m_loadedStopData.Clear();
                        UpdateVehicleButtons(m_currentLine, true);
                    }
                }
                else if (m_currentLine > 0)
                {
                    UpdateVehicleButtons(m_currentLine);
                }
            }
            else
            {
                Visible = false;
                m_currentLine = 0;
            }

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
        protected override void OnWindowDestroyed()
        {
            instance = null;
        }
        private Texture2D GetStationImage(StationData s)
        {
            if (s.tariffMultiplier > 1.001f)
            {
                return TexStationHigh;
            }
            else
            if (s.tariffMultiplier < 0.999f)
            {
                return TexStationFree;
            }
            else
            {
                return TexStation;
            }
        }

        private class StationData
        {
            public ushort stopId;
            public string cachedName;
            public float distanceNextStop;
            public Vector3 position;
            public float tariffMultiplier;


        }

        private class VehicleData : IDisposable
        {

            private Color m_vehicleColor;
            private Texture2D m_cachedBg;
            private GUIStyle m_cachedStyle;
            public ushort m_nextStop;
            public int m_nextStopIdx;
            public int m_progressItemIdx;
            public VehicleStopProgressState m_progressState;
            public int m_capacity;
            public int m_passengers;
            public ushort VehicleId { get; set; }
            public Color VehicleColor
            {
                get => m_vehicleColor; set
                {
                    if (m_vehicleColor != value)
                    {
                        m_vehicleColor = value;
                        Destroy(m_cachedBg);
                        m_cachedBg = null;
                        m_cachedStyle = null;
                    }
                }
            }
            public GUIStyle CachedStyle
            {
                get
                {
                    if (m_cachedStyle == null)
                    {
                        var contrast = m_vehicleColor.ContrastColor();
                        m_cachedStyle = new GUIStyle(GUI.skin.label)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontStyle = FontStyle.Bold,
                            normal =
                            {
                                textColor =contrast,
                                background = CachedBG
                            },
                        };
                    }
                    return m_cachedStyle;
                }
            }
            public Texture2D CachedBG
            {
                get
                {
                    if (m_cachedBg is null)
                    {
                        m_cachedBg = TextureUtils.NewSingleColorForUI(VehicleColor);
                    }
                    return m_cachedBg;
                }
            }
            public string VehicleName => $"#{VehicleId}";

            public float StationPositionMultiplierY => m_nextStopIdx + ((float)m_progressState * .25f);
            public Vector2 GetPositionOffset(float stationHeight) => new Vector2(150 - (m_progressItemIdx % 4 * 50f), (stationHeight * StationPositionMultiplierY) + (Mathf.Floor(m_progressItemIdx * .25f) * 18) - 9f);

            public void Dispose()
            {
                Destroy(m_cachedBg);
            }
        }

        private enum VehicleStopProgressState
        {
            PREVIOUS = -3,
            EXITING_FROM_PREVIOUS,
            ON_ROUTE,
            ARRIVING
        }

        private enum VehicleShowDataType
        {
            PassengerCapacity,
            Identifier,
            EarningsAllTime,
            EarningsLastWeek,
            EarningsCurrentWeek
        }


        private void UpdateVehicleButtons(ushort lineID, bool force = false)
        {
            if (!force && m_vehicleRecalcFrame + 23 < SimulationManager.instance.m_referenceFrameIndex)
            {
                return;
            }
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

                vehicleId = vehicleData.m_nextLineVehicle;
                if (++idx >= bufferV.Length)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            m_loadedVehiclesData.SetCapacity(idx);
        }
    }
}
