using ColossalFramework.UI;
using ImprovedTransportManager.UI;
using Kwytto.Interfaces;
using Kwytto.Utils;

namespace ImprovedTransportManager
{
    public class ITMMainController : BaseController<ModInstance, ITMMainController>
    {
        protected override void StartActions()
        {
            base.StartActions();
            GameObjectUtils.CreateElement<LinesListingUI>(UIView.GetAView().gameObject.transform, "LinesListingUI");

        }
    }
}
