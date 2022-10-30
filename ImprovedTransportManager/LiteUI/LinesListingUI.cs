using ColossalFramework;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.TransportSystems;
using Kwytto.LiteUI;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    internal class LinesListingUI : GUIOpacityChanging
    {
        protected override bool showOverModals => false;
        protected override bool requireModal => false;
        public static LinesListingUI Instance { get; private set; }
        public override void Awake()
        {
            base.Awake();
            Instance = this;
            Init(Str.itm_linesListingWindow_tilte, new Rect(128, 128, 680, 420), resizable: true, minSize: new Vector2(440, 260));

            Visible = false;
        }

        private uint m_lastUsedCount = 0;
        private readonly Dictionary<InstanceID, LineListItem> m_lines = new Dictionary<InstanceID, LineListItem>();
        private Vector2 m_scrollLines;

        private GUIStyle m_LineBasicLabelStyle;
        private GUIStyle m_LineBasicTextStyle;
        private readonly Texture2D m_iconGoToLine = KResourceLoader.LoadTextureKwytto(Kwytto.UI.CommonsSpriteNames.K45_Right);

        protected override void DrawWindow(Vector2 size)
        {
            if (m_LineBasicLabelStyle is null)
            {
                m_LineBasicLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    stretchHeight = true,
                    fixedWidth = 40,
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.CeilToInt(14 * ResolutionMultiplier),
                    padding = new RectOffset(),
                    contentOffset = default
                };
            }
            if (m_LineBasicTextStyle is null)
            {
                m_LineBasicTextStyle = new GUIStyle(GUI.skin.textField)
                {
                    stretchHeight = true,
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = Mathf.CeilToInt(13 * ResolutionMultiplier),
                    padding = new RectOffset(),
                    contentOffset = default
                };
            }
            if (m_lastUsedCount != TransportManager.instance.m_lines.ItemCount())
            {
                m_lines.Clear();
                var buff = Singleton<TransportManager>.instance.m_lines.m_buffer;
                for (ushort lineID = 1; lineID < TransportManager.instance.m_lines.m_buffer.Length; lineID++)
                {
                    if ((buff[lineID].m_flags & (TransportLine.Flags.Created | TransportLine.Flags.Temporary)) == TransportLine.Flags.Created)
                    {
                        m_lines[new InstanceID { TransportLine = lineID }] = LineListItem.FromLine(lineID);
                    }
                }
            }
            using (var scroll = new GUILayout.ScrollViewScope(m_scrollLines))
            {
                foreach (var line in m_lines.Values)
                {
                    line.GetUpdated();
                    using (new GUILayout.HorizontalScope(GUILayout.Height(20)))
                    {
                        GUILayout.Label("0", m_LineBasicLabelStyle);
                        var rect = GUILayoutUtility.GetLastRect();
                        GUI.DrawTexture(rect, line.m_uiTextureColor);
                        GUI.Label(rect, $"<color=#{line.LineColor.ContrastColor().ToRGB()}>{line.LineIdentifier()}</color>", m_LineBasicLabelStyle);
                        var oldName = line.LineName;
                        if (GUILayout.TextField(oldName, GUILayout.Width(size.x - 340), GUILayout.ExpandHeight(true)) is string str && str != oldName)
                        {
                            line.LineName = oldName;
                        }
                        GUILayout.Label($"{line.m_stopsCount:N0}", m_LineBasicLabelStyle);
                        GUILayout.Label($"{line.m_budget:N0}%", m_LineBasicLabelStyle);
                        GUILayout.Label($"{line.m_vehiclesCount:N0}/{line.m_vehiclesTarget:N0}", m_LineBasicLabelStyle);
                        GUILayout.Label($"{line.m_passengersResCount + line.m_passengersTouCount:N0}", m_LineBasicLabelStyle);
                        GUILayout.Label($"{line.m_lineFinancesBalance:C}", m_LineBasicLabelStyle, GUILayout.Width(80));
                        GUIKwyttoCommons.SquareTextureButton(m_iconGoToLine, "", () => { }, size: 20);
                    }
                }
                m_scrollLines = scroll.scrollPosition;
            }

        }
        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();
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
    }

    internal class LineListItem
    {
        public uint m_lastUpdate;

        public InstanceID m_id;
        public TransportSystemType m_type;
        public string LineName
        {
            get => TransportManager.instance.GetLineName(m_id.TransportLine);
            set => ModInstance.Controller.StartCoroutine(TransportManager.instance.SetLineName(m_id.TransportLine, value));
        }
        public Func<string> LineIdentifier { get; private set; }
        public Func<ushort> LineInternalSequentialNumber { get; private set; }
        public Color LineColor
        {
            get
            {
                var clr = TransportManager.instance.GetLineColor(m_id.TransportLine);
                if (m_uiTextureColor.GetPixel(0, 0) != clr)
                {
                    m_uiTextureColor.SetPixels(new[] { clr });
                    m_uiTextureColor.Apply();
                }
                return clr;
            }

            set => ModInstance.Controller.StartCoroutine(TransportManager.instance.SetLineColor(m_id.TransportLine, value));
        }
        public int m_stopsCount;
        public uint m_passengersResCount;
        public uint m_passengersTouCount;
        public int m_budget;
        public int m_vehiclesCount;
        public int m_vehiclesTarget;
        public float m_lineFinancesBalance;
        public readonly Texture2D m_uiTextureColor = TextureUtils.New(1, 1);


        internal static LineListItem FromLine(ushort lineID)
        {
            TransportLine[] tlBuff = TransportManager.instance.m_lines.m_buffer;
            return new LineListItem
            {
                m_id = new InstanceID { TransportLine = lineID },
                m_type = TransportSystemTypeExtensions.FromLineId(lineID, false),
                LineIdentifier = () => tlBuff[lineID].m_lineNumber.ToString(),
                LineInternalSequentialNumber = () => tlBuff[lineID].m_lineNumber,

            };
        }

        public LineListItem GetUpdated()
        {
            if (SimulationManager.instance.m_currentTickIndex - 30 > m_lastUpdate)
            {
                _ = LineColor;
                m_lastUpdate = SimulationManager.instance.m_currentTickIndex;
                ref TransportLine refLine = ref Singleton<TransportManager>.instance.m_lines.m_buffer[m_id.TransportLine];
                m_stopsCount = refLine.CountStops(m_id.TransportLine);
                m_passengersResCount = refLine.m_passengers.m_residentPassengers.m_averageCount;
                m_passengersTouCount = refLine.m_passengers.m_touristPassengers.m_averageCount;
                m_budget = Singleton<EconomyManager>.instance.GetBudget(refLine.Info.m_class); // ATUALIZAR COM VALOR CUSTOMIZADO!
                m_vehiclesCount = refLine.CountVehicles(m_id.TransportLine);
                m_vehiclesTarget = -1; //ATUALIZAR COM O CALCULO!
                m_lineFinancesBalance = 0f; //ATUALIZAR COM O SALDO!
            }
            return this;
        }

        ~LineListItem()
        {

        }
    }
}
