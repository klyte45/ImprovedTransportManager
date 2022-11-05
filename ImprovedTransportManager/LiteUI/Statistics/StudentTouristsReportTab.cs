using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.Singleton;
using Kwytto.Utils;
using System;
using System.Collections.Generic;

namespace ImprovedTransportManager.UI
{
    public class StudentTouristsReportTab : BasicStatisticsTableView<StudentsTouristsReport>
    {
        public StudentTouristsReportTab(Func<ushort> getCurrentLine, Func<ushort> getCurrentStop, Func<ushort> getCurrentVehicle) : base(getCurrentLine, getCurrentStop, getCurrentVehicle)
        {
        }

        public override string TabDisplayName => Str.itm_statisticsTable_passengerStudentsTouristsReport_title;

        public override List<Tuple<Func<string>, Func<StudentsTouristsReport, string>>> ColumnsDescriptors => new List<Tuple<Func<string>, Func<StudentsTouristsReport, string>>>
        {
            Tuple.New<Func<string>, Func<StudentsTouristsReport, string>>(()=>Str.itm_statisticsTable_passengerStudentsTouristsReport_students   ,(x) =>$"{x.Student:N0}\n{x.PercentageStudents:P2}"),
            Tuple.New<Func<string>, Func<StudentsTouristsReport, string>>(()=>Str.itm_statisticsTable_passengerStudentsTouristsReport_tourists   ,(x) =>$"{x.Tourists:N0}\n{x.PercentageTourists:P2}"),
            Tuple.New<Func<string>, Func<StudentsTouristsReport, string>>(()=>Str.itm_statisticsTable_passengerStudentsTouristsReport_total      ,(x) =>x.Total.ToString("N0"))
        };

        protected override void AddToTotalizer(StudentsTouristsReport totalizer, StudentsTouristsReport data)
        {
            totalizer.Student += data.Student;
            totalizer.Tourists += data.Tourists;
            totalizer.Total += data.Total;
        }

        protected override List<StudentsTouristsReport> GetLineData(ushort lineId) => ITMTransportLineStatusesManager.Instance.GetLineStudentTouristsTotalReport(lineId);
        protected override List<StudentsTouristsReport> GetStopData(ushort stopId) => ITMTransportLineStatusesManager.Instance.GetStopStudentTouristsTotalReport(stopId);
        protected override List<StudentsTouristsReport> GetVehicleData(ushort vehicleId) => ITMTransportLineStatusesManager.Instance.GetVehicleStudentTouristsTotalReport(vehicleId);
    }
}
