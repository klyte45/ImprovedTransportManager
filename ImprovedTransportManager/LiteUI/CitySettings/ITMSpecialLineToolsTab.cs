using ImprovedTransportManager.Localization;
using ImprovedTransportManager.TransportSystems;
using Kwytto.LiteUI;
using Kwytto.UI;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    public class ITMSpecialLineToolsTab : IGUIVerticalITab
    {
        public string TabDisplayName => Str.itm_specialLineTools_title;
        private Vector2 m_scrollPos;

        public void DrawArea(Vector2 tabAreaSize)
        {
            GUILayout.Label(Str.itm_specialLineTools_header);
            GUIKwyttoCommons.Space(4);
            using (var scroll = new GUILayout.ScrollViewScope(m_scrollPos))
            {
                foreach (var type in TransportSystemTypeExtensions.TransportInfoDict.Values)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (type.Local != null)
                        {
                            if (GUILayout.Button(type.Local.name))
                            {
                                TransportTool transportTool = ToolsModifierControl.SetTool<TransportTool>();
                                if (transportTool != null)
                                {
                                    transportTool.m_prefab = type.Local;
                                    transportTool.m_building = 0;
                                }
                            }
                        }
                        if (type.Intercity != null)
                        {
                            if (GUILayout.Button(type.Intercity.name))
                            {
                                TransportTool transportTool = ToolsModifierControl.SetTool<TransportTool>();
                                if (transportTool != null)
                                {
                                    transportTool.m_prefab = type.Intercity;
                                    transportTool.m_building = 0;
                                }
                            }
                        }
                    }
                }
                m_scrollPos = scroll.scrollPosition;
            }
        }

        public void Reset()
        {
        }
    }
}
