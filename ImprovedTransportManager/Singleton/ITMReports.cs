using ColossalFramework;
using ImprovedTransportManager.Data;
using System.Collections.Generic;
using System.Linq;
using static ImprovedTransportManager.Singleton.ITMTransportLineStatusesManager;

namespace ImprovedTransportManager.Singleton
{
    public static class ITMReports
    {
        #region Enum grouping

        private static LineDataUshort[] LowWealthData = new LineDataUshort[]
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
        private static LineDataUshort[] MedWealthData = new LineDataUshort[]
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
            LineDataUshort.W2_ELDER_FEML_PASSENGERS,
};
        private static LineDataUshort[] HghWealthData = new LineDataUshort[]
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



        private static LineDataUshort[] ChildData = new LineDataUshort[]
        {
            LineDataUshort.W1_CHILD_MALE_PASSENGERS,
            LineDataUshort.W1_CHILD_FEML_PASSENGERS,
            LineDataUshort.W2_CHILD_MALE_PASSENGERS,
            LineDataUshort.W2_CHILD_FEML_PASSENGERS,
            LineDataUshort.W3_CHILD_MALE_PASSENGERS,
            LineDataUshort.W3_CHILD_FEML_PASSENGERS,
};
        private static LineDataUshort[] TeenData = new LineDataUshort[]
         {
            LineDataUshort.W1_TEENS_MALE_PASSENGERS,
            LineDataUshort.W1_TEENS_FEML_PASSENGERS,
            LineDataUshort.W2_TEENS_MALE_PASSENGERS,
            LineDataUshort.W2_TEENS_FEML_PASSENGERS,
            LineDataUshort.W3_TEENS_MALE_PASSENGERS,
            LineDataUshort.W3_TEENS_FEML_PASSENGERS,
};
        private static LineDataUshort[] YoungData = new LineDataUshort[]
         {
            LineDataUshort.W1_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W1_YOUNG_FEML_PASSENGERS,
            LineDataUshort.W2_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W2_YOUNG_FEML_PASSENGERS,
            LineDataUshort.W3_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W3_YOUNG_FEML_PASSENGERS,
        };
        private static LineDataUshort[] AdultData = new LineDataUshort[]
         {
            LineDataUshort.W1_ADULT_MALE_PASSENGERS,
            LineDataUshort.W1_ADULT_FEML_PASSENGERS,
            LineDataUshort.W2_ADULT_MALE_PASSENGERS,
            LineDataUshort.W2_ADULT_FEML_PASSENGERS,
            LineDataUshort.W3_ADULT_MALE_PASSENGERS,
            LineDataUshort.W3_ADULT_FEML_PASSENGERS,
        };
        private static LineDataUshort[] ElderData = new LineDataUshort[]
         {
            LineDataUshort.W1_ELDER_MALE_PASSENGERS,
            LineDataUshort.W1_ELDER_FEML_PASSENGERS,
            LineDataUshort.W2_ELDER_MALE_PASSENGERS,
            LineDataUshort.W2_ELDER_FEML_PASSENGERS,
            LineDataUshort.W3_ELDER_MALE_PASSENGERS,
            LineDataUshort.W3_ELDER_FEML_PASSENGERS,
        };
        private static LineDataUshort[] MaleData = new LineDataUshort[]
         {
            LineDataUshort.W1_CHILD_MALE_PASSENGERS,
            LineDataUshort.W1_TEENS_MALE_PASSENGERS,
            LineDataUshort.W1_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W1_ADULT_MALE_PASSENGERS,
            LineDataUshort.W1_ELDER_MALE_PASSENGERS,
            LineDataUshort.W2_CHILD_MALE_PASSENGERS,
            LineDataUshort.W2_TEENS_MALE_PASSENGERS,
            LineDataUshort.W2_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W2_ADULT_MALE_PASSENGERS,
            LineDataUshort.W2_ELDER_MALE_PASSENGERS,
            LineDataUshort.W3_CHILD_MALE_PASSENGERS,
            LineDataUshort.W3_TEENS_MALE_PASSENGERS,
            LineDataUshort.W3_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W3_ADULT_MALE_PASSENGERS,
            LineDataUshort.W3_ELDER_MALE_PASSENGERS,
        };
        private static LineDataUshort[] FemaleData = new LineDataUshort[]
        {
           LineDataUshort.W1_CHILD_FEML_PASSENGERS,
           LineDataUshort.W1_TEENS_FEML_PASSENGERS,
           LineDataUshort.W1_YOUNG_FEML_PASSENGERS,
           LineDataUshort.W1_ADULT_FEML_PASSENGERS,
           LineDataUshort.W1_ELDER_FEML_PASSENGERS,
           LineDataUshort.W2_CHILD_FEML_PASSENGERS,
           LineDataUshort.W2_TEENS_FEML_PASSENGERS,
           LineDataUshort.W2_YOUNG_FEML_PASSENGERS,
           LineDataUshort.W2_ADULT_FEML_PASSENGERS,
           LineDataUshort.W2_ELDER_FEML_PASSENGERS,
           LineDataUshort.W3_CHILD_FEML_PASSENGERS,
           LineDataUshort.W3_TEENS_FEML_PASSENGERS,
           LineDataUshort.W3_YOUNG_FEML_PASSENGERS,
           LineDataUshort.W3_ADULT_FEML_PASSENGERS,
           LineDataUshort.W3_ELDER_FEML_PASSENGERS,
        };

        #endregion

        #region Report extraction - Lines
        public static List<IncomeExpenseReport> GetLineFinanceReport(this ITMTransportLineStatusesManager manager, ushort lineId)
        {
            var result = new List<IncomeExpenseReport>();
            for (int j = 0; j < CYCLES_HISTORY_SIZE; j++)
            {
                result.Add(new IncomeExpenseReport
                {
                    Income = GetAtArray(lineId, manager.m_linesDataLong, (int)LineDataLong.INCOME, j),
                    Expense = GetAtArray(lineId, manager.m_linesDataLong, (int)LineDataLong.EXPENSE, j),
                    RefFrame = manager.GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new IncomeExpenseReport
            {
                Income = GetAtArray(lineId, manager.m_linesDataLong, (int)LineDataLong.INCOME, CYCLES_CURRENT_DATA_IDX),
                Expense = GetAtArray(lineId, manager.m_linesDataLong, (int)LineDataLong.EXPENSE, CYCLES_CURRENT_DATA_IDX),
                RefFrame = (Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }
        public static List<StudentsTouristsReport> GetLineStudentTouristsTotalReport(this ITMTransportLineStatusesManager manager, ushort lineId)
        {
            var result = new List<StudentsTouristsReport>();
            for (int j = 0; j < CYCLES_HISTORY_SIZE; j++)
            {
                result.Add(new StudentsTouristsReport
                {
                    Total = GetAtArray(lineId, manager.m_linesDataInt, (int)LineDataSmallInt.TOTAL_PASSENGERS, j),
                    Student = GetAtArray(lineId, manager.m_linesDataInt, (int)LineDataSmallInt.STUDENT_PASSENGERS, j),
                    Tourists = GetAtArray(lineId, manager.m_linesDataInt, (int)LineDataSmallInt.TOURIST_PASSENGERS, j),
                    RefFrame = manager.GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new StudentsTouristsReport
            {
                Total = GetAtArray(lineId, manager.m_linesDataInt, (int)LineDataSmallInt.TOTAL_PASSENGERS, CYCLES_CURRENT_DATA_IDX),
                Student = GetAtArray(lineId, manager.m_linesDataInt, (int)LineDataSmallInt.STUDENT_PASSENGERS, CYCLES_CURRENT_DATA_IDX),
                Tourists = GetAtArray(lineId, manager.m_linesDataInt, (int)LineDataSmallInt.TOURIST_PASSENGERS, CYCLES_CURRENT_DATA_IDX),
                RefFrame = (Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }
        public static List<WealthPassengerReport> GetLineWealthReport(this ITMTransportLineStatusesManager manager, ushort lineId)
        {
            var result = new List<WealthPassengerReport>();
            for (int j = 0; j < CYCLES_HISTORY_SIZE; j++)
            {
                result.Add(new WealthPassengerReport
                {
                    Low = LowWealthData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, j)).Sum(x => x),
                    Medium = MedWealthData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, j)).Sum(x => x),
                    High = HghWealthData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, j)).Sum(x => x),
                    RefFrame = manager.GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new WealthPassengerReport
            {
                Low = LowWealthData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                Medium = MedWealthData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                High = HghWealthData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                RefFrame = (Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }
        public static List<AgePassengerReport> GetLineAgeReport(this ITMTransportLineStatusesManager manager, ushort lineId)
        {
            var result = new List<AgePassengerReport>();
            for (int j = 0; j < CYCLES_HISTORY_SIZE; j++)
            {
                result.Add(new AgePassengerReport
                {
                    Child = ChildData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, j)).Sum(x => x),
                    Teen = TeenData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, j)).Sum(x => x),
                    Young = YoungData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, j)).Sum(x => x),
                    Adult = AdultData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, j)).Sum(x => x),
                    Elder = ElderData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, j)).Sum(x => x),
                    RefFrame = manager.GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new AgePassengerReport
            {
                Child = ChildData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                Teen = TeenData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                Young = YoungData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                Adult = AdultData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                Elder = ElderData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                RefFrame = (Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }
        public static List<GenderPassengerReport> GetLineGenderReport(this ITMTransportLineStatusesManager manager, ushort lineId)
        {
            var result = new List<GenderPassengerReport>();
            for (int j = 0; j < CYCLES_HISTORY_SIZE; j++)
            {
                result.Add(new GenderPassengerReport
                {
                    Male = MaleData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, j)).Sum(x => x),
                    Female = FemaleData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, j)).Sum(x => x),
                    RefFrame = manager.GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new GenderPassengerReport
            {
                Male = MaleData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                Female = FemaleData.Select(x => GetAtArray(lineId, manager.m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                RefFrame = (Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }


        #endregion

        #region Report extraction - Stops
        public static List<IncomeExpenseReport> GetStopFinanceReport(this ITMTransportLineStatusesManager manager, ushort stopId)
        {
            var result = new List<IncomeExpenseReport>();
            for (int j = 0; j < CYCLES_HISTORY_SIZE; j++)
            {
                result.Add(new IncomeExpenseReport
                {
                    Income = GetAtArray(stopId, manager.m_stopDataLong, (int)StopDataLong.INCOME, j),
                    RefFrame = manager.GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new IncomeExpenseReport
            {
                Income = GetAtArray(stopId, manager.m_stopDataLong, (int)StopDataLong.INCOME, CYCLES_CURRENT_DATA_IDX),
                RefFrame = (Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }
        public static List<StudentsTouristsReport> GetStopStudentTouristsTotalReport(this ITMTransportLineStatusesManager manager, ushort stopId)
        {
            var result = new List<StudentsTouristsReport>();
            for (int j = 0; j < CYCLES_HISTORY_SIZE; j++)
            {
                result.Add(new StudentsTouristsReport
                {
                    Total = GetAtArray(stopId, manager.m_stopDataInt, (int)StopDataSmallInt.TOTAL_PASSENGERS, j),
                    Student = GetAtArray(stopId, manager.m_stopDataInt, (int)StopDataSmallInt.STUDENT_PASSENGERS, j),
                    Tourists = GetAtArray(stopId, manager.m_stopDataInt, (int)StopDataSmallInt.TOURIST_PASSENGERS, j),
                    RefFrame = manager.GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new StudentsTouristsReport
            {
                Total = GetAtArray(stopId, manager.m_stopDataInt, (int)StopDataSmallInt.TOTAL_PASSENGERS, CYCLES_CURRENT_DATA_IDX),
                Student = GetAtArray(stopId, manager.m_stopDataInt, (int)StopDataSmallInt.STUDENT_PASSENGERS, CYCLES_CURRENT_DATA_IDX),
                Tourists = GetAtArray(stopId, manager.m_stopDataInt, (int)StopDataSmallInt.TOURIST_PASSENGERS, CYCLES_CURRENT_DATA_IDX),
                RefFrame = (Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }


        #endregion

        #region Report extraction - Vehicles
        public static List<IncomeExpenseReport> GetVehicleFinanceReport(this ITMTransportLineStatusesManager manager, ushort vehicleId)
        {
            var result = new List<IncomeExpenseReport>();
            for (int j = 0; j < CYCLES_HISTORY_SIZE; j++)
            {
                result.Add(new IncomeExpenseReport
                {
                    Income = GetAtArray(vehicleId, manager.m_vehiclesDataLong, (int)VehicleDataLong.INCOME, j),
                    Expense = GetAtArray(vehicleId, manager.m_vehiclesDataLong, (int)VehicleDataLong.EXPENSE, j),
                    RefFrame = manager.GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new IncomeExpenseReport
            {
                Income = GetAtArray(vehicleId, manager.m_vehiclesDataLong, (int)VehicleDataLong.INCOME, CYCLES_CURRENT_DATA_IDX),
                Expense = GetAtArray(vehicleId, manager.m_vehiclesDataLong, (int)VehicleDataLong.EXPENSE, CYCLES_CURRENT_DATA_IDX),
                RefFrame = (Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }
        public static List<StudentsTouristsReport> GetVehicleStudentTouristsTotalReport(this ITMTransportLineStatusesManager manager, ushort vehicleId)
        {
            var result = new List<StudentsTouristsReport>();
            for (int j = 0; j < CYCLES_HISTORY_SIZE; j++)
            {
                result.Add(new StudentsTouristsReport
                {
                    Total = GetAtArray(vehicleId, manager.m_vehiclesDataInt, (int)VehicleDataSmallInt.TOTAL_PASSENGERS, j),
                    Student = GetAtArray(vehicleId, manager.m_vehiclesDataInt, (int)VehicleDataSmallInt.STUDENT_PASSENGERS, j),
                    Tourists = GetAtArray(vehicleId, manager.m_vehiclesDataInt, (int)VehicleDataSmallInt.TOURIST_PASSENGERS, j),
                    RefFrame = manager.GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new StudentsTouristsReport
            {
                Total = GetAtArray(vehicleId, manager.m_vehiclesDataInt, (int)VehicleDataSmallInt.TOTAL_PASSENGERS, CYCLES_CURRENT_DATA_IDX),
                Student = GetAtArray(vehicleId, manager.m_vehiclesDataInt, (int)VehicleDataSmallInt.STUDENT_PASSENGERS, CYCLES_CURRENT_DATA_IDX),
                Tourists = GetAtArray(vehicleId, manager.m_vehiclesDataInt, (int)VehicleDataSmallInt.TOURIST_PASSENGERS, CYCLES_CURRENT_DATA_IDX),
                RefFrame = (Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }
        #endregion
    }
}