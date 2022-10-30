using ColossalFramework;
using ColossalFramework.UI;
using ImprovedTransportManager.Localization;
using Kwytto.LiteUI;
using Kwytto.Utils;
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
        private GUIStyle m_HeaderLineStyle;
        private GUIStyle m_LineBasicTextStyle;
        private readonly Texture2D m_iconGoToLine = KResourceLoader.LoadTextureKwytto(Kwytto.UI.CommonsSpriteNames.K45_Right);

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
                        m_lines[new InstanceID { TransportLine = lineID }] = LineListItem.FromLine(lineID);
                    }
                }
                m_lastUsedCount = TransportManager.instance.m_lines.ItemCount();
            }
            var lineNameSize = size.x - 345;
            using (new GUILayout.HorizontalScope(GUILayout.Height(20)))
            {
                GUILayout.Button("ID", m_HeaderLineStyle, GUILayout.Width(40));
                GUILayout.Button(Str.itm_linesListingWindow_nameColumnTitle, m_HeaderLineStyle, GUILayout.Width(lineNameSize));
                GUILayout.Button(Str.itm_linesListingWindow_stopsColumnTitle, m_HeaderLineStyle, GUILayout.Width(40));
                GUILayout.Button(Str.itm_linesListingWindow_budgetColumnTitle, m_HeaderLineStyle, GUILayout.Width(40));
                GUILayout.Button(Str.itm_linesListingWindow_vehiclesColumnTitle, m_HeaderLineStyle, GUILayout.Width(40));
                GUILayout.Button(Str.itm_linesListingWindow_passengersColumnTitle, m_HeaderLineStyle, GUILayout.Width(40));
                GUILayout.Button(Str.itm_linesListingWindow_balanceColumnTitle, m_HeaderLineStyle, GUILayout.Width(80));
                GUILayout.Space(20);
            }
            using (var scroll = new GUILayout.ScrollViewScope(m_scrollLines))
            {
                foreach (var line in m_lines.Values)
                {
                    line.GetUpdated();
                    using (new GUILayout.HorizontalScope(GUILayout.Height(22)))
                    {
                        GUILayout.Label("0", m_LineBasicLabelStyle);
                        if (line.IsHovered)
                        {
                            var rectBg = GUILayoutUtility.GetLastRect();
                            GUI.DrawTexture(new Rect(rectBg.position, new Vector2(size.x - 15, 22)), GUIKwyttoCommons.darkGreenTexture);
                        }
                        var rect = GUILayoutUtility.GetLastRect();
                        GUI.DrawTexture(rect, line.m_uiTextureColor);
                        GUI.Label(rect, $"<color=#{line.LineColor.ContrastColor().ToRGB()}>{line.LineIdentifier()}</color>", m_LineBasicLabelStyle);
                        var oldName = line.LineName;
                        if (GUILayout.TextField(oldName, GUILayout.Width(lineNameSize)) is string str && str != oldName)
                        {
                            line.LineName = oldName;
                        }
                        GUILayout.Label($"{line.m_stopsCount:N0}", m_LineBasicLabelStyle);
                        GUILayout.Label($"{line.m_budget:N0}%", m_LineBasicLabelStyle);
                        GUILayout.Label($"{line.m_vehiclesCount:N0}/{line.m_vehiclesTarget:N0}", m_LineBasicLabelStyle);
                        GUILayout.Label($"{line.m_passengersResCount + line.m_passengersTouCount:N0}", m_LineBasicLabelStyle);
                        GUILayout.Label($"{line.m_lineFinancesBalance:C}", m_HeaderLineStyle, GUILayout.Width(80));
                        GUIKwyttoCommons.SquareTextureButton(m_iconGoToLine, "", () => { }, size: 20);
                    }
                    if (Event.current.type == EventType.Repaint)
                    {
                        if (GUILayoutUtility.GetLastRect().Contains(GUIUtility.ScreenToGUIPoint(UIScaler.MousePosition)))
                        {
                            line.OnMouseEnter();
                        }
                        else
                        {
                            line.OnMouseLeave();
                        }
                    }
                }
                m_scrollLines = scroll.scrollPosition;
            }

        }

        private void InitStyles()
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
                    contentOffset = default,
                    wordWrap = false,
                };
            }
            if (m_HeaderLineStyle is null)
            {
                m_HeaderLineStyle = new GUIStyle(GUI.skin.label)
                {
                    stretchHeight = true,
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.CeilToInt(14 * ResolutionMultiplier),
                    padding = new RectOffset(),
                    contentOffset = default,
                    wordWrap = false,
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
}
