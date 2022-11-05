using ColossalFramework;
using ColossalFramework.UI;
using ImprovedTransportManager.TransportSystems;
using Kwytto.Utils;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ImprovedTransportManager.Data;

namespace ImprovedTransportManager.Utility
{
    public static class ITMAssetUtils
    {
        public static HashSet<VehicleInfo> GetAllLoadedForType(this List<string> assetNames, TransportSystemType targetTst)
        {
            var result = new HashSet<VehicleInfo>();
            assetNames.Select(x => VehiclesIndexes.instance.PrefabsData.TryGetValue(x, out var val) ? val.Info as VehicleInfo : null).Where(x => x != null).ForEach(x => result.Add(x));
            return result;
        }

        private static int GetTotalUnitGroups(uint unitID)
        {
            int num = 0;
            while (unitID != 0u)
            {
                CitizenUnit citizenUnit = Singleton<CitizenManager>.instance.m_units.m_buffer[(int)((UIntPtr)unitID)];
                unitID = citizenUnit.m_nextUnit;
                num++;
            }
            return num;
        }

        public static IEnumerator UpdateCapacityUnitsFromTSD()
        {
            int count = 0;
            Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
            int i = 0;
            TransportSystemType tsd;
            while (i < (long)((ulong)vehicles.m_size))
            {
                if ((vehicles.m_buffer[i].m_flags & Vehicle.Flags.Spawned) == Vehicle.Flags.Spawned && (tsd = vehicles.m_buffer[i].Info.ToTST()) != default && ITMAssetSettings.Instance.IsCustomCapacity(vehicles.m_buffer[i].Info.name))
                {
                    int capacity = ITMAssetSettings.Instance.GetCustomCapacity(vehicles.m_buffer[i].Info.name);
                    if (capacity != -1)
                    {
                        CitizenUnit[] units = Singleton<CitizenManager>.instance.m_units.m_buffer;
                        uint unit = vehicles.m_buffer[i].m_citizenUnits;
                        int currentUnitCount = GetTotalUnitGroups(unit);
                        int newUnitCount = Mathf.CeilToInt(capacity / 5f);
                        if (newUnitCount < currentUnitCount)
                        {
                            uint j = unit;
                            for (int k = 1; k < newUnitCount; k++)
                            {
                                j = units[(int)((UIntPtr)j)].m_nextUnit;
                            }
                            Singleton<CitizenManager>.instance.ReleaseUnits(units[(int)((UIntPtr)j)].m_nextUnit);
                            units[(int)((UIntPtr)j)].m_nextUnit = 0u;
                            count++;
                        }
                        else if (newUnitCount > currentUnitCount)
                        {
                            uint l = unit;
                            while (units[(int)((UIntPtr)l)].m_nextUnit != 0u)
                            {
                                l = units[(int)((UIntPtr)l)].m_nextUnit;
                            }
                            int newCapacity = capacity - currentUnitCount * 5;
                            if (!Singleton<CitizenManager>.instance.CreateUnits(out units[l].m_nextUnit, ref Singleton<SimulationManager>.instance.m_randomizer, 0, (ushort)i, 0, 0, 0, newCapacity, 0))
                            {
                                LogUtils.DoErrorLog("FAILED CREATING UNITS!!!!");
                            }
                            count++;
                        }
                    }
                }
                if (i % 256 == 255)
                {
                    yield return i % 256;
                }
                i++;
            }
            yield break;
        }
    }
}
