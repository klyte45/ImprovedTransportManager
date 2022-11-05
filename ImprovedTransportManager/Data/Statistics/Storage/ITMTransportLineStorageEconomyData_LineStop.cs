using ImprovedTransportManager.Singleton;
using System;
using static ImprovedTransportManager.Singleton.ITMTransportLineStatusesManager;

namespace ImprovedTransportManager.Data
{
    public class ITMTransportLineStorageEconomyData_LineStop : TransportLineStorageBasicData
    {
        public override string SaveId => "K45_ITM_ITMTransportLineStorageEconomyData";

        protected override Enum[] LoadOrder { get; } = new Enum[]
                                                        {
                                                                LineDataLong.EXPENSE,
                                                                LineDataLong.INCOME,
                                                                StopDataLong.INCOME,
                                                        };
    }


}