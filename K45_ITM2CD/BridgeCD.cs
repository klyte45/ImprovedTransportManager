extern alias CD;

using CD::CustomData.Overrides;
using ColossalFramework.Plugins;
using ImprovedTransportManager.ModShared;
using System;
using System.Linq;
using UnityEngine;

namespace K45_ITM2CD
{
    public class BridgeCD : IBridgeCD
    {
        public bool CustomDataAvailable => true;

        public int Priority => 0;

        public bool IsBridgeEnabled { get; private set; }

        public BridgeCD()
        {
            if (!PluginManager.instance.GetPluginsInfo().Any(x => x.assemblyCount > 0 && x.isEnabled && x.ContainsAssembly(typeof(CDFacade).Assembly)))
            {
                throw new Exception("The Custom Data bridge isn't available due to the mod not being active. Using fallback!");
            }
        }

        public bool GetAddressStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out int number, out string streetName)
            => CDFacade.Instance.GetStreetAndNumber(sidewalk, midPosBuilding, out number, out streetName);

        public Texture2D GetLineIcon(ushort lineId)
            => CDFacade.Instance.GetLineIcon(lineId);


        public void SetLineIcon(ushort lineId, Texture2D newIcon)
            => CDFacade.Instance.SetLineIcon(lineId, newIcon);


        public string GetVehicleIdentifier(ushort vehicleId) 
            => CDFacade.Instance.GetVehicleIdentifier(vehicleId);
    }
}
