using ColossalFramework.UI;
using ICities;
using ImprovedTransportManager.TransportSystems;
using ImprovedTransportManager.Utility;
using ImprovedTransportManager.Xml;
using Kwytto.Data;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using static ImprovedTransportManager.TransportSystems.TransportSystemTypeExtensions;

namespace ImprovedTransportManager.Data
{
    [XmlRoot("ITMAssetSettings")]
    public class ITMAssetSettings : DataExtensionBase<ITMAssetSettings>
    {
        [XmlElement("VehicleConfigurations")]
        public SimpleXmlDictionary<string, ITMVehicleAssetXml> Vehicles { get; set; } = new SimpleXmlDictionary<string, ITMVehicleAssetXml>();
        internal void SafeCleanEntry(string vehicleName) => Vehicles[vehicleName] = new ITMVehicleAssetXml();
        public ITMVehicleAssetXml SafeGetVehicle(string vehicleName)
        {
            if (!Vehicles.ContainsKey(vehicleName))
            {
                Vehicles[vehicleName] = new ITMVehicleAssetXml
                {
                };
            }
            return Vehicles[vehicleName];
        }

        internal const string DEFAULTS_FILENAME = "AssetSettingsDefault.xml";
        public static string DefaultsFilePath => Path.Combine(ModInstance.Instance.ModRootFolder, DEFAULTS_FILENAME);

        public override ITMAssetSettings LoadDefaults(ISerializableData serializableData)
        {
            if (File.Exists(DefaultsFilePath))
            {
                try
                {
                    return XmlUtils.DefaultXmlDeserialize<ITMAssetSettings>(File.ReadAllText(DefaultsFilePath));
                }
                catch { }
            }
            return null;
        }
        internal static void ExportAsDefault()
        {
            KFileUtils.EnsureFolderCreation(ModInstance.Instance.ModRootFolder);
            File.WriteAllText(DefaultsFilePath, XmlUtils.DefaultXmlSerialize(Instance));
        }

        internal static void ImportFromDefault()
        {
            if (Instance.LoadDefaults(null) is ITMAssetSettings s)
            {
                Instance = s;
            }
            Instance.AfterDeserialize(Instance);
        }

        public override void AfterDeserialize(ITMAssetSettings loadedData)
        {
            VehiclesIndexes.instance.PrefabsData
                .Select(x => x.Value.Info is VehicleInfo info && info.m_placementStyle != ItemClass.Placement.Procedural ? Tuple.New(info.ToTST(), info) : null)
                .Where(x => x != null && x.First != default && x.First.IsCitywide(x.Second.m_class.m_level))
                .GroupBy(x => x.First)
                .ForEach(x => loadedData.UpdateAssetCapacity(x.First().Second));
            RunUpdateCapacityCoroutine();
        }

        private void UpdateAssetCapacity(VehicleInfo vehicle)
        {
            if (IsCustomCapacity(vehicle.name))
            {
                SetVehicleCapacity(vehicle.m_vehicleAI, GetCustomCapacity(vehicle.name));
                foreach (var trailer in vehicle.m_trailers)
                {
                    if (IsCustomCapacity(trailer.m_info.name))
                    {
                        SetVehicleCapacity(trailer.m_info.m_vehicleAI, GetCustomCapacity(trailer.m_info.name));
                    }
                }
            }
        }
        public int GetCustomCapacity(string name)
        {
            int capacity = SafeGetVehicle(name).OverwrittenCapacity;
            return capacity <= 0 ? m_defaultCapacities[name] : capacity;
        }

        public void SetVehicleCapacity(VehicleInfo vehicle, int newCapacity)
        {
            if (vehicle != null && !VehicleUtils.IsTrailer(vehicle))
            {
                Dictionary<string, MutableTuple<float, int>> assetsCapacitiesPercentagePerTrailer = GetCapacityRelative(vehicle);
                int capacityUsed = 0;
                foreach (KeyValuePair<string, MutableTuple<float, int>> entry in assetsCapacitiesPercentagePerTrailer)
                {
                    SafeGetVehicle(entry.Key).OverwrittenCapacity = Mathf.RoundToInt(newCapacity <= 0 ? -1f : entry.Value.First * newCapacity);
                    capacityUsed += SafeGetVehicle(entry.Key).OverwrittenCapacity * entry.Value.Second;
                }
                if (newCapacity > 0 && capacityUsed != newCapacity)
                {
                    SafeGetVehicle(assetsCapacitiesPercentagePerTrailer.Keys.ElementAt(0)).OverwrittenCapacity += (newCapacity - capacityUsed) / assetsCapacitiesPercentagePerTrailer[assetsCapacitiesPercentagePerTrailer.Keys.ElementAt(0)].Second;
                }
                foreach (string entry in assetsCapacitiesPercentagePerTrailer.Keys)
                {
                    VehicleAI vai = PrefabCollection<VehicleInfo>.FindLoaded(entry).m_vehicleAI;
                    SetVehicleCapacity(vai, SafeGetVehicle(entry).OverwrittenCapacity);
                }
                RunUpdateCapacityCoroutine();
            }
        }

        private static void RunUpdateCapacityCoroutine() => SimulationManager.instance.StartCoroutine(ITMAssetUtils.UpdateCapacityUnitsFromTSD());

        public bool IsCustomCapacity(string name) => Vehicles.ContainsKey(name);
        private void SetVehicleCapacity<AI>(AI ai, int newCapacity) where AI : VehicleAI
        {
            int defaultCapacity = UpdateDefaultCapacity(ai);
            if (newCapacity <= 0)
            {
                newCapacity = defaultCapacity;
            }
            VehicleUtils.GetVehicleCapacityField(ai).SetValue(ai, newCapacity);
            if (ModInstance.DebugMode)
            {
                LogUtils.DoLog($"SET VEHICLE CAPACITY {newCapacity} at {ai.m_info.name}");
            }
        }


        public Dictionary<string, MutableTuple<float, int>> GetCapacityRelative(VehicleInfo info)
        {
            var relativeParts = new Dictionary<string, MutableTuple<float, int>>();
            GetCapacityRelative(info, info.m_vehicleAI, ref relativeParts, out _);
            return relativeParts;
        }

        private void GetCapacityRelative<AI>(VehicleInfo info, AI ai, ref Dictionary<string, MutableTuple<float, int>> relativeParts, out int totalCapacity, bool noLoop = false) where AI : VehicleAI
        {
            if (info == null)
            {
                totalCapacity = 0;
                return;
            }

            totalCapacity = UpdateDefaultCapacity(ai);
            if (relativeParts.ContainsKey(info.name))
            {
                relativeParts[info.name].Second++;
            }
            else
            {
                relativeParts[info.name] = MutableTuple.New((float)totalCapacity, 1);
            }
            if (!noLoop)
            {
                try
                {
                    foreach (VehicleInfo.VehicleTrailer trailer in info.m_trailers)
                    {
                        if (trailer.m_info != null)
                        {
                            GetCapacityRelative(trailer.m_info, trailer.m_info.m_vehicleAI, ref relativeParts, out int capacity, true);
                            totalCapacity += capacity;
                        }
                    }

                    for (int i = 0; i < relativeParts.Keys.Count; i++)
                    {
                        relativeParts[relativeParts.Keys.ElementAt(i)].First /= totalCapacity;
                    }
                }
                catch (Exception e)
                {
                    LogUtils.DoLog($"ERRO AO OBTER CAPACIDADE REL: [{info}] {e} {e.Message}\n{e.StackTrace}");
                }
            }
        }
        private int UpdateDefaultCapacity<AI>(AI ai) where AI : VehicleAI
        {
            if (!m_defaultCapacities.ContainsKey(ai.m_info.name))
            {
                m_defaultCapacities[ai.m_info.name] = (int)VehicleUtils.GetVehicleCapacityField(ai).GetValue(ai);
                if (ModInstance.DebugMode)
                {
                    LogUtils.DoLog($"STORED DEFAULT VEHICLE CAPACITY {m_defaultCapacities[ai.m_info.name]} for {ai.m_info.name}");
                }
            }
            return m_defaultCapacities[ai.m_info.name];
        }

        private readonly Dictionary<string, int> m_defaultCapacities = new Dictionary<string, int>();

        public override string SaveId => $"K45_ITM_ITMAssetSettings";



    }
}
