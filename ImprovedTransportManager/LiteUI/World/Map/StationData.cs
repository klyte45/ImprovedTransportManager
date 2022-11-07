using ColossalFramework;
using ImprovedTransportManager.Data;
using ImprovedTransportManager.ModShared;
using ImprovedTransportManager.Singleton;
using ImprovedTransportManager.Utility;
using System;
using System.Collections;
using UnityEngine;

namespace ImprovedTransportManager.UI
{
    internal class StationData
    {
        public ushort stopId;
        public string cachedName;
        public float distanceNextStop;
        public Vector3 position;
        public float fareMultiplier;
        private long m_earningAllTime;
        private long m_earningLastWeek;
        private long m_earningCurrentWeek;
        public int residentsWaiting;
        public int touristsWaiting;
        public int timeUntilBored;
        public bool isTerminal;
        private ushort lineId;

        public float EarningAllTime => m_earningAllTime * .01f;
        public float EarningLastWeek => m_earningLastWeek * .01f;
        public float EarningCurrentWeek => m_earningCurrentWeek * .01f;

        private uint lastUpdateFrame;
        private uint lastUpdateTick;

        public static StationData FromStop(ushort currentStop)
        {
            var bufferN = NetManager.instance.m_nodes.m_buffer;
            var bufferS = NetManager.instance.m_segments.m_buffer;
            ref NetNode nd = ref bufferN[currentStop];
            var stop = new StationData
            {
                stopId = currentStop,
                position = nd.m_position,
                fareMultiplier = nd.m_position.DistrictFareMultiplierHere(),
                lineId = nd.m_transportLine
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

            if (lastUpdateTick + 60 < SimulationManager.instance.m_currentTickIndex)
            {
                cachedName = ITMLineUtils.GetEffectiveStopName(stopId);
                lastUpdateTick = SimulationManager.instance.m_currentTickIndex;
                fareMultiplier = NetManager.instance.m_nodes.m_buffer[stopId].m_position.DistrictFareMultiplierHere();
                isTerminal = ITMLineUtils.IsTerminal(stopId, lineId);
            }
            if (lastUpdateFrame + 23 < SimulationManager.instance.m_referenceFrameIndex)
            {
                lastUpdateFrame = SimulationManager.instance.m_referenceFrameIndex;
                ITMTransportLineStatusesManager.Instance.GetLastWeekStopIncome(stopId, out m_earningLastWeek);
                ITMTransportLineStatusesManager.Instance.GetStopIncome(stopId, out m_earningAllTime);
                ITMTransportLineStatusesManager.Instance.GetCurrentStopIncome(stopId, out m_earningCurrentWeek);
                ITMLineUtils.GetQuantityPassengerWaiting(stopId, out residentsWaiting, out touristsWaiting, out timeUntilBored);
            }
        }

        internal void SetAsFirst()
        {
            TransportManager.instance.m_lines.m_buffer[lineId].m_stops = stopId;
            ITMFacade.Instance.RunEventLineDestinationsChanged(lineId);
        }

        internal void UnsetTerminal()
        {
            ITMTransportLineSettings.Instance.m_terminalStops.Remove(stopId);
            ITMFacade.Instance.RunEventLineDestinationsChanged(lineId);
        }

        internal void SetTerminal()
        {
            ITMTransportLineSettings.Instance.m_terminalStops.Add(stopId);
            ITMFacade.Instance.RunEventLineDestinationsChanged(lineId);
        }

        internal void RemoveStop(Action callback)
        {
            Singleton<SimulationManager>.instance.AddAction(RemoveStopCoroutine(callback));
        }

        private IEnumerator RemoveStopCoroutine(Action callback)
        {
            TransportManager instance = Singleton<TransportManager>.instance;
            int num = instance.m_lines.m_buffer[lineId].CountStops(lineId);
            if (num <= 2)
            {
                instance.ReleaseLine(lineId);
            }
            else if (num >= 2)
            {
                TransportLine[] buff = instance.m_lines.m_buffer;
                ushort stopScan = buff[lineId].m_stops;
                int i = 0;
                while (stopScan != stopId && stopScan != 0)
                {
                    stopScan = TransportLine.GetNextStop(stopScan);
                    i++;
                }
                if (stopScan == 0)
                {
                    yield break;
                }
                buff[lineId].RemoveStop(lineId, i);
            }
            callback?.Invoke();
            ITMFacade.Instance.RunEventLineDestinationsChanged(lineId);
            yield return 0;
        }
    }


}

