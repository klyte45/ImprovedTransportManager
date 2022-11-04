using ColossalFramework.UI;
using ImprovedTransportManager.Localization;
using Kwytto.Utils;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    public class ITMLineVehicleSelectionWindow : ITMBaseWipDependentWindow<ITMLineVehicleSelectionWindow, PublicTransportWorldInfoPanel>
    {
        protected override bool showOverModals => false;
        protected override bool requireModal => false;
        protected override bool ShowCloseButton => false;
        protected override bool ShowMinimizeButton => true;
        protected override float FontSizeMultiplier => .9f;
        protected override bool Resizable => false;
        protected override string InitTitle => Str.itm_vehicleSelectWindow_title;
        protected override Vector2 StartSize => new Vector2(400, 600);
        protected override Vector2 StartPosition => new Vector2(600, 256);
        protected override Tuple<UIComponent, PublicTransportWorldInfoPanel>[] ComponentsWatching => ModInstance.Controller.PTPanels;

        protected override void DrawWindow(Vector2 size)
        {

        }

        protected override void OnIdChanged(InstanceID currentId)
        {

        }
    }
}
