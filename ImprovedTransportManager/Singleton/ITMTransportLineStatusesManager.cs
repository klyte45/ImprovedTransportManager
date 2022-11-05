using ColossalFramework;
using Kwytto.Utils;
using System;
using UnityEngine;
using static ImprovedTransportManager.Data.TransportLineStorageBasicData;

namespace ImprovedTransportManager.Singleton
{
    public class ITMTransportLineStatusesManager : MonoBehaviour
    {
        private static ITMTransportLineStatusesManager _instance;
        public static ITMTransportLineStatusesManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = ModInstance.Instance.OwnGO.AddComponent<ITMTransportLineStatusesManager>();
                }
                return _instance;
            }
        }

        public static int BYTES_PER_CYCLE
        {
            get
            {
                if (m_cachedFrameSize != SimulationManager.DAYTIME_FRAMES)
                {
                    m_cachedFrameSize = SimulationManager.DAYTIME_FRAMES;
                    m_cachedBytesPerCycle = Mathf.RoundToInt(Mathf.Log(SimulationManager.DAYTIME_FRAMES) / Mathf.Log(2)) - 4;
                }
                return m_cachedBytesPerCycle;
            }
        }

        public static uint FRAMES_PER_CYCLE => 1u << (BYTES_PER_CYCLE);
        public static uint FRAMES_PER_CYCLE_MASK => FRAMES_PER_CYCLE - 1;
        public static uint TOTAL_STORAGE_CAPACITY => (1u << (BYTES_PER_CYCLE + 4));
        public static uint OFFSET_FRAMES => DayTimeOffsetFrames & FRAMES_PER_CYCLE_MASK;
        public static uint DayTimeOffsetFrames => SimulationManager.instance.m_enableDayNight ? SimulationManager.instance.m_dayTimeOffsetFrames : 0;
        public static uint INDEX_AND_FRAMES_MASK => TOTAL_STORAGE_CAPACITY - 1;
        public const int CYCLES_HISTORY_SIZE = 16;
        public const int CYCLES_HISTORY_MASK = CYCLES_HISTORY_SIZE - 1;
        public const int CYCLES_HISTORY_ARRAY_SIZE = CYCLES_HISTORY_SIZE + 1;
        public const int CYCLES_CURRENT_DATA_IDX = CYCLES_HISTORY_SIZE;

        private static uint m_cachedFrameSize = 0;
        private static int m_cachedBytesPerCycle = 0;

        public void Awake()
        {
            InitArray(ref m_linesDataLong, (int)TransportManager.instance.m_lines.m_size, typeof(LineDataLong));
            InitArray(ref m_vehiclesDataLong, (int)VehicleManager.instance.m_vehicles.m_size, typeof(VehicleDataLong));
            InitArray(ref m_stopDataLong, (int)NetManager.instance.m_nodes.m_size, typeof(StopDataLong));

            InitArray(ref m_linesDataInt, (int)TransportManager.instance.m_lines.m_size, typeof(LineDataSmallInt));
            InitArray(ref m_vehiclesDataInt, (int)VehicleManager.instance.m_vehicles.m_size, typeof(VehicleDataSmallInt));
            InitArray(ref m_stopDataInt, (int)NetManager.instance.m_nodes.m_size, typeof(StopDataSmallInt));

            InitArray(ref m_linesDataUshort, (int)TransportManager.instance.m_lines.m_size, typeof(LineDataUshort));
        }

        private void InitArray<T>(ref T[][] array, int size, Type enumType) where T : struct, IConvertible
        {
            array = new T[CYCLES_HISTORY_ARRAY_SIZE * size][];
            for (int k = 0; k < array.Length; k++)
            {
                array[k] = new T[Enum.GetValues(enumType).Length];
            }
        }

        #region Data feeding
        public void AddToLine(ushort lineId, long income, long expense, ref Citizen citizenData, ushort citizenId)
        {
            IncrementInArray(lineId, ref m_linesDataLong, ref m_linesDataInt, (int)LineDataLong.INCOME, (int)LineDataLong.EXPENSE, (int)LineDataSmallInt.TOTAL_PASSENGERS, (int)LineDataSmallInt.TOURIST_PASSENGERS, (int)LineDataSmallInt.STUDENT_PASSENGERS, income, expense, ref citizenData, citizenId);
            if (citizenId != 0)
            {
                int idxW = ((((int)citizenData.WealthLevel * 5) + (int)Citizen.GetAgeGroup(citizenData.m_age)) << 1) + (int)Citizen.GetGender(citizenId);
                m_linesDataUshort[(lineId * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][idxW]++;
            }
        }

        public void AddToVehicle(ushort vehicleId, long income, long expense, ref Citizen citizenData, ushort citizenId) => IncrementInArray(vehicleId, ref m_vehiclesDataLong, ref m_vehiclesDataInt, (int)VehicleDataLong.INCOME, (int)VehicleDataLong.EXPENSE, (int)VehicleDataSmallInt.TOTAL_PASSENGERS, (int)VehicleDataSmallInt.TOURIST_PASSENGERS, (int)VehicleDataSmallInt.STUDENT_PASSENGERS, income, expense, ref citizenData, citizenId);
        public void AddToStop(ushort stopId, long income, ref Citizen citizenData, ushort citizenId) => IncrementInArray(stopId, ref m_stopDataLong, ref m_stopDataInt, (int)StopDataLong.INCOME, null, (int)StopDataSmallInt.TOTAL_PASSENGERS, (int)StopDataSmallInt.TOURIST_PASSENGERS, (int)StopDataSmallInt.STUDENT_PASSENGERS, income, 0, ref citizenData, citizenId);

        private void IncrementInArray(ushort id, ref long[][] arrayRef, ref int[][] arrayRefInt, int incomeIdx, int? expenseIdx, int totalPassIdx, int tourPassIdx, int studPassIdx, long income, long expense, ref Citizen citizenData, ushort citizenId)
        {
            arrayRef[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][incomeIdx] += income;
            if (expenseIdx is int idx)
            {
                arrayRef[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][idx] += expense;
            }
            if (citizenId != 0)
            {
                arrayRefInt[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][totalPassIdx]++;
                if ((citizenData.m_flags & Citizen.Flags.Tourist) != 0)
                {
                    arrayRefInt[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][tourPassIdx]++;
                }

                if ((citizenData.m_flags & Citizen.Flags.Student) != 0)
                {
                    arrayRefInt[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][studPassIdx]++;
                }
            }
        }
        #endregion

        #region Generic Getters Income/Expense

        internal void GetIncomeAndExpensesForLine(ushort lineId, out long income, out long expenses) => GetGenericIncomeExpense(lineId, out income, out expenses, m_linesDataLong, (int)LineDataLong.INCOME, (int)LineDataLong.EXPENSE);

        internal void GetGenericIncomeExpense(ushort id, out long income, out long expenses, long[][] arrayData, int incomeEntry, int expenseEntry)
        {
            income = 0L;
            expenses = 0L;
            for (int j = 0; j <= CYCLES_HISTORY_SIZE; j++)
            {
                income += GetAtArray(id, arrayData, incomeEntry, j);
                expenses += GetAtArray(id, arrayData, expenseEntry, j);
            }
        }

        internal static T GetAtArray<T>(ushort id, T[][] arrayData, int entryIdx, int dataIdx) where T : struct, IComparable => arrayData[(id * 17) + dataIdx][entryIdx];

        internal void GetGenericIncome(ushort id, out long income, long[][] arrayData, int incomeEntry)
        {
            income = 0L;
            for (int j = 0; j <= CYCLES_HISTORY_SIZE; j++)
            {
                income += arrayData[(id * CYCLES_HISTORY_ARRAY_SIZE) + j][incomeEntry];
            }
        }
        #endregion

        #region Specific Income/Expense Getters

        public void GetIncomeAndExpensesForVehicle(ushort vehicleId, out long income, out long expenses) => GetGenericIncomeExpense(vehicleId, out income, out expenses, m_vehiclesDataLong, (int)VehicleDataLong.INCOME, (int)VehicleDataLong.EXPENSE);
        public void GetStopIncome(ushort stopId, out long income) => GetGenericIncome(stopId, out income, m_stopDataLong, (int)StopDataLong.INCOME);

        public void GetCurrentIncomeAndExpensesForLine(ushort lineId, out long income, out long expenses)
        {
            income = GetAtArray(lineId, m_linesDataLong, (int)LineDataLong.INCOME, CYCLES_CURRENT_DATA_IDX);
            expenses = GetAtArray(lineId, m_linesDataLong, (int)LineDataLong.EXPENSE, CYCLES_CURRENT_DATA_IDX);
        }
        public void GetCurrentIncomeAndExpensesForVehicles(ushort vehicleId, out long income, out long expenses)
        {
            income = GetAtArray(vehicleId, m_vehiclesDataLong, (int)VehicleDataLong.INCOME, CYCLES_CURRENT_DATA_IDX);
            expenses = GetAtArray(vehicleId, m_vehiclesDataLong, (int)VehicleDataLong.EXPENSE, CYCLES_CURRENT_DATA_IDX);
        }
        public void GetCurrentStopIncome(ushort stopId, out long income) => income = GetAtArray(stopId, m_stopDataLong, (int)StopDataLong.INCOME, CYCLES_CURRENT_DATA_IDX);

        public void GetLastWeekIncomeAndExpensesForLine(ushort lineId, out long income, out long expenses)
        {
            int lastIdx = ((int)CurrentArrayEntryIdx + CYCLES_HISTORY_SIZE - 1) & CYCLES_HISTORY_MASK;
            income = GetAtArray(lineId, m_linesDataLong, (int)LineDataLong.INCOME, lastIdx);
            expenses = GetAtArray(lineId, m_linesDataLong, (int)LineDataLong.EXPENSE, lastIdx);
        }
        public void GetLastWeekIncomeAndExpensesForVehicles(ushort vehicleId, out long income, out long expenses)
        {
            int lastIdx = ((int)CurrentArrayEntryIdx + CYCLES_HISTORY_SIZE - 1) & CYCLES_HISTORY_MASK;
            income = GetAtArray(vehicleId, m_vehiclesDataLong, (int)VehicleDataLong.INCOME, lastIdx);
            expenses = GetAtArray(vehicleId, m_vehiclesDataLong, (int)VehicleDataLong.EXPENSE, lastIdx);
        }
        public void GetLastWeekStopIncome(ushort stopId, out long income)
        {
            int lastIdx = ((int)CurrentArrayEntryIdx + CYCLES_HISTORY_SIZE - 1) & CYCLES_HISTORY_MASK;
            income = GetAtArray(stopId, m_stopDataLong, (int)StopDataLong.INCOME, lastIdx);
        }
        #endregion

        #region Cycling
        private uint CurrentArrayEntryIdx => ((Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) >> BYTES_PER_CYCLE) & CYCLES_HISTORY_MASK;

        internal long GetStartFrameForArrayIdx(int idx) => ((Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~INDEX_AND_FRAMES_MASK) + (idx << BYTES_PER_CYCLE) - (idx >= CurrentArrayEntryIdx ? TOTAL_STORAGE_CAPACITY : 0);

        public static void SimulationStepImpl(int subStep)
        {
            if (subStep != 0 && subStep != 1000)
            {
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES;
                uint frameCounterCycle = currentFrameIndex & FRAMES_PER_CYCLE_MASK;
                if (frameCounterCycle == 0)
                {
                    currentFrameIndex--;
                    uint idxEnum = (currentFrameIndex >> BYTES_PER_CYCLE) & CYCLES_HISTORY_MASK;
                    LogUtils.DoLog($"Stroring data for frame {currentFrameIndex & ~FRAMES_PER_CYCLE_MASK:X8} into idx {idxEnum:X1}");

                    FinishCycle(idxEnum, ref Instance.m_linesDataLong, TransportManager.MAX_LINE_COUNT);
                    FinishCycle(idxEnum, ref Instance.m_vehiclesDataLong, (int)VehicleManager.instance.m_vehicles.m_size);
                    FinishCycle(idxEnum, ref Instance.m_stopDataLong, NetManager.MAX_NODE_COUNT);
                    FinishCycle(idxEnum, ref Instance.m_linesDataInt, TransportManager.MAX_LINE_COUNT);
                    FinishCycle(idxEnum, ref Instance.m_vehiclesDataInt, (int)VehicleManager.instance.m_vehicles.m_size);
                    FinishCycle(idxEnum, ref Instance.m_stopDataInt, NetManager.MAX_NODE_COUNT);
                    FinishCycle(idxEnum, ref Instance.m_linesDataUshort, TransportManager.MAX_LINE_COUNT);
                }
            }
        }

        private static void FinishCycle<T>(uint idxEnum, ref T[][] arrayRef, int loopSize) where T : struct, IConvertible
        {
            for (int k = 0; k < loopSize; k++)
            {
                int kIdx = (k * CYCLES_HISTORY_ARRAY_SIZE);
                for (int l = 0; l < arrayRef[kIdx].Length; l++)
                {
                    arrayRef[kIdx + idxEnum][l] = arrayRef[kIdx + CYCLES_CURRENT_DATA_IDX][l];
                    arrayRef[kIdx + CYCLES_CURRENT_DATA_IDX][l] = default;
                }
            }
        }

        private static void ClearArray<T>(ref T[][] arrayRef) where T : struct, IComparable
        {
            for (int k = 0; k < arrayRef.Length; k++)
            {
                for (int l = 0; l < arrayRef[k].Length; l++)
                {
                    arrayRef[k][l] = default;
                }
            }
        }


        public static void UpdateData(SimulationManager.UpdateMode mode)
        {
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginLoading("ITMStatistics.UpdateData");
            if (mode == SimulationManager.UpdateMode.NewMap || mode == SimulationManager.UpdateMode.NewGameFromMap || mode == SimulationManager.UpdateMode.NewScenarioFromMap || mode == SimulationManager.UpdateMode.UpdateScenarioFromMap || mode == SimulationManager.UpdateMode.NewAsset)
            {
                ClearArray(ref Instance.m_linesDataLong);
                ClearArray(ref Instance.m_vehiclesDataLong);
                ClearArray(ref Instance.m_stopDataLong);
                ClearArray(ref Instance.m_linesDataInt);
                ClearArray(ref Instance.m_vehiclesDataInt);
                ClearArray(ref Instance.m_stopDataInt);
                ClearArray(ref Instance.m_linesDataUshort);
            }
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndLoading();
        }
        #endregion

        internal long[][] m_linesDataLong;
        internal long[][] m_vehiclesDataLong;
        internal long[][] m_stopDataLong;

        internal int[][] m_linesDataInt;
        internal int[][] m_vehiclesDataInt;
        internal int[][] m_stopDataInt;

        internal ushort[][] m_linesDataUshort;

        internal void DoWithArray(Enum e, DoWithArrayRef<long> action, DoWithArrayRef<int> actionInt, DoWithArrayRef<ushort> actionUshort)
        {
            switch (e)
            {
                case LineDataLong _:
                    action(ref m_linesDataLong);
                    break;
                case VehicleDataLong _:
                    action(ref m_vehiclesDataLong);
                    break;
                case StopDataLong _:
                    action(ref m_stopDataLong);
                    break;
                case LineDataSmallInt _:
                    actionInt(ref m_linesDataInt);
                    break;
                case VehicleDataSmallInt _:
                    actionInt(ref m_vehiclesDataInt);
                    break;
                case StopDataSmallInt _:
                    actionInt(ref m_stopDataInt);
                    break;
                case LineDataUshort _:
                    actionUshort(ref m_linesDataUshort);
                    break;
            }
        }

        public const long CURRENT_VERSION = 0;
    }
}