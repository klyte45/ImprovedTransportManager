using ColossalFramework.UI;
using ImprovedTransportManager.TransportSystems;
using Kwytto.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ImprovedTransportManager.Utility
{
    public static class ITMAssetUtils
    {
        public static HashSet<VehicleInfo> GetAllLoadedForType(this List<string> assetNames, TransportSystemType targetTst)
        {
            var result = new HashSet<VehicleInfo>();
            assetNames.Select(x => VehiclesIndexes.instance.PrefabsData.TryGetValue(x, out var val) ? val.Info as VehicleInfo : null).Where(x => x != null).ForEach(x => result.Add(x));
            return result;
        }
    }
}
