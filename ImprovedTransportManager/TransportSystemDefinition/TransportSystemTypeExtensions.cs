extern alias UUI;
using ColossalFramework;
using ColossalFramework.Globalization;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImprovedTransportManager.TransportSystems
{
    public static class TransportSystemTypeExtensions
    {
        public const int ROLL_TRANSPORT_TYPE = 19;
        public const int ROLL_VEHICLE_TYPE_NTH_BIT = 14;
        public const int ROLL_SUBSERVICE = 8;
        public const int ROLL_CITY_LEVELMASK = 3;
        public const int ROLL_INTERCITY_LEVEL = 0;

        public const int LENGHT_TRANSPORT_TYPE = 5;
        public const int LENGHT_VEHICLE_TYPE_NTH_BIT = 5;
        public const int LENGHT_SUBSERVICE = 7;
        public const int LENGHT_CITY_LEVELMASK = 5;
        public const int LENGHT_INTERCITY_LEVEL = 3;

        public const uint BITMASK_TRANSPORT_TYPE = ((1 << LENGHT_TRANSPORT_TYPE) - 1);
        public const uint BITMASK_VEHICLE_TYPE_NTH_BIT = ((1 << LENGHT_VEHICLE_TYPE_NTH_BIT) - 1);
        public const uint BITMASK_SUBSERVICE = ((1 << LENGHT_SUBSERVICE) - 1);
        public const uint BITMASK_CITY_LEVELMASK = ((1 << LENGHT_CITY_LEVELMASK) - 1);
        public const uint BITMASK_INTERCITY_LEVEL = ((1 << LENGHT_INTERCITY_LEVEL) - 1);

        public const uint MASK_TRANSPORT_TYPE = BITMASK_TRANSPORT_TYPE << ROLL_TRANSPORT_TYPE;
        public const uint MASK_VEHICLE_TYPE_NTH_BIT = BITMASK_VEHICLE_TYPE_NTH_BIT << ROLL_VEHICLE_TYPE_NTH_BIT;
        public const uint MASK_SUBSERVICE = BITMASK_SUBSERVICE << ROLL_SUBSERVICE;
        public const uint MASK_CITY_LEVELMASK = BITMASK_CITY_LEVELMASK << ROLL_CITY_LEVELMASK;
        public const uint MASK_INTERCITY_LEVEL = BITMASK_INTERCITY_LEVEL << ROLL_INTERCITY_LEVEL;



        private static readonly Dictionary<TransportSystemType, TransportInfoContainer> m_infoList = new Dictionary<TransportSystemType, TransportInfoContainer>();

        public static Dictionary<TransportSystemType, TransportInfoContainer> TransportInfoDict
        {
            get
            {
                if (m_infoList.Count == 0)
                {
                    LogUtils.DoLog("TSD loading infos");
                    for (uint i = 0; i < PrefabCollection<TransportInfo>.LoadedCount(); i++)
                    {
                        TransportInfo info = PrefabCollection<TransportInfo>.GetLoaded(i);
                        var tsd = FromLocal(info);
                        if (tsd == default)
                        {
                            tsd = FromIntercity(info);

                            if (tsd == default)
                            {
                                LogUtils.DoErrorLog($"TSD not found for info: {info}");
                                continue;
                            }
                            else if (m_infoList.ContainsKey(tsd))
                            {
                                if (m_infoList[tsd].Intercity != null)
                                {
                                    LogUtils.DoErrorLog($"More than one info for same TSD Intercity \"{tsd}\": {m_infoList[tsd]},{info}");
                                    continue;
                                }
                                m_infoList[tsd].Intercity = info;
                            }
                            else
                            {
                                m_infoList[tsd] = new TransportInfoContainer
                                {
                                    Intercity = info
                                };
                            }
                        }
                        else if (m_infoList.ContainsKey(tsd))
                        {
                            if (m_infoList[tsd].Local != null)
                            {
                                LogUtils.DoErrorLog($"More than one info for same TSD Local \"{tsd}\": {m_infoList[tsd]},{info}");
                                continue;
                            }
                            m_infoList[tsd].Local = info;
                        }
                        else
                        {
                            m_infoList[tsd] = new TransportInfoContainer
                            {
                                Local = info
                            };
                        }
                    }
                    IEnumerable<TransportSystemType> missing = m_allTypes.Where(x => !m_infoList.ContainsKey(x));
                    if (missing.Count() > 0 && ModInstance.DebugMode)
                    {
                        LogUtils.DoLog($"Some TSDs can't find their infos: [{string.Join(", ", missing.Select(x => x.ToString()).ToArray())}]\nIgnore if you don't have all DLCs installed");
                    }
                    LogUtils.DoLog("TSD end loading infos");
                }
                return m_infoList;
            }
        }

        private static readonly TransportSystemType[] m_allTypes = Enum.GetValues(typeof(TransportSystemType)).Cast<TransportSystemType>().ToArray();

        public static VehicleInfo.VehicleType ToVehicleType(this TransportSystemVehicleType tsvt) => tsvt == TransportSystemVehicleType.None ? VehicleInfo.VehicleType.None : (VehicleInfo.VehicleType)(1 << ((int)tsvt));
        public static bool Is(this TransportSystemType tst, ItemClass.SubService s) => ((uint)tst & MASK_SUBSERVICE) >> ROLL_SUBSERVICE == (uint)s;
        public static bool Is(this TransportSystemType tst, VehicleInfo.VehicleType s) => ToVehicleType((TransportSystemVehicleType)(((uint)tst & MASK_VEHICLE_TYPE_NTH_BIT) >> ROLL_VEHICLE_TYPE_NTH_BIT)) == s;
        public static bool Is(this TransportSystemType tst, TransportInfo.TransportType s) => ((uint)tst & MASK_TRANSPORT_TYPE) >> ROLL_TRANSPORT_TYPE == (uint)s;
        public static bool IsCitywide(this TransportSystemType tst, ItemClass.Level s) => ((((uint)tst & MASK_CITY_LEVELMASK) >> ROLL_CITY_LEVELMASK) & (1 << (int)s)) != 0;
        public static bool IsIntercity(this TransportSystemType tst, ItemClass.Level s) => (((uint)tst & MASK_INTERCITY_LEVEL) >> ROLL_INTERCITY_LEVEL) == (uint)s;
        public static bool HasIntercity(this TransportSystemType tst) => ((uint)tst & MASK_INTERCITY_LEVEL) >> ROLL_INTERCITY_LEVEL != BITMASK_INTERCITY_LEVEL;
        public static int DefaultCapacity(this TransportSystemType tst)
        {
            switch (tst)
            {
                case TransportSystemType.BUS: return 30;
                case TransportSystemType.BLIMP: return 35;
                case TransportSystemType.BALLOON: return 1;
                case TransportSystemType.CABLE_CAR: return 30;
                case TransportSystemType.EVAC_BUS: return 50;
                case TransportSystemType.FERRY: return 50;
                case TransportSystemType.FISHING: return 1;
                case TransportSystemType.HELICOPTER: return 10;
                case TransportSystemType.METRO: return 30;
                case TransportSystemType.MONORAIL: return 30;
                case TransportSystemType.PLANE: return 200;
                case TransportSystemType.POST: return 1;
                case TransportSystemType.SHIP: return 100;
                case TransportSystemType.TAXI: return 1;
                case TransportSystemType.TOUR_BUS: return 30;
                case TransportSystemType.TOUR_PED: return 1;
                case TransportSystemType.TRAIN: return 40;
                case TransportSystemType.TRAM: return 30;
                case TransportSystemType.TROLLEY: return 30;
                default: return 1;
            }
        }

        public static TransportSystemType FromLocal(TransportInfo info)
        {
            if (info is null)
            {
                return default;
            }
            var result = m_allTypes.FirstOrDefault(x =>
            x.Is(info.m_class.m_subService)
            && x.Is(info.m_vehicleType)
            && x.Is(info.m_transportType)
            && x.IsCitywide(info.GetClassLevel())
            );
            if (result == default)
            {
                LogUtils.DoErrorLog($"Local TSD NOT FOUND FOR TRANSPORT INFO: info.m_class.m_subService={info.m_class.m_subService}, info.m_vehicleType={info.m_vehicleType}, info.m_transportType={info.m_transportType}, info.classLevel = {info.GetClassLevel()}");
            }
            return result;
        }


        public static TransportSystemType FromNetInfo(NetInfo info)
        {
            if (info is null)
            {
                return default;
            }
            var result = m_allTypes.FirstOrDefault(x =>
            x.Is(info.m_class.m_subService)
            && (info.m_lanes.Any(lane => x.Is(lane.m_stopType)) || (info.m_netAI is TransportLineAI tlai && x.Is(tlai.m_vehicleType)))
            && (x.IsIntercity(info.GetClassLevel()) || x.IsCitywide(info.GetClassLevel())));
            return result;
        }
        public static TransportSystemType FromIntercity(TransportInfo info)
        {
            if (info is null)
            {
                return default;
            }
            var result = m_allTypes.FirstOrDefault(x =>
            x.Is(info.m_class.m_subService)
            && x.Is(info.m_vehicleType)
            && x.Is(info.m_transportType)
            && x.IsIntercity(info.GetClassLevel())
            );
            if (result == default)
            {
                LogUtils.DoLog($"Intercity TSD NOT FOUND FOR TRANSPORT INFO: info.m_class.m_subService={info.m_class.m_subService}, info.m_vehicleType={info.m_vehicleType}, info.m_transportType={info.m_transportType}, info.classLevel = {info.GetClassLevel()}");
            }
            return result;
        }

        public static TransportSystemType From(PrefabAI prefabAI) =>
           prefabAI is DepotAI depotAI
               ? FromLocal(depotAI.m_transportInfo)
               : prefabAI is OutsideConnectionAI ocAI
                   ? FromIntercity(ocAI.m_transportInfo)
                   : default;

        public static TransportSystemType From(VehicleInfo info) =>
            info is null
                ? (default)
                : m_allTypes.FirstOrDefault(x =>
                    x.Is(info.m_class.m_subService)
                    && x.Is(info.m_vehicleType)
                && (x.IsIntercity(info.GetClassLevel()) || x.IsCitywide(info.GetClassLevel()))
                    && ReflectionUtils.HasField(info.GetAI(), "m_transportInfo")
                    && (info.GetAI() is PrefabAI prefabAI) && prefabAI.GetType().GetField("m_transportInfo").GetValue(prefabAI) is TransportInfo ti
                    && x.Is(ti.m_transportType)
                );


        public static NetInfo GetLineInfoLocal(this TransportSystemType tst)
            => NetIndexes.instance.PrefabsData.Values.Where(x => (x.Info is NetInfo info) && info.m_netAI is TransportLineAI tlai && tst.Is(info.m_class.m_subService) && tst.Is(tlai.m_vehicleType) && tst.IsCitywide(info.m_class.m_level)).FirstOrDefault()?.Info as NetInfo;

        public static NetInfo GetLineInfoIntercity(this TransportSystemType tst)
            => tst.HasIntercity()
            ? NetIndexes.instance.PrefabsData.Values.Where(x => (x.Info is NetInfo info) && info.m_netAI is TransportLineAI tlai && tst.Is(info.m_class.m_subService) && tst.Is(tlai.m_vehicleType) && tst.IsIntercity(info.m_class.m_level)).FirstOrDefault()?.Info as NetInfo
            : null;

        public static TransportInfo GetTransportInfoLocal(this TransportSystemType tst)
            => TransportIndexes.instance.PrefabsData.Values.Where(x => x.Info is TransportInfo ti && tst.Is(ti.m_transportType) && tst.Is(ti.m_class.m_subService) && tst.Is(ti.m_vehicleType) && tst.IsCitywide(ti.m_class.m_level)).FirstOrDefault()?.Info as TransportInfo;
        public static TransportInfo GetTransportInfoIntercity(this TransportSystemType tst)
            => tst.HasIntercity()
            ? TransportIndexes.instance.PrefabsData.Values.Where(x => x.Info is TransportInfo ti && tst.Is(ti.m_transportType) && tst.Is(ti.m_class.m_subService) && tst.Is(ti.m_vehicleType) && tst.IsIntercity(ti.m_class.m_level)).FirstOrDefault()?.Info as TransportInfo
            : null;
        public static bool IsTour(this TransportSystemType tst) => tst.Is(ItemClass.SubService.PublicTransportTours);
        public static bool IsShelterAiDepot(this TransportSystemType tst) => tst == TransportSystemType.EVAC_BUS;
        public static bool HasVehicles(this TransportSystemType tst) => !tst.Is(VehicleInfo.VehicleType.None);



        public static bool IsFromSystemCityWide(this TransportSystemType x, VehicleInfo info)
            => x.Is(info.m_class.m_subService)
            && x.Is(info.m_vehicleType)
            && x.IsCitywide(info.GetClassLevel())
            && x.Is((VehicleUtils.GetTransportInfoField(info.m_vehicleAI)?.GetValue(info.m_vehicleAI) as TransportInfo)?.m_transportType ?? (TransportInfo.TransportType)(-1))
            && VehicleUtils.GetVehicleCapacityField(info.m_vehicleAI) != null;


        public static bool IsFromSystemIntercity(this TransportSystemType x, VehicleInfo info)
            => x.Is(info.m_class.m_subService)
            && x.IsIntercity(info.GetClassLevel())
            && x.Is(info.m_vehicleType)
            && x.Is((VehicleUtils.GetTransportInfoField(info.m_vehicleAI)?.GetValue(info.m_vehicleAI) as TransportInfo)?.m_transportType ?? (TransportInfo.TransportType)(-1))
             && VehicleUtils.GetVehicleCapacityField(info.m_vehicleAI) != null;

        public static bool IsFromSystem(this TransportSystemType x, TransportInfo info)
            => info != null
            && x.Is(info.m_class.m_subService)
            && x.Is(info.m_vehicleType)
            && x.Is(info.m_transportType);

        public static bool IsFromSystem(this TransportSystemType x, DepotAI p)
            => p != null
            && ((
                p.m_maxVehicleCount > 0
                && x.Is(p.m_info.m_class.m_subService)
                && x.Is(p.m_transportInfo.m_vehicleType)
                && x.Is(p.m_transportInfo.m_transportType)
            ) || (
                p.m_secondaryTransportInfo != null
                && p.m_maxVehicleCount2 > 0
                && x.Is(p.m_secondaryTransportInfo.m_vehicleType)
                && x.Is(p.m_secondaryTransportInfo.m_transportType)
            ));
        public static bool IsFromSystem(this TransportSystemType x, ref TransportLine tl)
            => x.Is(tl.Info.m_class.m_subService)
            && x.Is(tl.Info.m_vehicleType)
            && x.Is(tl.Info.m_transportType);

        public static TransportSystemType FromLineId(ushort lineId, bool fromBuilding)
            => fromBuilding
                ? FromNetInfo(NetManager.instance.m_nodes.m_buffer[lineId].Info)
                : FromLocal(Singleton<TransportManager>.instance.m_lines.m_buffer[lineId].Info);

        public static bool IsIntercityBusConnection(this TransportSystemType x, BuildingInfo connectionInfo)
                 => connectionInfo.m_class.m_service == ItemClass.Service.Road && x == TransportSystemType.BUS && connectionInfo.m_class.m_subService == ItemClass.SubService.None;
        public static bool IsIntercityBusConnectionTrack(this TransportSystemType x, NetInfo trackInfo)
            => trackInfo.m_class.m_service == ItemClass.Service.Road && x == TransportSystemType.BUS && trackInfo.m_class.m_subService == ItemClass.SubService.None;
        public static bool IsValidOutsideConnection(this TransportSystemType x, ushort outsideConnectionBuildingId)
            => BuildingManager.instance.m_buildings.m_buffer[outsideConnectionBuildingId].Info is BuildingInfo outsideConn
            && outsideConn.m_buildingAI is OutsideConnectionAI
            && (
                FromOutsideConnection(outsideConn.m_class.m_subService, outsideConn.m_class.m_level, VehicleInfo.VehicleType.None) == x
                 || x.IsIntercityBusConnection(outsideConn)
            );

        public static bool IsValidOutsideConnectionTrack(this TransportSystemType x, NetInfo netInfo) =>
              FromOutsideConnection(netInfo.m_class.m_subService, netInfo.m_class.m_level, VehicleInfo.VehicleType.None) == x
              || x.IsIntercityBusConnectionTrack(netInfo);

        internal static TransportSystemType FromOutsideConnection(ItemClass.SubService subService, ItemClass.Level level, VehicleInfo.VehicleType type)
            => subService == ItemClass.SubService.PublicTransportTrain //TEMPORARY!
            ? m_allTypes.Where(x => x.IsIntercity(level) && x.Is(subService) && (type == VehicleInfo.VehicleType.None || x.Is(type))).FirstOrDefault()
            : default;
        public static TransportSystemType From(TransportInfo.TransportType TransportType, ItemClass.SubService SubService, VehicleInfo.VehicleType VehicleType, ItemClass.Level Level)
            => m_allTypes.FirstOrDefault(x =>
                    x.Is(SubService)
                    && x.Is(VehicleType)
                    && (x.IsIntercity(Level) || x.IsCitywide(Level))
                    && x.Is(TransportType)
            );


        public static float GetEffectivePassengerCapacityCost(this TransportSystemType x)
        {
            int settedCost =/* GetConfig()?.DefaultCostPerPassenger ??*/ 0;
            return settedCost <= 0 ? x.GetDefaultPassengerCapacityCostLocal() : settedCost / 100f;
        }
        public static float GetDefaultPassengerCapacityCostLocal(this TransportSystemType x) => TransportInfoDict.TryGetValue(x, out TransportInfoContainer info) && !(info.Local is null) ? info.Local.m_maintenanceCostPerVehicle / (float)x.DefaultCapacity() : -1;

        public static string GetTransportName(this TransportSystemType x)
        {
            switch (x)
            {
                case TransportSystemType.TRAIN: return Locale.Get("VEHICLE_TITLE", "Train Engine");
                case TransportSystemType.TRAM: return Locale.Get("VEHICLE_TITLE", "Tram");
                case TransportSystemType.METRO: return Locale.Get("VEHICLE_TITLE", "Metro");
                case TransportSystemType.BUS: return Locale.Get("VEHICLE_TITLE", "Bus");
                case TransportSystemType.PLANE: return Locale.Get("VEHICLE_TITLE", "Aircraft Passenger");
                case TransportSystemType.SHIP: return Locale.Get("VEHICLE_TITLE", "Ship Passenger");
                case TransportSystemType.BLIMP: return Locale.Get("VEHICLE_TITLE", "Blimp");
                case TransportSystemType.FERRY: return Locale.Get("VEHICLE_TITLE", "Ferry");
                case TransportSystemType.MONORAIL: return Locale.Get("VEHICLE_TITLE", "Monorail Front");
                case TransportSystemType.EVAC_BUS: return Locale.Get("VEHICLE_TITLE", "Evacuation Bus");
                case TransportSystemType.TOUR_BUS: return Locale.Get("TOOLTIP_TOURISTBUSLINES");
                case TransportSystemType.TOUR_PED: return Locale.Get("TOOLTIP_WALKINGTOURS");
                case TransportSystemType.CABLE_CAR: return Locale.Get("VEHICLE_TITLE", "Cable Car");
                case TransportSystemType.TAXI: return Locale.Get("VEHICLE_TITLE", "Taxi");
                case TransportSystemType.HELICOPTER: return Locale.Get("VEHICLE_TITLE", "Passenger Helicopter");
                case TransportSystemType.TROLLEY: return Locale.Get("VEHICLE_TITLE", "Trolleybus 01");
                case TransportSystemType.BALLOON:
                case TransportSystemType.FISHING:
                case TransportSystemType.POST:
                default: return "???";
            }
        }
    }
}
