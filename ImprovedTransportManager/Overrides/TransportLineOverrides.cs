﻿using ColossalFramework;
using HarmonyLib;
using ImprovedTransportManager.Data;
using ImprovedTransportManager.Utility;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ImprovedTransportManager.Overrides
{
    public class TransportLineOverrides : Redirector, IRedirectable
    {
        #region Hooking

        public void Awake()
        {

            #region Automation Hooks
            //MethodInfo doAutomation = typeof(TransportLineOverrides).GetMethod("DoAutomation", ReflectionUtils.allFlags);
            //MethodInfo preDoAutomation = typeof(TransportLineOverrides).GetMethod("PreDoAutomation", ReflectionUtils.allFlags);

            //LogUtils.DoLog("Loading AutoColor & AutoName Hook");
            //AddRedirect(typeof(TransportLine).GetMethod("AddStop", ReflectionUtils.allFlags), preDoAutomation, doAutomation);
            #endregion

            #region Budget Override Hooks

            MethodInfo TranspileSimulationStepLine = typeof(TransportLineOverrides).GetMethod("TranspileSimulationStepLine", ReflectionUtils.allFlags);
            MethodInfo TranspileSimulationStepAI = typeof(TransportLineOverrides).GetMethod("TranspileSimulationStepAI", ReflectionUtils.allFlags);
            LogUtils.DoLog("Loading SimulationStepPre Hook");
            AddRedirect(typeof(TransportLine).GetMethod("SimulationStep", ReflectionUtils.allFlags), null, null, TranspileSimulationStepLine);
            AddRedirect(typeof(TransportLineAI).GetMethod("SimulationStep", ReflectionUtils.allFlags, null, new Type[] { typeof(ushort), typeof(NetNode).MakeByRefType() }, null), null, null, TranspileSimulationStepAI);
            #endregion

            #region Ticket Override Hooks
            //MethodInfo GetTicketPriceTranspile = typeof(TransportLineOverrides).GetMethod("TicketPriceTranspilerEnterVehicle", ReflectionUtils.allFlags);

            //LogUtils.DoLog("Loading Ticket Override Hooks");
            //AddRedirect(typeof(HumanAI).GetMethod("EnterVehicle", ReflectionUtils.allFlags), null, null, GetTicketPriceTranspile);
            #endregion

            #region Bus Spawn Unbunching
            MethodInfo BusUnbuncher = typeof(TransportLineOverrides).GetMethod("BusUnbuncher", ReflectionUtils.allFlags);
            AddRedirect(typeof(TransportLine).GetMethod("AddVehicle", ReflectionUtils.allFlags), null, BusUnbuncher);
            #endregion


            List<Type> allVehicleAI = ReflectionUtils.GetSubtypesRecursive(typeof(VehicleAI), typeof(VehicleAI));

            #region Vehicle going back on terminal stop only
            MethodInfo PreventShouldReturnToSource = typeof(TransportLineOverrides).GetMethod("PreventShouldReturnToSource", ReflectionUtils.allFlags);
            LogUtils.DoLog("Loading PreventShouldReturnToSource Hook");
            AddRedirect(typeof(TramAI).GetMethod("ShouldReturnToSource", ReflectionUtils.allFlags), PreventShouldReturnToSource);
            AddRedirect(typeof(BusAI).GetMethod("ShouldReturnToSource", ReflectionUtils.allFlags), PreventShouldReturnToSource);
            AddRedirect(typeof(TrolleybusAI).GetMethod("ShouldReturnToSource", ReflectionUtils.allFlags), PreventShouldReturnToSource);

            MethodInfo TranspileSimulationStepLine_GoingBack = typeof(TransportLineOverrides).GetMethod("TranspileSimulationStepLine_GoingBack", ReflectionUtils.allFlags);
            LogUtils.DoLog("Loading TranspileSimulationStepLine_GoingBack Hook");
            AddRedirect(typeof(TransportLine).GetMethod("SimulationStep", ReflectionUtils.allFlags), null, null, TranspileSimulationStepLine_GoingBack);
            #endregion

            #region Express Bus Hooks
            MethodInfo TranspileCanLeaveStop = typeof(TransportLineOverrides).GetMethod("TranspileCanLeaveStop", ReflectionUtils.allFlags);
            LogUtils.DoLog("Loading CanLeaveStop Hook");
            foreach (Type ai in allVehicleAI)
            {
                var canLeaveMI = ai.GetMethod("CanLeave", ReflectionUtils.allFlags);
                if (canLeaveMI is null)
                {
                    LogUtils.DoLog($"Skipping: {ai} doesn't have CanLeave");
                    continue;
                }
                AddRedirect(canLeaveMI, null, null, TranspileCanLeaveStop);
            }
            #endregion
        }
        #endregion

        #region On Line Create

        //public static void PreDoAutomation(ushort lineID, ref TransportLine.Flags __state) => __state = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags;

        //public static void DoAutomation(ushort lineID, TransportLine.Flags __state)
        //{
        //    LogUtils.DoLog("OLD: " + __state + " ||| NEW: " + Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags);
        //    if (lineID > 0 && (__state & TransportLine.Flags.Complete) == TransportLine.Flags.None && (__state & TransportLine.Flags.Temporary) == TransportLine.Flags.None)
        //    {
        //        if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) != TransportLine.Flags.None
        //            && (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & (TransportLine.Flags.Temporary)) == TransportLine.Flags.None)
        //        {
        //            if (ITMBaseConfigXML.Instance.UseAutoColor)
        //            {
        //                ITMController.AutoColor(lineID);
        //            }
        //            if (ITMBaseConfigXML.Instance.UseAutoName)
        //            {
        //                ITMController.AutoName(lineID);
        //            }
        //            ITMController.Instance.LineCreationToolbox.IncrementNumber();
        //        }
        //    }
        //    if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None)
        //    {
        //        Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags &= ~TransportLine.Flags.CustomColor;
        //        ITMTransportLineExtension.Instance.SafeCleanEntry(lineID);
        //    }

        //}
        #endregion

        #region Budget Override
        private static readonly MethodInfo m_targetVehicles = typeof(TransportLine).GetMethod("CalculateTargetVehicleCount", RedirectorUtils.allFlags);
        private static readonly MethodInfo m_setActive = typeof(TransportLine).GetMethod("SetActive", RedirectorUtils.allFlags);
        private static readonly MethodInfo m_newTargetVehicles = typeof(TransportLineOverrides).GetMethod("NewCalculateTargetVehicleCount", RedirectorUtils.allFlags);
        private static readonly FieldInfo m_budgetField = typeof(TransportLine).GetField("m_budget", RedirectorUtils.allFlags);
        private static readonly MethodInfo m_getBudgetInt = typeof(ITMLineUtils).GetMethod("GetEffectiveBudgetInt", RedirectorUtils.allFlags);
        public static IEnumerable<CodeInstruction> TranspileSimulationStepLine(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            inst.InsertRange(0, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, m_getBudgetInt),
                        new CodeInstruction(OpCodes.Ldc_I4_0),
                        new CodeInstruction(OpCodes.Cgt),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, m_getBudgetInt),
                        new CodeInstruction(OpCodes.Ldc_I4_0),
                        new CodeInstruction(OpCodes.Cgt),
                        new CodeInstruction(OpCodes.Call,m_setActive ),
                    });
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Call && inst[i].operand == m_targetVehicles)
                {
                    inst[i - 1].opcode = OpCodes.Ldarg_1;
                    inst[i] = new CodeInstruction(OpCodes.Call, m_newTargetVehicles);
                    inst.RemoveRange(i - 6, 5);
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }
        public static IEnumerable<CodeInstruction> TranspileSimulationStepAI(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldfld && inst[i].operand == m_budgetField)
                {
                    inst[i] = new CodeInstruction(OpCodes.Call, m_getBudgetInt);
                    inst[i + 1] = new CodeInstruction(OpCodes.Stloc_S, 4);
                    inst.RemoveAt(i + 9);
                    inst.RemoveAt(i + 8);
                    inst.RemoveAt(i + 7);
                    inst.RemoveAt(i + 6);
                    inst.RemoveAt(i + 5);
                    inst.RemoveAt(i + 4);
                    inst.RemoveAt(i + 3);
                    inst.RemoveAt(i + 2);
                    inst.RemoveAt(i - 1);
                    inst.RemoveAt(i - 4);
                    inst.RemoveAt(i - 5);
                    inst.RemoveAt(i - 6);
                    break;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }
        public static int NewCalculateTargetVehicleCount(ushort lineId)
        {
            ref TransportLine t = ref TransportManager.instance.m_lines.m_buffer[lineId];
            float lineLength = t.m_totalLength;
            if (lineLength == 0f && t.m_stops != 0)
            {
                NetManager instance = NetManager.instance;
                ushort stops = t.m_stops;
                ushort num2 = stops;
                int num3 = 0;
                while (num2 != 0)
                {
                    ushort num4 = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        ushort segment = instance.m_nodes.m_buffer[num2].GetSegment(i);
                        if (segment != 0 && instance.m_segments.m_buffer[segment].m_startNode == num2)
                        {
                            lineLength += instance.m_segments.m_buffer[segment].m_averageLength;
                            num4 = instance.m_segments.m_buffer[segment].m_endNode;
                            break;
                        }
                    }
                    num2 = num4;
                    if (num2 == stops)
                    {
                        break;
                    }
                    if (++num3 >= 32768)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
                t.m_totalLength = lineLength;
            }
            return ITMLineUtils.ProjectTargetVehicleCount(t.Info, lineLength, ITMLineUtils.GetEffectiveBudget(lineId));
        }

        #endregion

        #region Ticket Override
        //private static readonly MethodInfo m_getTicketPriceForPrefix = typeof(TransportLineOverrides).GetMethod("GetTicketPriceForVehicle", RedirectorUtils.allFlags);
        //private static readonly MethodInfo m_getTicketPriceDefault = typeof(VehicleAI).GetMethod("GetTicketPrice", RedirectorUtils.allFlags);

        //public static IEnumerable<CodeInstruction> TicketPriceTranspilerEnterVehicle(IEnumerable<CodeInstruction> instructions)
        //{
        //    var inst = new List<CodeInstruction>(instructions);

        //    for (int i = 0; i < inst.Count; i++)
        //    {
        //        if (inst[i].opcode == OpCodes.Callvirt && inst[i].operand == m_getTicketPriceDefault)
        //        {
        //            inst[i] = new CodeInstruction(OpCodes.Call, m_getTicketPriceForPrefix);
        //            inst.RemoveAt(i + 3);
        //            inst.RemoveAt(i + 2);
        //            break;
        //        }
        //    }
        //    LogUtils.PrintMethodIL(inst);
        //    return inst;
        //}

        //private static int GetTicketPriceForVehicle(VehicleAI ai, ushort vehicleID, ref Vehicle vehicleData)
        //{
        //    var def = TransportSystemDefinition.From(vehicleData.Info);

        //    if (def == default)
        //    {
        //        LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}):DEFAULT TSD FOR {ai}");
        //        return ai.GetTicketPrice(vehicleID, ref vehicleData);
        //    }

        //    DistrictManager instance = Singleton<DistrictManager>.instance;
        //    byte district = instance.GetDistrict(vehicleData.m_targetPos3);
        //    DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
        //    DistrictPolicies.Event @event = instance.m_districts.m_buffer[district].m_eventPolicies & Singleton<EventManager>.instance.GetEventPolicyMask();
        //    float multiplier;
        //    if (vehicleData.Info.m_class.m_subService == ItemClass.SubService.PublicTransportTours)
        //    {
        //        multiplier = 1;
        //    }
        //    else
        //    {
        //        if ((servicePolicies & DistrictPolicies.Services.FreeTransport) != DistrictPolicies.Services.None)
        //        {
        //            LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}): FreeTransport at district!");
        //            return 0;
        //        }
        //        if ((@event & DistrictPolicies.Event.ComeOneComeAll) != DistrictPolicies.Event.None)
        //        {
        //            LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}): ComeOneComeAll at district!");
        //            return 0;
        //        }
        //        if ((servicePolicies & DistrictPolicies.Services.HighTicketPrices) != DistrictPolicies.Services.None)
        //        {
        //            LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}): HighTicketPrices at district!");
        //            instance.m_districts.m_buffer[district].m_servicePoliciesEffect = (instance.m_districts.m_buffer[district].m_servicePoliciesEffect | DistrictPolicies.Services.HighTicketPrices);
        //            multiplier = 5f / 4f;
        //        }
        //        else
        //        {
        //            multiplier = 1;
        //        }
        //    }
        //    uint ticketPriceDefault = ITMLineUtils.GetTicketPriceForLine(def, vehicleData.m_transportLine).First.Value;
        //    LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}): multiplier = {multiplier}, ticketPriceDefault = {ticketPriceDefault}");

        //    return (int)(multiplier * ticketPriceDefault);

        //}


        #endregion


        #region Bus Spawn Unbunching
        public static void BusUnbuncher(ushort vehicleID, ref Vehicle data, bool findTargetStop)
        {
            if (findTargetStop && (data.Info.GetAI() is BusAI || data.Info.GetAI() is TramAI || data.Info.GetAI() is TrolleybusAI) && data.m_transportLine > 0)
            {
                TransportLine[] buff = TransportManager.instance.m_lines.m_buffer;
                var lineId = data.m_transportLine;
                if (ITMTransportLineSettings.Instance.SafeGetLine(lineId).m_requireLineStartTerminal)
                {
                    var terminalMarkedStops = new ushort[buff[lineId].CountStops(lineId) - 1].Select((x, i) =>
                    {
                        var stopId = buff[lineId].GetStop(i + 1);
                        return Tuple.New(stopId, buff[lineId].IsTerminal(stopId));
                    }).Where(x => x.Second).Select(x => x.First);
                    var terminalStops = new ushort[] { buff[lineId].m_stops }.Union(terminalMarkedStops).GroupBy(x => x).First().ToList();
                    data.m_targetBuilding = terminalStops[SimulationManager.instance.m_randomizer.Int32((uint)terminalStops.Count)];
                }
                else
                {
                    data.m_targetBuilding = buff[lineId].GetStop(SimulationManager.instance.m_randomizer.Int32((uint)buff[lineId].CountStops(lineId)));
                }
            }
        }
        #endregion


        #region Express Bus
        public static bool PreCanLeaveStop(ref TransportLine tl, ushort nextStop, int waitTime)
        {
            if (tl.m_vehicles == 0 || (tl.m_flags & TransportLine.Flags.Created) == 0)
            {
                return tl.CanLeaveStop(nextStop, waitTime);
            }
            ref Vehicle firstVehicle = ref VehicleManager.instance.m_vehicles.m_buffer[tl.m_vehicles];
            var info = firstVehicle.Info;

            var validType = (info.m_vehicleType == VehicleInfo.VehicleType.Car && ITMCitySettings.Instance.expressBuses)
                || (info.m_vehicleType == VehicleInfo.VehicleType.Tram && ITMCitySettings.Instance.expressTrams)
                || (info.m_vehicleType == VehicleInfo.VehicleType.Trolleybus && ITMCitySettings.Instance.expressTrolleybus);
            var currentStop = TransportLine.GetPrevStop(nextStop);
            return (validType && (ITMCitySettings.Instance.disableUnbunchingTerminals || !tl.IsTerminal(currentStop) || ITMTransportLineSettings.Instance.SafeGetLine(firstVehicle.m_transportLine).m_ignoreTerminalsMandatoryStop)) || tl.CanLeaveStop(nextStop, waitTime);
        }


        public static IEnumerable<CodeInstruction> TranspileCanLeaveStop(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            MethodInfo CanLeaveStop = typeof(TransportLine).GetMethod("CanLeaveStop", RedirectorUtils.allFlags);
            var instrList = new List<CodeInstruction>(instructions);
            var preCanLeave = typeof(TransportLineOverrides).GetMethod("PreCanLeaveStop", RedirectorUtils.allFlags);
            for (int i = 0; i < instrList.Count; i++)
            {
                if (instrList[i].operand == CanLeaveStop)
                {
                    instrList[i].opcode = OpCodes.Call;
                    instrList[i].operand = preCanLeave;
                }
            }
            LogUtils.PrintMethodIL(instrList);
            return instrList;
        }
        #endregion

        #region Vehicle going back on terminal stop only or if empty
        public static bool PreventShouldReturnToSource(VehicleAI __instance, ushort vehicleID, ref Vehicle data)
        {
            return false;
        }
        public static IEnumerable<CodeInstruction> TranspileSimulationStepLine_GoingBack(IEnumerable<CodeInstruction> instructions)

        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 1; i < inst.Count - 3; i++)
            {
                if (
                     inst[i].opcode == OpCodes.Ldloc_S
                    && inst[i].operand is LocalBuilder lb1
                    && lb1.LocalIndex == 41
                    && inst[i + 1].opcode == OpCodes.Brfalse
             )
                {
                    LogUtils.DoLog($"Found @ line {i}");
                    inst.InsertRange(i + 2, new[]
                    {
                        inst[i],
                        new CodeInstruction(OpCodes.Call, typeof(TransportLineOverrides).GetMethod("IsEmpty",ReflectionUtils.allFlags)),
                        inst[i+1]
                    });
                    break;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        private static bool IsEmpty(ushort vehicleId)
        {
            ref Vehicle data = ref VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
            data.Info.m_vehicleAI.GetBufferStatus(vehicleId, ref data, out _, out var passengers, out _);
            return passengers == 0;
        }
        #endregion
    }
}
