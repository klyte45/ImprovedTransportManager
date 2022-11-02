using ColossalFramework.UI;
using ImprovedTransportManager.UI;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Collections.Generic;
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
        }

        public void OnDestroy()
        {
            foreach(GameObject go in refGOs)
            {
                Destroy(go);
            }
        }
    }
}
