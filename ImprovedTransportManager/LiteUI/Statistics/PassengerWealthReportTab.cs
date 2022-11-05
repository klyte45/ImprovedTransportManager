using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.Singleton;
using Kwytto.Utils;
using System;
using System.Collections.Generic;

namespace ImprovedTransportManager.UI
{
    public class PassengerWealthReportTab : BasicStatisticsTableView<WealthPassengerReport>
    {
        public PassengerWealthReportTab(Func<ushort> getCurrentLine, Func<ushort> getCurrentStop, Func<ushort> getCurrentVehicle) : base(getCurrentLine, getCurrentStop, getCurrentVehicle)
        {
        }

        public override string TabDisplayName => Str.itm_statisticsTable_passengerWealthReport_title;

        public override List<Tuple<Func<string>, Func<WealthPassengerReport, string>>> ColumnsDescriptors => new List<Tuple<Func<string>, Func<WealthPassengerReport, string>>>
        {
            Tuple.New<Func<string>, Func<WealthPassengerReport, string>>(()=>Str.itm_statisticsTable_passengerWealthReport_low ,(x) =>x.Low.ToString("N0")),
            Tuple.New<Func<string>, Func<WealthPassengerReport, string>>(()=>Str.itm_statisticsTable_passengerWealthReport_medium ,(x) =>x.Medium.ToString("N0")),
            Tuple.New<Func<string>, Func<WealthPassengerReport, string>>(()=>Str.itm_statisticsTable_passengerWealthReport_high ,(x) =>x.High.ToString("N0")),
            Tuple.New<Func<string>, Func<WealthPassengerReport, string>>(()=>Str.itm_statisticsTable_passengerWealthReport_total   ,(x) =>x.Total.ToString("N0")),
        };

        protected override void AddToTotalizer(WealthPassengerReport totalizer, WealthPassengerReport data)
        {
            totalizer.Low += data.Low;
            totalizer.Medium += data.Medium;
            totalizer.High += data.High;
        }

        protected override List<WealthPassengerReport> GetLineData(ushort lineId) => ITMTransportLineStatusesManager.Instance.GetLineWealthReport(lineId);
        protected override List<WealthPassengerReport> GetStopData(ushort stopId) => ITMTransportLineStatusesManager.Instance.GetStopWealthReport(stopId);
        protected override List<WealthPassengerReport> GetVehicleData(ushort vehicleId) => ITMTransportLineStatusesManager.Instance.GetVehicleWealthReport(vehicleId);
    }
}
