using ImprovedTransportManager.Singleton;
using System;
using System.IO;

namespace ImprovedTransportManager.Data
{
    public class ITMTransportLineStorageDetailedPassengerData_W1 : TransportLineStorageBasicData
    {
        public override string SaveId => "K45_ITM_ITMTransportLineStorageDetailedPassengerData_W1";

        protected override Enum[] LoadOrder { get; } = new Enum[]
                                                        {
                                                              LineDataUshort.W1_CHILD_MALE_PASSENGERS,
                                                              LineDataUshort.W1_TEENS_MALE_PASSENGERS,
                                                              LineDataUshort.W1_YOUNG_MALE_PASSENGERS,
                                                              LineDataUshort.W1_ADULT_MALE_PASSENGERS,
                                                              LineDataUshort.W1_ELDER_MALE_PASSENGERS,
                                                              LineDataUshort.W1_CHILD_FEML_PASSENGERS,
                                                              LineDataUshort.W1_TEENS_FEML_PASSENGERS,
                                                              LineDataUshort.W1_YOUNG_FEML_PASSENGERS,
                                                              LineDataUshort.W1_ADULT_FEML_PASSENGERS,
                                                              LineDataUshort.W1_ELDER_FEML_PASSENGERS,
                                                        };
        protected override Action<Stream, long> SerializeFunction { get; } = WriteInt16;
        protected override Func<Stream, long> DeserializeFunction { get; } = ReadInt16;
    }


}