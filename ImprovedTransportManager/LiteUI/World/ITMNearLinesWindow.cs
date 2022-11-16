using ColossalFramework.UI;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.Utility;
using Kwytto.LiteUI;
using Kwytto.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    public class ITMNearLinesWindow : ITMBaseWipDependentWindow<ITMNearLinesWindow, BuildingWorldInfoPanel>
    {
        protected override bool Resizable => false;

        protected override string InitTitle => Str.itm_nearLines_title;

        protected override Vector2 StartSize { get; } = new Vector2(450, 400) ;

        protected override Vector2 StartPosition => new Vector2(UIScaler.MaxWidth * .75f, UIScaler.MaxHeight * .75f) - (StartSize / 2);
        private Tuple<UIComponent, BuildingWorldInfoPanel>[] ptPanels;
        protected override Tuple<UIComponent, BuildingWorldInfoPanel>[] ComponentsWatching
        {
            get
            {
                if (ptPanels is null)
                {
                    var BWIPs = UIView.GetAView().GetComponentsInChildren<BuildingWorldInfoPanel>();
                    if (BWIPs is null || BWIPs.Length == 0)
                    {
                        return null;
                    }
                    ptPanels = BWIPs.Select(x => Tuple.New(x.GetComponent<UIComponent>(), x)).ToArray();
                }
                return ptPanels;
            }
        }
        private readonly HashSet<ushort> linesFound = new HashSet<ushort>();
        private Vector2 m_scrollPos;
        private GUIStyle m_btnStyle;
        protected override void DrawWindow(Vector2 size)
        {
            InitStyles();
            if (linesFound.Count == 0)
            {
                var content = new GUIContent(Str.itm_nearLines_noNearLinesHere);
                windowRect.height = TitleBarHeight + GUI.skin.label.CalcHeight(content, 200 ) + 3;
                windowRect.width = 200;
                GUILayout.Label(content);
                return;
            }
            windowRect.height = Mathf.Min(600 , TitleBarHeight + (60 * Mathf.Ceil(linesFound.Count / 3f)));
            windowRect.width = Mathf.Min(450 , 150  * linesFound.Count);

            var idx = 0;
            using (var scroll = new GUILayout.ScrollViewScope(m_scrollPos))
            {
                foreach (var line in linesFound)
                {
                    var targetRect = new Rect(idx % 3 * 150 , idx / 3 * 60 , 150 , 60 );

                    if (GUI.Button(targetRect, TransportManager.instance.GetLineName(line), m_btnStyle))
                    {
                        ITMLineDataWindow.Instance.OnIdChanged(new InstanceID { TransportLine = line });
                    }
                    idx++;
                }
                m_scrollPos = scroll.scrollPosition;
            }
        }

        private void InitStyles()
        {
            if (m_btnStyle is null)
            {
                m_btnStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    hover ={
                        background = GUIKwyttoCommons.yellowTexture,

                        },
                    normal = {
                        background = GUIKwyttoCommons.blackTexture
                        }
                };
            }
        }

        protected override void OnIdChanged(InstanceID currentId)
        {
            var pos = BuildingManager.instance.m_buildings.m_buffer[currentId.Building].m_position;
            linesFound.Clear();
            ITMLineUtils.GetNearLines(pos, 120f, linesFound);
        }
    }
}
