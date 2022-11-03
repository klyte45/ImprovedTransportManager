using ImprovedTransportManager.Utility;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    internal class StationData
    {
        public ushort stopId;
        public string cachedName;
        public float distanceNextStop;
        public Vector3 position;
        public float tariffMultiplier;
        public float earningAllTime;
        public float earningLastWeek;
        public float earningCurrentWeek;
        public int residentsWaiting;
        public int touristsWaiting;
        public int timeUntilBored;

        private uint lastUpdateFrame;

        public static StationData FromStop(ushort currentStop)
        {
            var bufferN = NetManager.instance.m_nodes.m_buffer;
            var bufferS = NetManager.instance.m_segments.m_buffer;
            ref NetNode nd = ref bufferN[currentStop];
            var stop = new StationData
            {
                stopId = currentStop,
                cachedName = $"Stop #{currentStop}",
                position = nd.m_position,
                tariffMultiplier = nd.m_position.DistrictTariffMultiplierHere(),
            };
            for (int s = 0; s < 8; s++)
            {
                ushort segmentId = bufferN[currentStop].GetSegment(s);
                if (segmentId != 0 && bufferS[segmentId].m_startNode == currentStop)
                {
                    stop.distanceNextStop = bufferS[segmentId].m_averageLength;
                    break;
                }
            }
            return stop;
        }

        public void GetUpdated()
        {
            if (lastUpdateFrame + 23 > SimulationManager.instance.m_referenceFrameIndex)
            {
                return;
            }
            lastUpdateFrame = SimulationManager.instance.m_referenceFrameIndex;
            earningAllTime++;
            earningCurrentWeek++;
            earningLastWeek++;
            ITMLineUtils.GetQuantityPassengerWaiting(stopId, out residentsWaiting, out touristsWaiting, out timeUntilBored);
        }
    }


}

