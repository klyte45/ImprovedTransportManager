using ImprovedTransportManager.Localization;
using Kwytto.LiteUI;
using Kwytto.UI;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    internal class ITMCitySettingsGUI : GUIOpacityChanging
    {

        public static ITMCitySettingsGUI Instance { get; private set; }
        protected override float FontSizeMultiplier => .9f;
        public override void Awake()
        {
            base.Awake();
            Instance = this;
            Init($"{ModInstance.Instance.GeneralName} - {Str.itm_citySettings_title}", new Rect(128, 128, 680, 420), resizable: true, minSize: new Vector2(440, 260));
            var tabs = new IGUIVerticalITab[] {
                new ITMGeneralTab(),
                new ITMExpressDataTab(),
                new ITMMaintenanceDataTab(),
                new ITMSpecialLineToolsTab()
                    };
            m_tabsContainer = new GUIVerticalTabsContainer(tabs);
            Visible = false;
        }
        protected override bool showOverModals => false;

        protected override bool requireModal => false;

        private GUIVerticalTabsContainer m_tabsContainer;


        protected override void DrawWindow(Vector2 size)
        {
            m_tabsContainer.DrawListTabs(new Rect(default, size), 200);
        }
        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();
            m_tabsContainer?.Reset();
        }

        protected override void OnWindowDestroyed()
        {
            Instance = null;
        }

        internal void GoToMaintenanceCost()
        {
            ModInstance.CitySettingsBtn.Open();
            m_tabsContainer.CurrentTabIdx = 1;
            GUI.BringWindowToFront(Id);
        }
    }
}
