using ColossalFramework.UI;
using Kwytto.Utils;
using System.Linq;

namespace ImprovedTransportManager.Overrides
{
    public class UIDynamicPanelsOverrides : Redirector, IRedirectable
    {
        public void Awake()
        {
            var src = typeof(UIDynamicPanels).GetMethods().Where(x => x.Name == "Show" && !x.ContainsGenericParameters && !x.IsGenericMethod && x.ReturnType == typeof(UIComponent) && x.GetParameters().Length == 3).First();
            var dest = GetType().GetMethod("AfterShow", RedirectorUtils.allFlags);
            LogUtils.DoLog($"pre detour: {src} => {dest}");
            AddRedirect(src, null, dest);
        }

        public static void AfterShow(ref UIComponent __result)
        {
            var panelCompoent = __result.GetComponent<PublicTransportDetailPanel>();
            if (panelCompoent != null)
            {
                __result.Hide();
                ModInstance.LinesListingBtn.Open();
            }
        }
    }
}
