using ColossalFramework;
using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.TransportSystems;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public static void DoSoftDespawn(this ref Vehicle vehicleData, ushort vehicleID)
        {
            var targetBuilding = vehicleData.m_targetBuilding;
            TransportManager.instance.m_lines.m_buffer[vehicleData.m_transportLine].RemoveVehicle(vehicleID, ref vehicleData);
            vehicleData.m_transportLine = 0;
            vehicleData.m_targetBuilding = targetBuilding;
        }

        public static string GetEffectiveIdentifier(this ref TransportLine tl, ushort lineId)
            => ITMTransportLineSettings.Instance.SafeGetLine(lineId).CustomCode.TrimToNull() ?? $"{tl.m_lineNumber}";
        public static void DoWithEachStop(ushort lineId, Action<ushort, int> action)
        {
            ref TransportLine tl = ref TransportManager.instance.m_lines.m_buffer[lineId];
            ushort currentStop = tl.GetStop(0);
            for (int i = 0; currentStop != 0 && i < 65536; currentStop = tl.GetStop(++i))
            {
                action(currentStop, i);
            }
        }

        public static string GetEffectiveStopName(ushort stopId)
            => ITMNodeSettings.Instance.GetNodeName(stopId) ?? $"Stop #{stopId}";

        public static void DoWithEachVehicle(ushort lineId, Action<ushort, int> action)
        {
            ref TransportLine tl = ref TransportManager.instance.m_lines.m_buffer[lineId];
            ushort currentVehicle = tl.GetVehicle(0);
            for (int i = 0; currentVehicle != 0 && i < 65536; currentVehicle = tl.GetVehicle(++i))
            {
                action(currentVehicle, i);
            }
        }
        public static string GetEffectiveVehicleName(ushort vehicleId)
            => ModInstance.Controller.ConnectorCD.GetVehicleIdentifier(vehicleId);

        internal static bool IsTerminal(ushort stopId, ushort lineId)
            => ITMTransportLineSettings.Instance.m_terminalStops.Contains(stopId)
            || (lineId > 0 && TransportManager.instance.m_lines.m_buffer[lineId].m_stops == stopId);
        internal static bool IsTerminal(this ref TransportLine tl, ushort stopId)
            => ITMTransportLineSettings.Instance.m_terminalStops.Contains(stopId)
            || (tl.m_stops == stopId);


        public static bool GetNearLines(Vector3 pos, float maxDistance, HashSet<ushort> linesFound)
        {
            float extendedMaxDistance = maxDistance * 1.3f;
            int num = Mathf.Max((int)(((pos.x - extendedMaxDistance) / 64f) + 135f), 0);
            int num2 = Mathf.Max((int)(((pos.z - extendedMaxDistance) / 64f) + 135f), 0);
            int num3 = Mathf.Min((int)(((pos.x + extendedMaxDistance) / 64f) + 135f), 269);
            int num4 = Mathf.Min((int)(((pos.z + extendedMaxDistance) / 64f) + 135f), 269);
            bool noneFound = true;
            NetManager nm = Singleton<NetManager>.instance;
            TransportManager tm = Singleton<TransportManager>.instance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num6 = nm.m_nodeGrid[(i * 270) + j];
                    int num7 = 0;
                    while (num6 != 0)
                    {
                        NetInfo info = nm.m_nodes.m_buffer[num6].Info;
                        if ((info.m_class.m_service == ItemClass.Service.PublicTransport))
                        {
                            ushort transportLine = nm.m_nodes.m_buffer[num6].m_transportLine;
                            var tsd = TransportSystemTypeExtensions.FromLineId(transportLine == 0 ? num6 : transportLine, transportLine == 0);
                            if (transportLine != 0 && tsd != default
                                //&& tsd.GetConfig().ShowInLinearMap
                                )
                            {
                                TransportInfo info2 = tm.m_lines.m_buffer[transportLine].Info;
                                if (!linesFound.Contains(transportLine) && (tm.m_lines.m_buffer[transportLine].m_flags & TransportLine.Flags.Temporary) == TransportLine.Flags.None)
                                {
                                    float num8 = Vector3.SqrMagnitude(pos - nm.m_nodes.m_buffer[num6].m_position);
                                    if (num8 < maxDistance * maxDistance || (info2.m_transportType == TransportInfo.TransportType.Ship && num8 < extendedMaxDistance * extendedMaxDistance))
                                    {
                                        linesFound.Add(transportLine);
                                        GetNearLines(nm.m_nodes.m_buffer[num6].m_position, maxDistance, linesFound);
                                        noneFound = false;
                                    }
                                }
                            }
                        }

                        num6 = nm.m_nodes.m_buffer[num6].m_nextGridNode;
                        if (++num7 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return noneFound;
        }

        internal static void DoAutoname(ushort currentLine)
        {
            var stopsInName = new List<ushort>();
            ref TransportLine tl = ref TransportManager.instance.m_lines.m_buffer[currentLine];
            if ((tl.m_flags & TransportLine.Flags.Complete) == 0)
            {
                return;
            }

            var currStop = tl.GetStop(0);
            for (var idx = 0; currStop != 0; currStop = tl.GetStop(++idx))
            {
                if (tl.IsTerminal(currStop))
                {
                    stopsInName.Add(currStop);
                }
            }
            if (stopsInName.Count == 0)
            {
                return;
            }

            string lineName = stopsInName.Count == 1
            ? $"[{tl.GetEffectiveIdentifier(currentLine)}] {string.Format(Str.itm_autoName_circularTemplate, ITMNodeSettings.Instance.GetNodeName(stopsInName[0]))}"
            : $"[{tl.GetEffectiveIdentifier(currentLine)}] {string.Join(" - ", stopsInName.Select(x => ITMNodeSettings.Instance.GetNodeName(x)).ToArray())}";
            SimulationManager.instance.AddAction(TransportManager.instance.SetLineName(currentLine, lineName));
        }

        #region Budget
        public static float ReferenceTimer => (!Singleton<SimulationManager>.instance.m_enableDayNight) ? (float)Singleton<SimulationManager>.instance.m_currentGameTime.TimeOfDay.TotalHours % 24 : Singleton<SimulationManager>.instance.m_currentDayTimeHour;
        public static DayOfWeek ReferenceWeekday => Singleton<SimulationManager>.instance.m_currentGameTime.DayOfWeek;

        //RESULT, PREV VAL, NEXT VAL, LERP, ISGROUP
        public static Tuple<float, ushort, ushort, float, bool> GetBudgetMultiplierLineWithIndexes(ushort lineId, DayOfWeek refWeek, float refTime)
        {
            float lerpPos;
            DayOfWeek selfRefDayOfWeek = refWeek;
            uint selfHourRef = (uint)Mathf.FloorToInt(refTime);
            uint otherHourRef;
            DayOfWeek otherRefDayOfWeek;
            if (refTime % 1 < 0.5f)
            {
                otherHourRef = (selfHourRef + 23) % 24;
                lerpPos = .5f - (refTime % 1);
                otherRefDayOfWeek = otherHourRef > selfHourRef ? (DayOfWeek)(((int)selfRefDayOfWeek + 6) % 7) : selfRefDayOfWeek;
            }
            else
            {
                otherHourRef = (selfHourRef + 1) % 24;
                lerpPos = (refTime % 1) - 0.5f;
                otherRefDayOfWeek = otherHourRef < selfHourRef ? (DayOfWeek)(((int)selfRefDayOfWeek + 1) % 7) : selfRefDayOfWeek;
            }

            var budgetConfigIn = ITMTransportLineSettings.Instance.GetWeekdayHourValue(lineId, selfRefDayOfWeek, selfHourRef);
            if (budgetConfigIn == ushort.MaxValue)
            {
                var budget = TransportManager.instance.m_lines.m_buffer[lineId].m_budget;
                return Tuple.New((float)budget, budget, budget, 0f, false);
            }
            var budgetConfigOut = ITMTransportLineSettings.Instance.GetWeekdayHourValue(lineId, otherRefDayOfWeek, otherHourRef);

            var effectiveBudgetNow = Mathf.Lerp(budgetConfigIn, budgetConfigOut, lerpPos);

            return Tuple.New(effectiveBudgetNow, budgetConfigIn, budgetConfigOut, lerpPos, true);
        }

        public static float GetEffectiveBudget(ushort transportLine) => GetEffectiveBudgetInt(transportLine) / 100f;

        public static int GetEffectiveBudgetInt(ushort transportLine)
        {
            ref TransportLine tl = ref Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine];
            TransportInfo info = tl.Info;
            var lineBudget = GetBudgetMultiplierLineWithIndexes(transportLine, ReferenceWeekday, ReferenceTimer);
            int budgetClass = lineBudget.Fifth ? 100 : Singleton<EconomyManager>.instance.GetBudget(info.m_class);

            var result = (int)(budgetClass * lineBudget.First);
            var lineCfg = ITMTransportLineSettings.Instance.SafeGetLine(transportLine);
            if (result == 0 != lineCfg.IsZeroed)
            {
                lineCfg.IsZeroed = result == 0;
                if (lineCfg.IsZeroed)
                {
                    SimulationManager.instance.StartCoroutine(MakePassengersBored(transportLine, SimulationManager.instance.m_referenceFrameIndex));
                }
            }
            return result;
        }

        private static IEnumerator MakePassengersBored(ushort transportLine, uint simulationFrameStart)
        {
            var lineCfg = ITMTransportLineSettings.Instance.SafeGetLine(transportLine);
            int citizensCount = 0;
            do
            {
                do
                {
                    yield return 0;
                } while (SimulationManager.instance.m_referenceFrameIndex - simulationFrameStart < 5);
                if (!lineCfg.IsZeroed)
                {
                    yield break;
                }
                ushort stop = Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine].m_stops;
                citizensCount = 0;
                do
                {
                    var citizensToBored = new List<ushort>();
                    DoWithEachPassengerWaiting(stop, (citizenId) => citizensToBored.Add(citizenId));
                    foreach (var citizenId in citizensToBored)
                    {
                        CitizenManager.instance.m_instances.m_buffer[citizenId].m_waitCounter = byte.MaxValue;
                    }
                    citizensCount += citizensToBored.Count;
                    stop = TransportLine.GetNextStop(stop);
                } while (stop != Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine].m_stops);
                simulationFrameStart = SimulationManager.instance.m_referenceFrameIndex;
            } while (citizensCount > 0 || !lineCfg.IsZeroed);
        }

        public static int ProjectTargetVehicleCount(TransportInfo info, float lineLength, float budget) => Mathf.CeilToInt(budget * lineLength / info.m_defaultVehicleDistance * .01f);
        public static float ProjectBudgetPercentagePerVehicle(TransportInfo info, float lineLength) => info.m_defaultVehicleDistance / lineLength;
        #endregion
    }
}
