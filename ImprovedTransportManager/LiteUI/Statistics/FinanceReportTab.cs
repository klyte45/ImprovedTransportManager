using ImprovedTransportManager.Data;
using ImprovedTransportManager.Localization;
using ImprovedTransportManager.Singleton;
using Kwytto.Utils;
using System;
using System.Collections.Generic;

namespace ImprovedTransportManager.UI
{
    public class FinanceReportTab : BasicStatisticsTableView<IncomeExpenseReport>
    {
        public FinanceReportTab(Func<ushort> getCurrentLine, Func<ushort> getCurrentStop, Func<ushort> getCurrentVehicle) : base(getCurrentLine, getCurrentStop, getCurrentVehicle)
        {
        }

        public override string TabDisplayName => Str.itm_statisticsTable_financeReport_title;

        public override List<Tuple<Func<string>, Func<IncomeExpenseReport, string>>> ColumnsDescriptors => new List<Tuple<Func<string>, Func<IncomeExpenseReport, string>>>
        {
            Tuple.New<Func<string>, Func<IncomeExpenseReport, string>>(()=>Str.itm_statisticsTable_financeReport_income,(x) => (x.Income*.01f).ToGameCurrencyFormat()),
            Tuple.New<Func<string>, Func<IncomeExpenseReport, string>>(()=>Str.itm_statisticsTable_financeReport_expense,(x) => loadedReportType == LoadedDataType.Stop ? Str.itm_common_notApplicableAcronym : (x.Expense*.01f).ToGameCurrencyFormat()),
            Tuple.New<Func<string>, Func<IncomeExpenseReport, string>>(()=>Str.itm_statisticsTable_financeReport_balance,(x) => ((x.Income-x.Expense)*.01f).ToGameCurrencyFormat()),
        };

        protected override void AddToTotalizer(IncomeExpenseReport totalizer, IncomeExpenseReport data)
        {
            totalizer.Income += data.Income;
            totalizer.Expense += data.Expense;
        }

        protected override List<IncomeExpenseReport> GetLineData(ushort lineId) => ITMTransportLineStatusesManager.Instance.GetLineFinanceReport(lineId);
        protected override List<IncomeExpenseReport> GetStopData(ushort stopId) => ITMTransportLineStatusesManager.Instance.GetStopFinanceReport(stopId);
        protected override List<IncomeExpenseReport> GetVehicleData(ushort vehicleId) => ITMTransportLineStatusesManager.Instance.GetVehicleFinanceReport(vehicleId);
    }
}
