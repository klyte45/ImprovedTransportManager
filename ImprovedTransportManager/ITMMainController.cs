using ColossalFramework.UI;
using ImprovedTransportManager.UI;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImprovedTransportManager
{
    public class ITMMainController : BaseController<ModInstance, ITMMainController>
    {
        public const ulong REALTIME_MOD_ID = 1420955187;

        private readonly List<GameObject> refGOs = new List<GameObject>();
        protected override void StartActions()
        {
            base.StartActions();
            refGOs.Add(GameObjectUtils.CreateElement<LinesListingUI>(UIView.GetAView().gameObject.transform, "LinesListingUI").gameObject);
            refGOs.Add(GameObjectUtils.CreateElement<ITMCitySettingsGUI>(UIView.GetAView().gameObject.transform, "ITMCitySettingsGUI").gameObject);
            refGOs.Add(GameObjectUtils.CreateElement<ITMStatisticsGUI>(UIView.GetAView().gameObject.transform, "ITMStatisticsGUI").gameObject);

            refGOs.Add(ITMLineDataWindow.Instance.gameObject);
            refGOs.Add(ITMLineStopsWindow.Instance.gameObject);
            refGOs.Add(ITMLineVehicleSelectionWindow.Instance.gameObject);
        }


        private Tuple<UIComponent, PublicTransportWorldInfoPanel>[] ptPanels;
        public Tuple<UIComponent, PublicTransportWorldInfoPanel>[] PTPanels
        {
            get
            {
                if (ptPanels is null)
                {
                    var BWIPs = UIView.GetAView().GetComponentsInChildren<PublicTransportWorldInfoPanel>();
                    if (BWIPs is null || BWIPs.Length == 0)
                    {
                        return null;
                    }
                    ptPanels = BWIPs.Select(x => Tuple.New(x.GetComponent<UIComponent>(), x)).ToArray();
                }
                return ptPanels;
            }
        }

        public void OnDestroy()
        {
            foreach (GameObject go in refGOs)
            {
                Destroy(go);
            }
        }

        #region Real Time
        public readonly bool m_isRealTimeEnabled = PluginUtils.VerifyModEnabled(REALTIME_MOD_ID);



        #endregion
    }
}
