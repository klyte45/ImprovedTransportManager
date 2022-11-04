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
        private readonly List<GameObject> refGOs = new List<GameObject>();
        protected override void StartActions()
        {
            base.StartActions();
            refGOs.Add(GameObjectUtils.CreateElement<LinesListingUI>(UIView.GetAView().gameObject.transform, "LinesListingUI").gameObject);

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
    }
}
