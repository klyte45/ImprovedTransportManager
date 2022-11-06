using ColossalFramework;
using ImprovedTransportManager.Data;
using ImprovedTransportManager.TransportSystems;
using ImprovedTransportManager.Utility;
using Kwytto.Utils;
using System.Linq;
using System.Security.AccessControl;
using UnityEngine;

namespace ImprovedTransportManager.Overrides
{
    public class ITMDepotAIOverrides : Redirector, IRedirectable
    {

        #region Generation methods

        private static VehicleInfo DoModelDraw(ushort lineId) => ITMTransportLineSettings.Instance.GetAModel(lineId);

        public static void SetRandomBuilding(TransportSystemType tsd, ushort lineId, ref ushort currentId)
        {
            var targetDepot = ITMTransportLineSettings.Instance.GetADepot(lineId);
            if (targetDepot != 0)
            {
                currentId = targetDepot;
            }
        }
        #endregion

        #region Overrides

        private static readonly TransferManager.TransferReason[] m_managedReasons = new TransferManager.TransferReason[]   {
                TransferManager.TransferReason.Tram,
                TransferManager.TransferReason.PassengerTrain,
                TransferManager.TransferReason.PassengerShip,
                TransferManager.TransferReason.PassengerPlane,
                TransferManager.TransferReason.MetroTrain,
                TransferManager.TransferReason.Monorail,
                TransferManager.TransferReason.CableCar,
                TransferManager.TransferReason.Blimp,
                TransferManager.TransferReason.Bus,
                TransferManager.TransferReason.Ferry,
                TransferManager.TransferReason.Trolleybus,
                TransferManager.TransferReason.PassengerHelicopter,
                TransferManager.TransferReason.TouristBus,
            };

        public static bool StartTransfer(DepotAI __instance, ushort buildingID, TransferManager.TransferReason reason, TransferManager.TransferOffer offer)
        {
            if (!m_managedReasons.Contains(reason) || offer.TransportLine == 0)
            {
                return true;
            }

            LogUtils.DoLog("START TRANSFER!!!!!!!!");
            TransportInfo m_transportInfo = __instance.m_transportInfo;
            BuildingInfo m_info = __instance.m_info;

            if (ModInstance.DebugMode)
            {
                LogUtils.DoLog("m_info {0} | m_transportInfo {1} | Line: {2}", m_info.name, m_transportInfo.name, offer.TransportLine);
            }


            if (reason == m_transportInfo.m_vehicleReason || (__instance.m_secondaryTransportInfo != null && reason == __instance.m_secondaryTransportInfo.m_vehicleReason))
            {
                var tsd = __instance.m_transportInfo.ToLocalTST();
                if (tsd == default)
                {
                    return true;
                }
                SetRandomBuilding(tsd, offer.TransportLine, ref buildingID);

                LogUtils.DoLog("randomVehicleInfo");
                VehicleInfo randomVehicleInfo = DoModelDraw(offer.TransportLine);
                if (randomVehicleInfo == null)
                {
                    randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, m_info.m_class.m_service, m_info.m_class.m_subService, m_info.m_class.m_level);
                }
                if (randomVehicleInfo != null)
                {
                    LogUtils.DoLog("randomVehicleInfo != null");
                    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                    __instance.CalculateSpawnPosition(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID], ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, out Vector3 position, out _);
                    if (Singleton<VehicleManager>.instance.CreateVehicle(out ushort vehicleID, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, position, reason, false, true))
                    {
                        LogUtils.DoLog("CreatedVehicle!!!");
                        randomVehicleInfo.m_vehicleAI.SetSource(vehicleID, ref vehicles.m_buffer[vehicleID], buildingID);
                        randomVehicleInfo.m_vehicleAI.StartTransfer(vehicleID, ref vehicles.m_buffer[vehicleID], reason, offer);
                    }
                    return false;
                }
            }
            return true;

        }
        #endregion

        #region Hooking

        public void Awake()
        {
            LogUtils.DoLog("Loading Depot Hooks!");
            AddRedirect(typeof(DepotAI).GetMethod("StartTransfer", ReflectionUtils.allFlags), typeof(ITMDepotAIOverrides).GetMethod("StartTransfer", ReflectionUtils.allFlags));
        }

        #endregion

    }
}
