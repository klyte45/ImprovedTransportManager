using ColossalFramework;
using System;
using UnityEngine;

namespace ImprovedTransportManager.Utility
{
    public static class ITMLineUtils
    {
        public static void GetQuantityPassengerWaiting(ushort currentStop, out int residents, out int tourists, out int timeTilBored)
        {
            var residentsIn = 0;
            var touristsIn = 0;
            var timeTilBoredIn = 255;
            var cm = CitizenManager.instance;
            DoWithEachPassengerWaiting(currentStop, (citizen) =>
            {
                if ((cm.m_citizens.m_buffer[citizen].m_flags & Citizen.Flags.Tourist) != Citizen.Flags.None)
                {
                    touristsIn++;
                }
                else
                {
                    residentsIn++;
                }
                timeTilBoredIn = Math.Min(255 - cm.m_instances.m_buffer[citizen].m_waitCounter, timeTilBoredIn);
            });

            residents = residentsIn;
            tourists = touristsIn;
            timeTilBored = timeTilBoredIn;
        }


        public static void DoWithEachPassengerWaiting(ushort currentStop, Action<ushort> actionToDo)
        {
            ushort nextStop = TransportLine.GetNextStop(currentStop);
            CitizenManager cm = Singleton<CitizenManager>.instance;
            NetManager nm = Singleton<NetManager>.instance;
            Vector3 position = nm.m_nodes.m_buffer[currentStop].m_position;
            Vector3 position2 = nm.m_nodes.m_buffer[nextStop].m_position;
            nm.m_nodes.m_buffer[currentStop].m_maxWaitTime = 0;
            int minX = Mathf.Max((int)((position.x - 72) / 8f + 1080f), 0);
            int minZ = Mathf.Max((int)((position.z - 72) / 8f + 1080f), 0);
            int maxX = Mathf.Min((int)((position.x + 72) / 8f + 1080f), 2159);
            int maxZ = Mathf.Min((int)((position.z + 72) / 8f + 1080f), 2159);
            int zIterator = minZ;
            while (zIterator <= maxZ)
            {
                int xIterator = minX;
                while (xIterator <= maxX)
                {
                    ushort citizenIterator = cm.m_citizenGrid[(zIterator * 2160) + xIterator];
                    int loopCounter = 0;
                    while (citizenIterator != 0)
                    {
                        ushort nextGridInstance = cm.m_instances.m_buffer[citizenIterator].m_nextGridInstance;
                        if ((cm.m_instances.m_buffer[citizenIterator].m_flags & CitizenInstance.Flags.WaitingTransport) != CitizenInstance.Flags.None)
                        {
                            Vector3 a = cm.m_instances.m_buffer[citizenIterator].m_targetPos;
                            float distance = Vector3.SqrMagnitude(a - position);
                            if (distance < 8196f)
                            {
                                CitizenInfo info = cm.m_instances.m_buffer[citizenIterator].Info;
                                if (info.m_citizenAI.TransportArriveAtSource(citizenIterator, ref cm.m_instances.m_buffer[citizenIterator], position, position2))
                                {
                                    actionToDo(citizenIterator);
                                }
                            }
                        }
                        citizenIterator = nextGridInstance;
                        if (++loopCounter > 65536)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                    xIterator++;
                }
                zIterator++;
            }
        }
    }
}
