using ImprovedTransportManager.Data;
using ImprovedTransportManager.Utility;
using System;
using System.Collections.Generic;

namespace ImprovedTransportManager.ModShared
{
    public class ITMFacade
    {
        public static ITMFacade Instance => ModInstance.Controller.Facade;

        public event Action<ushort> EventLineDestinationsChanged;

        public event Action<ushort> EventStopNameChanged;

        internal void RunEventLineDestinationsChanged(ushort lineId) => EventLineDestinationsChanged?.Invoke(lineId);
        internal void RunEventStopNameChanged(ushort stopId) => EventStopNameChanged?.Invoke(stopId);

        public string GetLineIdentifier(ushort lineId) => ITMLineUtils.GetEffectiveIdentifier(ref TransportManager.instance.m_lines.m_buffer[lineId], lineId);
        public ushort GetStopBuilding(ushort stopId) => ITMNodeSettings.Instance.GetBuildingReference(stopId);
        public string GetStopName(ushort stopId) => ITMNodeSettings.Instance.GetNodeName(stopId);
        public string GetVehicleIdentifier(ushort vehicleId) => ModInstance.Controller.ConnectorCD.GetVehicleIdentifier(vehicleId);
        public List<ushort> MapAllTerminals(ushort lineId)
        {
            ref TransportLine tl = ref TransportManager.instance.m_lines.m_buffer[lineId];
            var result = new List<ushort>();
            var idx = 0;
            for (ushort currStop = tl.GetStop(idx); currStop != 0; currStop = tl.GetStop(++idx))
            {
                if (tl.IsTerminal(currStop))
                {
                    result.Add(currStop);
                }
            }

            return result;
        }
    }
}
