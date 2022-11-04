using System;
using System.IO;
using static ImprovedTransportManager.Data.ITMTransportLineStatusesManager;

namespace ImprovedTransportManager.Data
{
    public class ITMTransportLineStorageDetailedPassengerData_W3 : TransportLineStorageBasicData
    {
        public override string SaveId => "K45_ITM_ITMTransportLineStorageDetailedPassengerData_W3";

        protected override Enum[] LoadOrder { get; } = new Enum[]
                                                        {
                                                              LineDataUshort.W3_CHILD_MALE_PASSENGERS,
                                                              LineDataUshort.W3_TEENS_MALE_PASSENGERS,
                                                              LineDataUshort.W3_YOUNG_MALE_PASSENGERS,
                                                              LineDataUshort.W3_ADULT_MALE_PASSENGERS,
                                                              LineDataUshort.W3_ELDER_MALE_PASSENGERS,
                                                              LineDataUshort.W3_CHILD_FEML_PASSENGERS,
                                                              LineDataUshort.W3_TEENS_FEML_PASSENGERS,
                                                              LineDataUshort.W3_YOUNG_FEML_PASSENGERS,
                                                              LineDataUshort.W3_ADULT_FEML_PASSENGERS,
                                                              LineDataUshort.W3_ELDER_FEML_PASSENGERS,
                                                        };
        protected override Action<Stream, long> SerializeFunction { get; } = WriteInt16;
        protected override Func<Stream, long> DeserializeFunction { get; } = ReadInt16;
    }


}