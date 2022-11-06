extern alias CD;

using CD::CustomData.Overrides;
using ImprovedTransportManager.ModShared;
using UnityEngine;

namespace K45_ITM2CD
{
    public class BridgeCD : IBridgeCD
    {
        public bool CustomDataAvailable => true;

        public int Priority => 0;

        public bool IsBridgeEnabled => true;

        public bool GetAddressStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out int number, out string streetName)
            => CDFacade.Instance.GetStreetAndNumber(sidewalk, midPosBuilding, out number, out streetName);

        public Texture2D GetLineIcon(ushort lineId)
            => CDFacade.Instance.GetLineIcon(lineId);

        public void SetLineIcon(ushort lineId, Texture2D newIcon)
            => CDFacade.Instance.SetLineIcon(lineId, newIcon);
    }
}
