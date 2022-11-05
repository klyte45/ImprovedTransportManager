using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.Singleton;
using Kwytto.Utils;
using System;
using System.Collections.Generic;

namespace ImprovedTransportManager.UI
{
    public class PassengerAgeReportTab : BasicStatisticsTableView<AgePassengerReport>
    {
        public PassengerAgeReportTab(Func<ushort> getCurrentLine, Func<ushort> getCurrentStop, Func<ushort> getCurrentVehicle) : base(getCurrentLine, getCurrentStop, getCurrentVehicle)
        {
        }

        public override string TabDisplayName => Str.itm_statisticsTable_passengerAgeReport_title;

        public override List<Tuple<Func<string>, Func<AgePassengerReport, string>>> ColumnsDescriptors => new List<Tuple<Func<string>, Func<AgePassengerReport, string>>>
        {
            Tuple.New<Func<string>, Func<AgePassengerReport, string>>(()=>Str.itm_statisticsTable_passengerAgeReport_child   ,(x) =>x.Child.ToString("N0")),
            Tuple.New<Func<string>, Func<AgePassengerReport, string>>(()=>Str.itm_statisticsTable_passengerAgeReport_teen   ,(x) =>x.Teen.ToString("N0")),
            Tuple.New<Func<string>, Func<AgePassengerReport, string>>(()=>Str.itm_statisticsTable_passengerAgeReport_young   ,(x) =>x.Young.ToString("N0")),
            Tuple.New<Func<string>, Func<AgePassengerReport, string>>(()=>Str.itm_statisticsTable_passengerAgeReport_adult   ,(x) =>x.Adult.ToString("N0")),
            Tuple.New<Func<string>, Func<AgePassengerReport, string>>(()=>Str.itm_statisticsTable_passengerAgeReport_elder   ,(x) =>x.Elder.ToString("N0")),
            Tuple.New<Func<string>, Func<AgePassengerReport, string>>(()=>Str.itm_statisticsTable_passengerAgeReport_total   ,(x) =>x.Total.ToString("N0")),
        };

        protected override void AddToTotalizer(AgePassengerReport totalizer, AgePassengerReport data)
        {
            totalizer.Child += data.Child;
            totalizer.Teen += data.Teen;
            totalizer.Young += data.Young;
            totalizer.Adult += data.Adult;
            totalizer.Elder += data.Elder;
        }

        protected override List<AgePassengerReport> GetLineData(ushort lineId) => ITMTransportLineStatusesManager.Instance.GetLineAgeReport(lineId);
        protected override List<AgePassengerReport> GetStopData(ushort stopId) => new List<AgePassengerReport>();
        protected override List<AgePassengerReport> GetVehicleData(ushort vehicleId) => new List<AgePassengerReport>();
    }
}
