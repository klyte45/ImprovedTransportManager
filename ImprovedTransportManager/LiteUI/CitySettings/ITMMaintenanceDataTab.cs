using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.TransportSystems;
using Kwytto.LiteUI;
using Kwytto.UI;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    public class ITMMaintenanceDataTab : IGUIVerticalITab
    {
        public string TabDisplayName => Str.itm_maintenanceData_title;
        private Vector2 m_scrollPos;

        public void DrawArea(Vector2 tabAreaSize)
        {
            GUILayout.Label(Str.itm_maintenanceData_header);
            GUILayout.Space(4);
            using (var scroll = new GUILayout.ScrollViewScope(m_scrollPos))
            {
                foreach (var type in TransportSystemTypeExtensions.TransportInfoDict)
                {
                    if (type.Value.Local != null && type.Key.HasVehicles())
                        GUIKwyttoCommons.AddIntField(tabAreaSize.x
                            , string.Format(Str.itm_maintenanceData_defaultMaintenanceFormat, type.Key.GetTransportName(), type.Key.GetDefaultPassengerCapacityCostLocal() * 1000),
                            ITMCitySettings.Instance.costPerThousandPassengers.TryGetValue(type.Key, out uint val) ? (int)val : 0, (x) => ITMCitySettings.Instance.costPerThousandPassengers[type.Key] = (uint)x, true, 0);
                }
                m_scrollPos = scroll.scrollPosition;
            }
        }

        public void Reset()
        {
        }
    }
}
