using System;
using System.IO;
using static ImprovedTransportManager.Data.ITMTransportLineStatusesManager;

namespace ImprovedTransportManager.Data
{
    public class ITMTransportLineStoragePassengerData_LineStop : TransportLineStorageBasicData
    {
        public override string SaveId => "K45_ITM_ITMTransportLineStoragePassengerData_LineStop";

        protected override Enum[] LoadOrder { get; } = new Enum[]
                                                        {
                                                                 LineDataSmallInt.TOTAL_PASSENGERS,
                                                                 LineDataSmallInt.TOURIST_PASSENGERS,
                                                                 LineDataSmallInt.STUDENT_PASSENGERS,
                                                                 StopDataSmallInt.TOTAL_PASSENGERS,
                                                                 StopDataSmallInt.TOURIST_PASSENGERS,
                                                                 StopDataSmallInt.STUDENT_PASSENGERS,
                                                        };
        protected override Action<Stream, long> SerializeFunction { get; } = WriteInt24;
        protected override Func<Stream, long> DeserializeFunction { get; } = ReadInt24;
    }


}