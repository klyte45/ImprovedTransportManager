using System;
using System.IO;
using static ImprovedTransportManager.Data.ITMTransportLineStatusesManager;

namespace ImprovedTransportManager.Data
{
    public class ITMTransportLineStorageDetailedPassengerData_W2 : TransportLineStorageBasicData
    {
        public override string SaveId => "K45_ITM_ITMTransportLineStorageDetailedPassengerData_W2";

        protected override Enum[] LoadOrder { get; } = new Enum[]
                                                        {
                                                              LineDataUshort.W2_CHILD_MALE_PASSENGERS,
                                                              LineDataUshort.W2_TEENS_MALE_PASSENGERS,
                                                              LineDataUshort.W2_YOUNG_MALE_PASSENGERS,
                                                              LineDataUshort.W2_ADULT_MALE_PASSENGERS,
                                                              LineDataUshort.W2_ELDER_MALE_PASSENGERS,
                                                              LineDataUshort.W2_CHILD_FEML_PASSENGERS,
                                                              LineDataUshort.W2_TEENS_FEML_PASSENGERS,
                                                              LineDataUshort.W2_YOUNG_FEML_PASSENGERS,
                                                              LineDataUshort.W2_ADULT_FEML_PASSENGERS,
                                                        };
        protected override Action<Stream, long> SerializeFunction { get; } = WriteInt16;
        protected override Func<Stream, long> DeserializeFunction { get; } = ReadInt16;
    }


}