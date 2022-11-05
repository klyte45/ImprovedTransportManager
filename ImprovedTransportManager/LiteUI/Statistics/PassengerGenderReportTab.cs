using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.Singleton;
using Kwytto.Utils;
using System;
using System.Collections.Generic;

namespace ImprovedTransportManager.UI
{
    public class PassengerGenderReportTab : BasicStatisticsTableView<GenderPassengerReport>
    {
        public PassengerGenderReportTab(Func<ushort> getCurrentLine, Func<ushort> getCurrentStop, Func<ushort> getCurrentVehicle) : base(getCurrentLine, getCurrentStop, getCurrentVehicle)
        {
        }

        public override string TabDisplayName => Str.itm_statisticsTable_passengerGenderReport_title;

        public override List<Tuple<Func<string>, Func<GenderPassengerReport, string>>> ColumnsDescriptors => new List<Tuple<Func<string>, Func<GenderPassengerReport, string>>>
        {
            Tuple.New<Func<string>, Func<GenderPassengerReport, string>>(()=>Str.itm_statisticsTable_passengerGenderReport_male  ,(x) =>x.Male.ToString("N0")),
            Tuple.New<Func<string>, Func<GenderPassengerReport, string>>(()=>Str.itm_statisticsTable_passengerGenderReport_female ,(x) =>x.Female.ToString("N0")),
            Tuple.New<Func<string>, Func<GenderPassengerReport, string>>(()=>Str.itm_statisticsTable_passengerGenderReport_total   ,(x) =>x.Total.ToString("N0")),
        };

        protected override void AddToTotalizer(GenderPassengerReport totalizer, GenderPassengerReport data)
        {
            totalizer.Male += data.Male;
            totalizer.Female += data.Female;
        }

        protected override List<GenderPassengerReport> GetLineData(ushort lineId) => ITMTransportLineStatusesManager.Instance.GetLineGenderReport(lineId);
        protected override List<GenderPassengerReport> GetStopData(ushort stopId) => new List<GenderPassengerReport>();
        protected override List<GenderPassengerReport> GetVehicleData(ushort vehicleId) => new List<GenderPassengerReport>();
    }
}
