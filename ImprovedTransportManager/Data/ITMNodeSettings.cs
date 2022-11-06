using ImprovedTransportManager.Xml;
using Kwytto.Data;
using Kwytto.Utils;
using System.Linq;
using System.Xml.Serialization;
using static TransportInfo;

namespace ImprovedTransportManager.Data
{
    public class ITMNodeSettings : DataExtensionBase<ITMNodeSettings>
    {
        public override string SaveId => $"K45_ITM_ITMNodeSettings";

        [XmlElement("Nodes")]
        public SimpleNonSequentialList<ITMNodeSettingsXmlItem> Nodes { get; set; } = new SimpleNonSequentialList<ITMNodeSettingsXmlItem>();

        public override void AfterDeserialize(ITMNodeSettings loadedData)
        {
            var nodesToCheck = loadedData.Nodes.Keys.ToArray();
            var buff = NetManager.instance.m_nodes.m_buffer;
            foreach (var node in nodesToCheck)
            {
                if ((buff[node].m_flags & NetNode.Flags.Created) == 0 || buff[node].m_transportLine == 0)
                {
                    loadedData.Nodes.Remove(node);
                }
            }
        }

        private static readonly TransportType[] searchableTransportTypes = new[]
        {
            TransportType.Airplane,
            TransportType.Helicopter,
            TransportType.Ship,
            TransportType.CableCar,
            TransportType.Train,
            TransportType.Metro,
            TransportType.Monorail,
            TransportType.HotAirBalloon,
            TransportType.EvacuationBus,
        };

        public bool ForceBindToDistrict(ushort nodeId)
        {
            ref NetNode node = ref NetManager.instance.m_nodes.m_buffer[nodeId];
            var districtId = DistrictManager.instance.GetDistrict(node.m_position);
            if (districtId == 0)
            {
                return false;
            }
            SetUpValueForNodeNamingRef(nodeId, new InstanceID { District = districtId });
            return true;
        }
        public bool ForceBindToPark(ushort nodeId)
        {
            ref NetNode node = ref NetManager.instance.m_nodes.m_buffer[nodeId];
            var parkId = DistrictManager.instance.GetPark(node.m_position);
            if (parkId == 0)
            {
                return false;
            }
            SetUpValueForNodeNamingRef(nodeId, new InstanceID { Park = parkId });
            return true;
        }
        public void ForceBindToBuilding(ushort nodeId, ushort buildingId) => SetUpValueForNodeNamingRef(nodeId, new InstanceID { Building = buildingId });
        public void ForceBindToRoad(ushort nodeId, ushort segmentId) => SetUpValueForNodeNamingRef(nodeId, new InstanceID { NetSegment = segmentId });

        public string GetNodeName(ushort nodeId, bool forceRecalculate = false)
        {
            if (!forceRecalculate)
            {
                if (InstanceManager.instance.GetName(new InstanceID { NetNode = nodeId }) is string name && name.Length > 0)
                {
                    return name;
                }
                if (Nodes.TryGetValue(nodeId, out var nodeData) && nodeData.Id != default)
                {
                    var relId = nodeData.Id;
                    if (relId.Building > 0)
                    {
                        return BuildingManager.instance.GetBuildingName(relId.Building, default);
                    }
                    if (relId.NetSegment > 0)
                    {
                        return NetManager.instance.GetSegmentName(relId.NetSegment);
                    }
                    if (relId.District > 0)
                    {
                        return DistrictManager.instance.GetDistrictName(relId.District);
                    }
                    if (relId.Park > 0)
                    {
                        return DistrictManager.instance.GetParkName(relId.Park);
                    }
                }
            }
            ref NetNode node = ref NetManager.instance.m_nodes.m_buffer[nodeId];
            if (!(node.Info.m_netAI is TransportLineAI ai))
            {
                return null;
            }
            if (node.m_lane == 0)
            {
                return null;
            }
            ref NetLane lane = ref NetManager.instance.m_lanes.m_buffer[node.m_lane];
            if (lane.m_segment == 0)
            {
                return null;
            }
            ref NetSegment segment = ref NetManager.instance.m_segments.m_buffer[lane.m_segment];
            if ((segment.m_flags & NetSegment.Flags.Untouchable) != 0)
            {
                var buildingId = BuildingManager.instance.FindTransportBuilding(node.m_position, 16, TransportManager.instance.m_lines.m_buffer[node.m_transportLine].Info.m_transportType);
                if (buildingId != 0)
                {
                    SetUpValueForNodeNamingRef(nodeId, new InstanceID { Building = buildingId });
                    return BuildingManager.instance.GetBuildingName(buildingId, default);
                }
            }

            foreach (var tt in searchableTransportTypes)
            {
                var buildingId = BuildingManager.instance.FindTransportBuilding(node.m_position, 100, tt);
                if (buildingId != 0)
                {
                    SetUpValueForNodeNamingRef(nodeId, new InstanceID { Building = buildingId });
                    return BuildingManager.instance.GetBuildingName(buildingId, default);
                }
            }

            SetUpValueForNodeNamingRef(nodeId, new InstanceID { NetSegment = lane.m_segment });
            return NetManager.instance.GetSegmentName(lane.m_segment);
        }

        private void SetUpValueForNodeNamingRef(ushort nodeId, InstanceID instanceID)
        {
            if (!Nodes.ContainsKey(nodeId))
            {
                Nodes[nodeId] = new ITMNodeSettingsXmlItem
                {
                    Id = instanceID
                };
            }
            else
            {
                Nodes[nodeId].Id = instanceID;
            }
        }
    }


}
