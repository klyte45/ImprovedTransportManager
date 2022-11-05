using ImprovedTransportManager.Singleton;
using System;
using System.IO;
using static ImprovedTransportManager.Singleton.ITMTransportLineStatusesManager;

namespace ImprovedTransportManager.Data
{
    public class ITMTransportLineStoragePassengerData_Vehicles : TransportLineStorageBasicData
    {
        public override string SaveId => "K45_ITM_ITMTransportLineStoragePassengerData";

        protected override Enum[] LoadOrder { get; } = new Enum[]
                                                        {
                                                                 VehicleDataSmallInt.TOTAL_PASSENGERS,
                                                                 VehicleDataSmallInt.TOURIST_PASSENGERS,
                                                                 VehicleDataSmallInt.STUDENT_PASSENGERS,
                                                        };
        protected override Action<Stream, long> SerializeFunction { get; } = WriteInt24;
        protected override Func<Stream, long> DeserializeFunction { get; } = ReadInt24;
    }


}