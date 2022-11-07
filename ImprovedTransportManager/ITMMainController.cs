using ColossalFramework.UI;
using ImprovedTransportManager.ModShared;
using ImprovedTransportManager.Singleton;
using ImprovedTransportManager.UI;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Collections.Generic;
using UnityEngine;
using WriteEverywhere.Tools;

namespace ImprovedTransportManager
{
    public class ITMMainController : BaseController<ModInstance, ITMMainController>
    {
        public const ulong REALTIME_MOD_ID = 1420955187;

        private readonly List<GameObject> refGOs = new List<GameObject>();
        public ITMTransportLineStatusesManager StatisticsManager => ITMTransportLineStatusesManager.Instance;

        public IBridgeCD ConnectorCD { get; } = BridgeUtils.GetMostPrioritaryImplementation<IBridgeCD>();
        protected override void StartActions()
        {
            ToolsModifierControl.toolController.AddExtraToolToController<BuildingSelectorTool>();
            ToolsModifierControl.toolController.AddExtraToolToController<SegmentSelectorTool>();
            base.StartActions();
            refGOs.Add(ITMNearLinesWindow.Instance.gameObject);
            refGOs.Add(GameObjectUtils.CreateElement<LinesListingUI>(UIView.GetAView().gameObject.transform, "LinesListingUI").gameObject);
            refGOs.Add(GameObjectUtils.CreateElement<ITMCitySettingsGUI>(UIView.GetAView().gameObject.transform, "ITMCitySettingsGUI").gameObject);
            refGOs.Add(GameObjectUtils.CreateElement<ITMStatisticsGUI>(UIView.GetAView().gameObject.transform, "ITMStatisticsGUI").gameObject);
            refGOs.Add(GameObjectUtils.CreateElement<ITMLineStopsWindow>(UIView.GetAView().gameObject.transform, "ITMLineStopsWindow").gameObject);
            refGOs.Add(GameObjectUtils.CreateElement<ITMLineVehicleSelectionWindow>(UIView.GetAView().gameObject.transform, "ITMLineVehicleSelectionWindow").gameObject);
            refGOs.Add(GameObjectUtils.CreateElement<ITMLineDepotSelectionWindow>(UIView.GetAView().gameObject.transform, "ITMLineDepotSelectionWindow").gameObject);
            refGOs.Add(GameObjectUtils.CreateElement<ITMLineDataWindow>(UIView.GetAView().gameObject.transform, "ITMLineDataWindow").gameObject);

        }

        #region Tool Access
        public SegmentSelectorTool RoadSegmentToolInstance => ToolsModifierControl.toolController.GetComponent<SegmentSelectorTool>();
        public BuildingSelectorTool BuildingToolInstance => ToolsModifierControl.toolController.GetComponent<BuildingSelectorTool>();

        public ITMFacade Facade { get; } = new ITMFacade();
        #endregion

        public void OnDestroy()
        {
            foreach (GameObject go in refGOs)
            {
                Destroy(go);
            }
        }

        #region Real Time
        public readonly bool m_isRealTimeEnabled = PluginUtils.VerifyModEnabled(REALTIME_MOD_ID);



        #endregion
    }
}
