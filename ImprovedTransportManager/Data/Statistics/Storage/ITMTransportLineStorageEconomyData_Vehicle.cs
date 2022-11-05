using ImprovedTransportManager.Singleton;
using System;
using System.IO;
using static ImprovedTransportManager.Singleton.ITMTransportLineStatusesManager;

namespace ImprovedTransportManager.Data
{
    public class ITMTransportLineStorageEconomyData_Vehicle : TransportLineStorageBasicData
    {
        public override string SaveId => "K45_ITM_ITMTransportLineStorageEconomyData_Vehicle";

        protected override Enum[] LoadOrder { get; } = new Enum[]
                                                        {
                                                                VehicleDataLong.EXPENSE,
                                                                VehicleDataLong.INCOME,
                                                        };
        protected override Action<Stream, long> SerializeFunction { get; } = WriteSemiLong;
        protected override Func<Stream, long> DeserializeFunction { get; } = ReadSemiLong;
    }


}