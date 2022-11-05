using ImprovedTransportManager.UI;
using Kwytto.Utils;
using System.Linq;

namespace ImprovedTransportManager.Overrides
{
    public class WorldInfoPanelOverrides : Redirector, IRedirectable
    {
        public void Awake()
        {
            var srcs = typeof(WorldInfoPanel).GetMethods(RedirectorUtils.allFlags).Where(x => x.Name == "SetTarget");
            var dest = GetType().GetMethod("PreSetTarget", RedirectorUtils.allFlags);
            LogUtils.DoLog($"pre detour: {srcs} => {dest}");
            foreach (var src in srcs)
                AddRedirect(src, dest);
        }

        public static bool PreSetTarget(InstanceID id)
        {
            if (InstanceManager.IsValid(id) && id.TransportLine != 0)
            {
                ITMLineDataWindow.Instance.OnIdChanged(id);
                return false;
            }
            return true;
        }
    }
}
